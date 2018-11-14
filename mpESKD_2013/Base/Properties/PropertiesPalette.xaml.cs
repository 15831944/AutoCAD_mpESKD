namespace mpESKD.Base.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Windows;
    using Controls;
    using Converters;
    using Enums;
    using Helpers;
    using ModPlusAPI.Windows;
    using Styles;

    public partial class PropertiesPalette
    {
        public PropertiesPalette()
        {
            InitializeComponent();
            ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources);
            StckMaxObjectsSelectedMessage.Visibility = System.Windows.Visibility.Collapsed;
            AcadHelpers.Documents.DocumentCreated += Documents_DocumentCreated;
            AcadHelpers.Documents.DocumentActivated += Documents_DocumentActivated;

            _enumPropertiesLocalizationValues = new Dictionary<Type, List<string>>();
            _categoryLocalizationNames = new Dictionary<string, string>();

            foreach (Document document in AcadHelpers.Documents)
            {
                document.ImpliedSelectionChanged += Document_ImpliedSelectionChanged;
            }
            if (AcadHelpers.Document != null)
                ShowPropertiesControlsBySelection();
        }

        /// <summary>
        /// Словарь хранения локализованных значений для свойств типа Enum чтобы не читать их из атрибутов много раз
        /// </summary>
        private readonly Dictionary<Type, List<string>> _enumPropertiesLocalizationValues;

        /// <summary>
        /// Словарь хранения локализованных значений имени категории
        /// </summary>
        private readonly Dictionary<string, string> _categoryLocalizationNames;

        private void Documents_DocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            if (e.Document != null)
            {
                e.Document.ImpliedSelectionChanged -= Document_ImpliedSelectionChanged;
                e.Document.ImpliedSelectionChanged += Document_ImpliedSelectionChanged;
            }
        }

        private void Documents_DocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            if (e.Document != null)
            {
                e.Document.ImpliedSelectionChanged -= Document_ImpliedSelectionChanged;
                e.Document.ImpliedSelectionChanged += Document_ImpliedSelectionChanged;
            }
        }

        private void Document_ImpliedSelectionChanged(object sender, EventArgs e)
        {
            ShowPropertiesControlsBySelection();
        }

        /// <summary>Добавление пользовательских элементов в палитру в зависимости от выбранных объектов</summary>
        private void ShowPropertiesControlsBySelection()
        {
            PromptSelectionResult psr = AcadHelpers.Editor.SelectImplied();
            if (psr.Value == null || psr.Value.Count == 0)
            {
                // Удаляем контролы свойств
                if (StackPanelProperties.Children.Count > 0)
                    StackPanelProperties.Children.Clear();
                // Очищаем панель описания
                ShowDescription(string.Empty);
                // hide message
                StckMaxObjectsSelectedMessage.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                if (MainStaticSettings.Settings.MaxSelectedObjects == 0 ||
                    MainStaticSettings.Settings.MaxSelectedObjects >= psr.Value.Count)
                {
                    StckMaxObjectsSelectedMessage.Visibility = System.Windows.Visibility.Collapsed;

                    List<ObjectId> objectIds = new List<ObjectId>();
                    using (OpenCloseTransaction tr = new OpenCloseTransaction())
                    {
                        foreach (SelectedObject selectedObject in psr.Value)
                        {
                            //todo Возможно стоит убрать транзакцию и заменить на id.GetObject или как там
                            var obj = tr.GetObject(selectedObject.ObjectId, OpenMode.ForRead);
                            if (obj is BlockReference)
                            {
                                objectIds.Add(selectedObject.ObjectId);
                            }
                        }
                        tr.Commit();
                    }
                    if (objectIds.Any())
                    {
                        var summaryPropertyData = new SummaryPropertyCollection(objectIds);
                        SetData(summaryPropertyData);
                    }
                }
                else StckMaxObjectsSelectedMessage.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public void SetData(SummaryPropertyCollection collection)
        {
            var entityGroups =
                collection.Where(sp => sp.EntityPropertyDataCollection.Any()).GroupBy(sp => sp.EntityName);

            foreach (IGrouping<string, SummaryProperty> entityGroup in entityGroups)
            {
                var c = entityGroup.SelectMany(sp => sp.EntityPropertyDataCollection).Select(p => p.OwnerObjectId).Distinct().Count();
                Expander entityExpander = new Expander
                {
                    IsExpanded = true,
                    Header = entityGroup.Key + " [" + c + "]"
                };
                Grid mainGrid = new Grid();
                var categoryIndex = 0;
                List<IGrouping<PropertiesCategory, SummaryProperty>> summaryPropertiesGroups = entityGroup.GroupBy(sp => sp.Category).ToList();
                summaryPropertiesGroups.Sort((sp1, sp2) => sp1.Key.CompareTo(sp2.Key));
                foreach (IGrouping<PropertiesCategory, SummaryProperty> summaryPropertiesGroup in summaryPropertiesGroups)
                {
                    mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    Grid grid = new Grid();
                    Grid.SetRow(grid, categoryIndex);

                    RowDefinition headerRowDefinition = new RowDefinition { Height = GridLength.Auto };
                    ColumnDefinition propertyNameColumnDefinition = new ColumnDefinition();
                    propertyNameColumnDefinition.MinWidth = 50;
                    BindingOperations.SetBinding(propertyNameColumnDefinition, WidthProperty, CreateBindingForColumnWidth());

                    ColumnDefinition gridSplitterColumnDefinition = new ColumnDefinition() { Width = GridLength.Auto };
                    ColumnDefinition propertyValueColumnDefinition = new ColumnDefinition();
                    propertyValueColumnDefinition.MinWidth = 50;
                    grid.RowDefinitions.Add(headerRowDefinition);
                    grid.ColumnDefinitions.Add(propertyNameColumnDefinition);
                    grid.ColumnDefinitions.Add(gridSplitterColumnDefinition);
                    grid.ColumnDefinitions.Add(propertyValueColumnDefinition);

                    TextBox categoryHeader = new TextBox
                    {
                        Text = GetCategoryLocalizationName(summaryPropertiesGroup.Key)
                    };
                    Grid.SetRow(categoryHeader, 0);
                    Grid.SetColumn(categoryHeader, 0);
                    Grid.SetColumnSpan(categoryHeader, 3);
                    categoryHeader.Style = Resources["HeaderTextBox"] as Style;
                    grid.Children.Add(categoryHeader);

                    // sort
                    var j = 1;
                    foreach (SummaryProperty summaryProperty in summaryPropertiesGroup.OrderBy(sp => sp.OrderIndex))
                    {
                        RowDefinition rowDefinition = new RowDefinition() { Height = GridLength.Auto };
                        grid.RowDefinitions.Add(rowDefinition);

                        // property name
                        TextBox propertyHeader = new TextBox();
                        propertyHeader.Text = GetPropertyDisplayName(summaryProperty);
                        propertyHeader.Style = Resources["PropertyNameTextBoxBase"] as Style;
                        propertyHeader.Tag = GetPropertyDescription(summaryProperty);
                        propertyHeader.GotFocus += _OnGotFocus;
                        propertyHeader.LostFocus += _OnLostFocus;
                        Grid.SetColumn(propertyHeader, 0);
                        Grid.SetRow(propertyHeader, j);
                        grid.Children.Add(propertyHeader);

                        IntellectualEntityProperty intellectualEntityProperty = summaryProperty.EntityPropertyDataCollection.FirstOrDefault();

                        //todo add tags
                        if (intellectualEntityProperty != null)
                        {
                            if (summaryProperty.PropertyName == "Scale")
                            {
                                try
                                {
                                    ComboBox cb = new ComboBox();
                                    Grid.SetColumn(cb, 2);
                                    Grid.SetRow(cb, j);
                                    cb.ItemsSource = AcadHelpers.Scales;
                                    cb.Style = Resources["PropertyValueComboBox"] as Style;
                                    BindingOperations.SetBinding(cb, ComboBox.TextProperty, CreateTwoWayBindingForProperty(summaryProperty));
                                    grid.Children.Add(cb);
                                }
                                catch (Exception exception)
                                {
                                    ExceptionBox.Show(exception);
                                }
                            }
                            else if (summaryProperty.PropertyName == "Style")
                            {
                                try
                                {
                                    ComboBox cb = new ComboBox();
                                    Grid.SetColumn(cb, 2);
                                    Grid.SetRow(cb, j);
                                    cb.ItemsSource = StyleManager.GetStyles(intellectualEntityProperty.EntityType.Name + "Style")
                                        .Select(s => s.Name);
                                    cb.Style = Resources["PropertyValueComboBox"] as Style;
                                    BindingOperations.SetBinding(cb, ComboBox.TextProperty, CreateTwoWayBindingForProperty(summaryProperty));
                                    grid.Children.Add(cb);
                                }
                                catch (Exception exception)
                                {
                                    ExceptionBox.Show(exception);
                                }
                            }
                            else if (summaryProperty.PropertyName == "LineType")
                            {
                                try
                                {
                                    TextBox tb = new TextBox();
                                    Grid.SetColumn(tb, 2);
                                    Grid.SetRow(tb, j);
                                    tb.Cursor = Cursors.Hand;
                                    tb.Style = Resources["PropertyValueTextBox"] as Style;
                                    tb.PreviewMouseDown += LineType_OnPreviewMouseDown;
                                    BindingOperations.SetBinding(tb, TextBox.TextProperty, CreateTwoWayBindingForProperty(summaryProperty));
                                    grid.Children.Add(tb);
                                }
                                catch (Exception exception)
                                {
                                    ExceptionBox.Show(exception);
                                }
                            }
                            else if (intellectualEntityProperty.Value is Enum)
                            {
                                try
                                {
                                    ComboBox cb = new ComboBox();
                                    Grid.SetColumn(cb, 2);
                                    Grid.SetRow(cb, j);
                                    cb.Style = Resources["PropertyValueComboBox"] as Style;
                                    var type = intellectualEntityProperty.Value.GetType();

                                    // При первом чтении локализованных значений для свойства типа Enum
                                    // запоминаю прочитанные значения в словарь, чтобы в следующий раз не читать повторно
                                    if (_enumPropertiesLocalizationValues.ContainsKey(type))
                                        cb.ItemsSource = _enumPropertiesLocalizationValues[type];
                                    else
                                    {
                                        List<string> enumPropertyLocalizationValues = new List<string>();
                                        foreach (FieldInfo fieldInfo in type.GetFields().Where(f => f.GetCustomAttribute<EnumPropertyDisplayValueKeyAttribute>() != null))
                                        {
                                            var attr = fieldInfo.GetCustomAttribute<EnumPropertyDisplayValueKeyAttribute>();
                                            if (attr != null)
                                            {
                                                enumPropertyLocalizationValues.Add(ModPlusAPI.Language.GetItem(MainFunction.LangItem, attr.LocalizationKey));
                                            }
                                        }
                                        _enumPropertiesLocalizationValues.Add(type, enumPropertyLocalizationValues);
                                        cb.ItemsSource = enumPropertyLocalizationValues;
                                    }

                                    BindingOperations.SetBinding(cb, ComboBox.TextProperty,
                                        CreateTwoWayBindingForProperty(summaryProperty, new EnumPropertyValueConverter()));

                                    grid.Children.Add(cb);
                                }
                                catch (Exception exception)
                                {
                                    ExceptionBox.Show(exception);
                                }
                            }
                            else if (intellectualEntityProperty.Value is int)
                            {
                                try
                                {
                                    IntTextBox tb = new IntTextBox();
                                    Grid.SetColumn(tb, 2);
                                    Grid.SetRow(tb, j);
                                    tb.Minimum = summaryProperty.EntityPropertyDataCollection.Select(p => p.Minimum).Cast<int>().Max();
                                    tb.Maximum = summaryProperty.EntityPropertyDataCollection.Select(p => p.Maximum).Cast<int>().Min();
                                    tb.Style = Resources["PropertyValueIntTextBox"] as Style;
                                    BindingOperations.SetBinding(tb, IntTextBox.ValueProperty, CreateTwoWayBindingForProperty(summaryProperty));

                                    grid.Children.Add(tb);
                                }
                                catch (Exception exception)
                                {
                                    ExceptionBox.Show(exception);
                                }
                            }
                            else if (intellectualEntityProperty.Value is double)
                            {
                                try
                                {
                                    DoubleTextBox tb = new DoubleTextBox();
                                    Grid.SetColumn(tb, 2);
                                    Grid.SetRow(tb, j);
                                    tb.Minimum = summaryProperty.EntityPropertyDataCollection.Select(p => p.Minimum).Cast<double>().Max();
                                    tb.Maximum = summaryProperty.EntityPropertyDataCollection.Select(p => p.Maximum).Cast<double>().Min();
                                    tb.Style = Resources["PropertyValueDoubleTextBox"] as Style;
                                    BindingOperations.SetBinding(tb, DoubleTextBox.ValueProperty, CreateTwoWayBindingForProperty(summaryProperty));

                                    grid.Children.Add(tb);
                                }
                                catch (Exception exception)
                                {
                                    ExceptionBox.Show(exception);
                                }
                            }
                            j++;
                        }
                    }

                    grid.Children.Add(CreateGridSplitter(j));

                    mainGrid.Children.Add(grid);

                    categoryIndex++;
                }

                entityExpander.Content = mainGrid;

                StackPanelProperties.Children.Add(entityExpander);
            }
        }

        private void _OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
                ShowDescription(element.Tag.ToString());
        }

        private void _OnLostFocus(object sender, RoutedEventArgs e)
        {
            ShowDescription(string.Empty);
        }

        private string GetCategoryLocalizationName(PropertiesCategory category)
        {
            if (_categoryLocalizationNames.ContainsKey(category.ToString()))
                return _categoryLocalizationNames[category.ToString()];

            var type = category.GetType();
            foreach (FieldInfo fieldInfo in type.GetFields().Where(f => f.GetCustomAttribute<EnumPropertyDisplayValueKeyAttribute>() != null))
            {
                var attr = fieldInfo.GetCustomAttribute<EnumPropertyDisplayValueKeyAttribute>();
                if (attr != null)
                {
                    try
                    {
                        var localName = ModPlusAPI.Language.GetItem(MainFunction.LangItem, attr.LocalizationKey);
                        if (!_categoryLocalizationNames.ContainsKey(category.ToString()))
                            _categoryLocalizationNames.Add(category.ToString(), localName);
                        return localName;
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            return category.ToString();
        }

        private string GetPropertyDisplayName(SummaryProperty summaryProperty)
        {
            try
            {
                var displayName = ModPlusAPI.Language.GetItem(MainFunction.LangItem, summaryProperty.DisplayNameLocalizationKey);
                if (!string.IsNullOrEmpty(displayName))
                    return displayName;
            }
            catch
            {
                // ignore
            }

            return string.Empty;
        }

        private string GetPropertyDescription(SummaryProperty summaryProperty)
        {
            try
            {
                var description = ModPlusAPI.Language.GetItem(MainFunction.LangItem, summaryProperty.DescriptionLocalizationKey);
                if (!string.IsNullOrEmpty(description))
                    return description;
            }
            catch
            {
                // ignore
            }

            return string.Empty;
        }

        private GridSplitter CreateGridSplitter(int rowSpan)
        {
            GridSplitter gridSplitter = new GridSplitter
            {
                BorderThickness = new Thickness(2, 0, 0, 0),
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#FF696969"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Grid.SetColumn(gridSplitter, 1);
            Grid.SetRow(gridSplitter, 1);
            Grid.SetRowSpan(gridSplitter, rowSpan);
            return gridSplitter;
        }

        private Binding CreateTwoWayBindingForProperty(SummaryProperty summaryProperty, IValueConverter converter = null)
        {
            Binding binding = new Binding
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Source = summaryProperty,
                Path = new PropertyPath("SummaryValue")
            };
            if (converter != null)
                binding.Converter = converter;
            return binding;
        }

        private Binding CreateBindingForColumnWidth()
        {
            Binding b = new Binding
            {
                Source = mpESKD.Properties.Settings.Default,
                Path = new PropertyPath(mpESKD.Properties.Settings.Default.GridColumnWidth),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new ColumnWidthConverter()
            };
            return b;
        }

        public void ShowDescription(string description)
        {
            TbDescription.Text = description;
        }

        // set line type
        private void LineType_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            using (AcadHelpers.Document.LockDocument())
            {
                var ltd = new LinetypeDialog { IncludeByBlockByLayer = false };
                if (ltd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!ltd.Linetype.IsNull)
                    {
                        //todo openCloseTransaction or objectId.open()
                        using (var tr = AcadHelpers.Document.TransactionManager.StartTransaction())
                        {
                            using (var ltr = tr.GetObject(ltd.Linetype, OpenMode.ForRead) as LinetypeTableRecord)
                            {
                                if (ltr != null)
                                {
                                    ((TextBox)sender).Text = ltr.Name;
                                }
                            }
                            tr.Commit();
                        }
                    }
                }
            }
        }

        private void LmSettings_OnClick(object sender, RoutedEventArgs e)
        {
            PaletteSettings lmSetting = new PaletteSettings()
            {
                Topmost = true
            };
            lmSetting.ShowDialog();

            if (!lmSetting.ChkAddToMpPalette.IsChecked ?? true)
            {
                MainFunction.RemoveFromMpPalette(true);
            }
            else
            {
                MainFunction.AddToMpPalette(true);
            }
        }

        // open settings
        private void OpenSettings_OnClick(object sender, RoutedEventArgs e)
        {
            if (AcadHelpers.Document != null)
                AcadHelpers.Document.SendStringToExecute("mpStyleEditor ", true, false, false);
        }
    }
}
