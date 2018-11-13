namespace mpESKD.Functions.mpGroundLine.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Base.Enums;
    using Base.Helpers;
    using Base.Properties;
    using Base.Properties.Controls;
    using Base.Properties.Converters;
    using ModPlusAPI.Windows;

    /// <summary>
    /// Логика взаимодействия для GroundLinePropertiesPaletteTest.xaml
    /// </summary>
    public partial class GroundLinePropertiesPaletteTest : UserControl
    {
        private readonly PropertiesPalette _parentPalette;

        public GroundLinePropertiesPaletteTest(PropertiesPalette palette)
        {
            _parentPalette = palette;

            InitializeComponent();

            if (AcadHelpers.Document != null)
            {
                ShowProperties();
                AcadHelpers.Document.ImpliedSelectionChanged += Document_ImpliedSelectionChanged;
            }
        }

        private SummaryPropertyData _summaryPropertyData;

        private void Document_ImpliedSelectionChanged(object sender, EventArgs e)
        {
            ShowProperties();
        }

        private void ShowProperties()
        {
            PromptSelectionResult psr = AcadHelpers.Editor.SelectImplied();

            if (psr.Status != PromptStatus.OK || psr.Value == null || psr.Value.Count == 0)
            {
                _summaryPropertyData = null;
            }
            else
            {
                List<ObjectId> objectIds = new List<ObjectId>();
                //todo Возможно стоит убрать транзакцию и заменить на id.GetObject или как там
                using (OpenCloseTransaction tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    foreach (SelectedObject selectedObject in psr.Value)
                    {
                        var obj = tr.GetObject(selectedObject.ObjectId, OpenMode.ForRead);
                        if (obj is BlockReference)
                        {
                            if (ExtendedDataHelpers.IsApplicable(obj, GroundLineFunction.MPCOEntName))
                            {
                                objectIds.Add(selectedObject.ObjectId);
                            }
                        }
                    }
                    tr.Commit();
                }
                if (objectIds.Any())
                {
                    //Expander.Header = GroundLineFunction.MPCOEntDisplayName + " (" + objectIds.Count + ")";
                    _summaryPropertyData = new SummaryPropertyData(objectIds);
                    SetData(_summaryPropertyData);
                }
            }
        }

        public void SetData(SummaryPropertyData data)
        {
            List<IGrouping<PropertiesCategory, SummaryProperty>> summaryPropertiesGroups = data.Where(sp => sp.EntityPropertyDataCollection.Any()).GroupBy(sp => sp.Category).ToList();

            for (var i = 0; i < summaryPropertiesGroups.Count; i++)
            {
                IGrouping<PropertiesCategory, SummaryProperty> summaryPropertiesGroup = summaryPropertiesGroups[i];

                RowDefinition mainGridRowDefinition = new RowDefinition { Height = GridLength.Auto };
                MainGrid.RowDefinitions.Add(mainGridRowDefinition);

                Grid grid = new Grid();
                Grid.SetRow(grid, i);

                RowDefinition headerRowDefinition = new RowDefinition() { Height = GridLength.Auto };
                ColumnDefinition propertyNameColumnDefinition = new ColumnDefinition();
                propertyNameColumnDefinition.MinWidth = 50;
                Binding b = new Binding();
                b.Source = mpESKD.Properties.Settings.Default;
                b.Path = new PropertyPath("GridColumnWidth");
                b.Mode = BindingMode.TwoWay;
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                b.Converter = new ColumnWidthConverter();
                BindingOperations.SetBinding(propertyNameColumnDefinition, Grid.WidthProperty, b);

                ColumnDefinition gridSplitterColumnDefinition = new ColumnDefinition() { Width = GridLength.Auto };
                ColumnDefinition propertyValueColumnDefinition = new ColumnDefinition();
                grid.RowDefinitions.Add(headerRowDefinition);
                grid.ColumnDefinitions.Add(propertyNameColumnDefinition);
                grid.ColumnDefinitions.Add(gridSplitterColumnDefinition);
                grid.ColumnDefinitions.Add(propertyValueColumnDefinition);

                TextBox categoryHeader = new TextBox
                {
                    //todo localization
                    Text = summaryPropertiesGroup.Key.ToString()
                };
                Grid.SetRow(categoryHeader, 0);
                Grid.SetColumn(categoryHeader, 0);
                Grid.SetColumnSpan(categoryHeader, 3);
                categoryHeader.Style = Resources["HeaderTextBox"] as Style;
                grid.Children.Add(categoryHeader);

                var j = 1;
                foreach (SummaryProperty summaryProperty in summaryPropertiesGroup)
                {
                    RowDefinition rowDefinition = new RowDefinition() { Height = GridLength.Auto };
                    grid.RowDefinitions.Add(rowDefinition);
                    TextBox propertyHeader = new TextBox();
                    propertyHeader.Text = ModPlusAPI.Language.GetItem(MainFunction.LangItem, summaryProperty.EntityPropertyDataCollection.First().DisplayNameLocalizationKey);
                    propertyHeader.Style = Resources["PropertyNameTextBoxBase"] as Style;
                    Grid.SetColumn(propertyHeader, 0);
                    Grid.SetRow(propertyHeader, j);
                    grid.Children.Add(propertyHeader);

                    // value
                    if (summaryProperty.EntityPropertyDataCollection.First().Value is int)
                    {
                        try
                        {
                            IntTextBox intTextBox = new IntTextBox();
                            Grid.SetColumn(intTextBox, 2);
                            Grid.SetRow(intTextBox, j);
                            intTextBox.Minimum = summaryProperty.EntityPropertyDataCollection.Select(p => p.Minimum).Cast<int>().Max();
                            intTextBox.Maximum = summaryProperty.EntityPropertyDataCollection.Select(p => p.Maximum).Cast<int>().Min();
                            intTextBox.Style = Resources["PropertyValueIntTextBox"] as Style;
                            Binding intB = new Binding();
                            intB.Mode = BindingMode.TwoWay;
                            intB.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                            intB.Source = summaryProperty;
                            intB.Path = new PropertyPath("SummaryValue");
                            BindingOperations.SetBinding(intTextBox, IntTextBox.ValueProperty, intB);

                            grid.Children.Add(intTextBox);
                        }
                        catch (Exception exception)
                        {
                            ExceptionBox.Show(exception);
                        }
                    }

                    j++;
                }

                GridSplitter gridSplitter = new GridSplitter();
                gridSplitter.BorderThickness = new Thickness(2,0,0,0);
                gridSplitter.BorderBrush = (Brush) new BrushConverter().ConvertFrom("#FF696969");
                gridSplitter.HorizontalAlignment = HorizontalAlignment.Center;
                gridSplitter.VerticalAlignment = VerticalAlignment.Stretch;
                Grid.SetColumn(gridSplitter, 1);
                Grid.SetRow(gridSplitter, 1);
                Grid.SetRowSpan(gridSplitter, j);

                grid.Children.Add(gridSplitter);

                MainGrid.Children.Add(grid);
            }

            DataContext = data;
        }

        private void GroundLinePropertiesPalette_OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (AcadHelpers.Document != null)
                AcadHelpers.Document.ImpliedSelectionChanged -= Document_ImpliedSelectionChanged;
        }
    }
}
