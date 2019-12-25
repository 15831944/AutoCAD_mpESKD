namespace mpESKD.Base.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
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

            foreach (Document document in AcadHelpers.Documents)
            {
                document.ImpliedSelectionChanged += Document_ImpliedSelectionChanged;
            }

            if (AcadHelpers.Document != null)
            {
                ShowPropertiesControlsBySelection();
            }
        }

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
            // Удаляем контролы свойств
            if (StackPanelProperties.Children.Count > 0)
            {
                StackPanelProperties.Children.Clear();
            }

            PromptSelectionResult psr = AcadHelpers.Editor.SelectImplied();
            if (psr.Value == null || psr.Value.Count == 0)
            {
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
                    using (AcadHelpers.Document.LockDocument())
                    {
                        using (OpenCloseTransaction tr = new OpenCloseTransaction())
                        {
                            foreach (SelectedObject selectedObject in psr.Value)
                            {
                                if (selectedObject.ObjectId == ObjectId.Null)
                                {
                                    continue;
                                }

                                var obj = tr.GetObject(selectedObject.ObjectId, OpenMode.ForRead);
                                if (obj is BlockReference blockReference &&
                                    ExtendedDataHelpers.IsApplicable(blockReference))
                                {
                                    objectIds.Add(selectedObject.ObjectId);
                                }
                            }

                            tr.Commit();
                        }
                    }

                    if (objectIds.Any())
                    {
                        var summaryPropertyCollection = new SummaryPropertyCollection(objectIds);
                        summaryPropertyCollection.OnLockedLayerEventHandler += delegate
                        {
                            ShowPropertiesControlsBySelection();
                        };
                        SetData(summaryPropertyCollection);
                    }
                }
                else
                {
                    StckMaxObjectsSelectedMessage.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Построение элементов в палитре по данным коллекции свойств
        /// </summary>
        /// <param name="collection"><see cref="SummaryPropertyCollection"/></param>
        public void SetData(SummaryPropertyCollection collection)
        {
            var entityGroups = collection.Where(sp => sp.EntityPropertyDataCollection.Any())
                .GroupBy(sp => sp.EntityType);

            foreach (IGrouping<Type, SummaryProperty> entityGroup in entityGroups)
            {
                // Тип примитива может содержать атрибуты указывающие зависимость видимости свойств
                // Собираю их в список для последующей работы
                var visibilityDependencyAttributes = GetVisibilityDependencyAttributes(entityGroup.Key);
                List<SummaryProperty> allEntitySummaryProperties = entityGroup.Select(g => g).ToList();

                var c = entityGroup.SelectMany(sp => sp.EntityPropertyDataCollection).Select(p => p.OwnerObjectId).Distinct().Count();
                Expander entityExpander = new Expander
                {
                    IsExpanded = true,
                    Header = LocalizationHelper.GetEntityLocalizationName(entityGroup.Key) + " [" + c + "]"
                };

                Grid mainGrid = new Grid();
                var categoryIndex = 0;
                List<IGrouping<PropertiesCategory, SummaryProperty>> summaryPropertiesGroups = entityGroup.GroupBy(sp => sp.Category).ToList();
                summaryPropertiesGroups.Sort((sp1, sp2) => sp1.Key.CompareTo(sp2.Key));

                foreach (IGrouping<PropertiesCategory, SummaryProperty> summaryPropertiesGroup in summaryPropertiesGroups)
                {
                    mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    var grid = new Grid();
                    Grid.SetRow(grid, categoryIndex);

                    var headerRowDefinition = new RowDefinition { Height = GridLength.Auto };
                    var firstColumn = new ColumnDefinition { MinWidth = 50 };
                    BindingOperations.SetBinding(firstColumn, ColumnDefinition.WidthProperty, CreateBindingForColumnWidth());
                    var secondColumn = new ColumnDefinition { Width = GridLength.Auto };
                    var thirdColumn = new ColumnDefinition { MinWidth = 50 };
                    grid.RowDefinitions.Add(headerRowDefinition);
                    grid.ColumnDefinitions.Add(firstColumn);
                    grid.ColumnDefinitions.Add(secondColumn);
                    grid.ColumnDefinitions.Add(thirdColumn);

                    TextBox categoryHeader = new TextBox { Text = LocalizationHelper.GetCategoryLocalizationName(summaryPropertiesGroup.Key) };
                    Grid.SetRow(categoryHeader, 0);
                    Grid.SetColumn(categoryHeader, 0);
                    Grid.SetColumnSpan(categoryHeader, 3);
                    categoryHeader.Style = Resources["HeaderTextBox"] as Style;
                    grid.Children.Add(categoryHeader);

                    // sort
                    var j = 1;
                    foreach (SummaryProperty summaryProperty in summaryPropertiesGroup.OrderBy(sp => sp.OrderIndex))
                    {
                        if (summaryProperty.PropertyScope == PropertyScope.Hidden)
                        {
                            continue;
                        }

                        RowDefinition rowDefinition = new RowDefinition { Height = GridLength.Auto };
                        grid.RowDefinitions.Add(rowDefinition);

                        // property name
                        var propertyDescription = GetPropertyDescription(summaryProperty);
                        var propertyHeader = new TextBox
                        {
                            Text = GetPropertyDisplayName(summaryProperty),
                            Style = Resources["PropertyNameTextBoxBase"] as Style
                        };
                        SetDescription(propertyHeader, propertyDescription);
                        SetVisibilityDependency(visibilityDependencyAttributes, allEntitySummaryProperties, summaryProperty.PropertyName, propertyHeader);
                        Grid.SetColumn(propertyHeader, 0);
                        Grid.SetRow(propertyHeader, j);
                        grid.Children.Add(propertyHeader);

                        IntellectualEntityProperty intellectualEntityProperty = summaryProperty.EntityPropertyDataCollection.FirstOrDefault();

                        if (intellectualEntityProperty != null)
                        {
                            if (summaryProperty.PropertyName == "Style")
                            {
                                try
                                {
                                    ComboBox cb = new ComboBox();
                                    Grid.SetColumn(cb, 2);
                                    Grid.SetRow(cb, j);
                                    cb.ItemsSource = StyleManager.GetStyles(intellectualEntityProperty.EntityType).Select(s => s.Name);
                                    cb.Style = Resources["PropertyValueComboBox"] as Style;
                                    SetDescription(cb, propertyDescription);
                                    SetVisibilityDependency(visibilityDependencyAttributes, allEntitySummaryProperties, summaryProperty.PropertyName, cb);
                                    BindingOperations.SetBinding(cb, ComboBox.TextProperty, CreateTwoWayBindingForProperty(summaryProperty));
                                    grid.Children.Add(cb);
                                }
                                catch (Exception exception)
                                {
                                    ExceptionBox.Show(exception);
                                }
                            }
                            else if (summaryProperty.PropertyName == "LayerName")
                            {
                                try
                                {
                                    ComboBox cb = new ComboBox();
                                    Grid.SetColumn(cb, 2);
                                    Grid.SetRow(cb, j);
                                    cb.ItemsSource = AcadHelpers.Layers;
                                    cb.Style = Resources["PropertyValueComboBox"] as Style;
                                    SetDescription(cb, propertyDescription);
                                    SetVisibilityDependency(visibilityDependencyAttributes, allEntitySummaryProperties, summaryProperty.PropertyName, cb);
                                    BindingOperations.SetBinding(cb, ComboBox.TextProperty, CreateTwoWayBindingForProperty(summaryProperty));
                                    grid.Children.Add(cb);
                                }
                                catch (Exception exception)
                                {
                                    ExceptionBox.Show(exception);
                                }
                            }
                            else if (summaryProperty.PropertyName == "Scale")
                            {
                                try
                                {
                                    ComboBox cb = new ComboBox();
                                    Grid.SetColumn(cb, 2);
                                    Grid.SetRow(cb, j);
                                    cb.ItemsSource = AcadHelpers.Scales;
                                    cb.Style = Resources["PropertyValueComboBox"] as Style;
                                    SetDescription(cb, propertyDescription);
                                    SetVisibilityDependency(visibilityDependencyAttributes, allEntitySummaryProperties, summaryProperty.PropertyName, cb);
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
                                    SetDescription(tb, propertyDescription);
                                    SetVisibilityDependency(visibilityDependencyAttributes, allEntitySummaryProperties, summaryProperty.PropertyName, tb);
                                    BindingOperations.SetBinding(tb, TextBox.TextProperty, CreateTwoWayBindingForProperty(summaryProperty));
                                    grid.Children.Add(tb);
                                }
                                catch (Exception exception)
                                {
                                    ExceptionBox.Show(exception);
                                }
                            }
                            else if (summaryProperty.PropertyName.Contains("TextStyle"))
                            {
                                try
                                {
                                    ComboBox cb = new ComboBox();
                                    Grid.SetColumn(cb, 2);
                                    Grid.SetRow(cb, j);
                                    cb.ItemsSource = AcadHelpers.TextStyles;
                                    cb.Style = Resources["PropertyValueComboBox"] as Style;
                                    SetDescription(cb, propertyDescription);
                                    SetVisibilityDependency(visibilityDependencyAttributes, allEntitySummaryProperties, summaryProperty.PropertyName, cb);
                                    BindingOperations.SetBinding(cb, ComboBox.TextProperty, CreateTwoWayBindingForProperty(summaryProperty));
                                    grid.Children.Add(cb);
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
                                    Type type = intellectualEntityProperty.Value.GetType();
                                    SetDescription(cb, propertyDescription);
                                    SetVisibilityDependency(visibilityDependencyAttributes, allEntitySummaryProperties, summaryProperty.PropertyName, cb);
                                    cb.ItemsSource = LocalizationHelper.GetEnumPropertyLocalizationFields(type);

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
                                    tb.Minimum = summaryProperty.EntityPropertyDataCollection.Select(p => Convert.ToInt32(p.Minimum)).Max();
                                    tb.Maximum = summaryProperty.EntityPropertyDataCollection.Select(p => Convert.ToInt32(p.Maximum)).Min();
                                    tb.Style = Resources["PropertyValueIntTextBox"] as Style;
                                    SetDescription(tb, propertyDescription);
                                    SetVisibilityDependency(visibilityDependencyAttributes, allEntitySummaryProperties, summaryProperty.PropertyName, tb);
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
                                    tb.Minimum = summaryProperty.EntityPropertyDataCollection.Select(p => Convert.ToDouble(p.Minimum)).Max();
                                    tb.Maximum = summaryProperty.EntityPropertyDataCollection.Select(p => Convert.ToDouble(p.Maximum)).Min();
                                    tb.Style = Resources["PropertyValueDoubleTextBox"] as Style;
                                    SetDescription(tb, propertyDescription);
                                    SetVisibilityDependency(visibilityDependencyAttributes, allEntitySummaryProperties, summaryProperty.PropertyName, tb);
                                    BindingOperations.SetBinding(tb, DoubleTextBox.ValueProperty, CreateTwoWayBindingForProperty(summaryProperty));

                                    grid.Children.Add(tb);
                                }
                                catch (Exception exception)
                                {
                                    ExceptionBox.Show(exception);
                                }
                            }
                            else if (intellectualEntityProperty.Value is bool)
                            {
                                try
                                {
                                    CheckBox chb = new CheckBox();
                                    chb.Style = Resources["PropertyValueCheckBox"] as Style;
                                    SetDescription(chb, propertyDescription);
                                    SetVisibilityDependency(visibilityDependencyAttributes, allEntitySummaryProperties, summaryProperty.PropertyName, chb);
                                    BindingOperations.SetBinding(chb, ToggleButton.IsCheckedProperty, CreateTwoWayBindingForProperty(summaryProperty));

                                    Border outterBorder = new Border();
                                    outterBorder.Style = Resources["BorderForValueCheckBox"] as Style;
                                    Grid.SetColumn(outterBorder, 2);
                                    Grid.SetRow(outterBorder, j);

                                    outterBorder.Child = chb;
                                    grid.Children.Add(outterBorder);
                                }
                                catch (Exception exception)
                                {
                                    ExceptionBox.Show(exception);
                                }
                            }
                            else if (intellectualEntityProperty.Value is string)
                            {
                                try
                                {
                                    TextBox tb = new TextBox();
                                    Grid.SetColumn(tb, 2);
                                    Grid.SetRow(tb, j);
                                    tb.Style = Resources["PropertyValueTextBox"] as Style;
                                    SetDescription(tb, propertyDescription);
                                    SetVisibilityDependency(visibilityDependencyAttributes, allEntitySummaryProperties, summaryProperty.PropertyName, tb);
                                    BindingOperations.SetBinding(tb, TextBox.TextProperty, CreateTwoWayBindingForProperty(summaryProperty));

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

        /// <summary>
        /// Добавление описания свойства в тэг элемента и подписывание на события
        /// </summary>
        /// <param name="e">Элемент</param>
        /// <param name="description">Описание свойства</param>
        private void SetDescription(FrameworkElement e, string description)
        {
            e.Tag = description;
            e.GotFocus += _OnGotFocus;
            e.LostFocus += _OnLostFocus;
        }

        /// <summary>
        /// Установка зависимости видимости в случае, если имеется специальный атрибут
        /// </summary>
        /// <param name="visibilityDependencyAttributes">Список атрибутов зависимостей видимости для читаемого типа примитива</param>
        /// <param name="element">Элемент палитры</param>
        /// <param name="allEntitySummaryProperties">Список всех свойств примитива</param>
        /// <param name="propertyName">Имя свойства, которое отображается текущим элементом палитры (заголовок или значение)</param>
        private void SetVisibilityDependency(
            Dictionary<string, PropertyVisibilityDependencyAttribute> visibilityDependencyAttributes,
            List<SummaryProperty> allEntitySummaryProperties, string propertyName,
            FrameworkElement element)
        {
            try
            {
                KeyValuePair<string, PropertyVisibilityDependencyAttribute> attribute =
                    visibilityDependencyAttributes.FirstOrDefault(a => a.Value.DependencyProperties.Contains(propertyName));
                if (attribute.Key != null)
                {
                    Binding binding = new Binding
                    {
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Source = allEntitySummaryProperties.FirstOrDefault(sp => sp.PropertyName == attribute.Key),
                        Path = new PropertyPath("SummaryValue"),
                        Converter = new ModPlusStyle.Converters.BooleanToVisibilityConverter()
                    };
                    BindingOperations.SetBinding(element, FrameworkElement.VisibilityProperty, binding);
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        private Dictionary<string, PropertyVisibilityDependencyAttribute> GetVisibilityDependencyAttributes(Type entityType)
        {
            Dictionary<string, PropertyVisibilityDependencyAttribute> dictionary = new Dictionary<string, PropertyVisibilityDependencyAttribute>();
            foreach (PropertyInfo propertyInfo in entityType.GetProperties())
            {
                var attribute = propertyInfo.GetCustomAttribute<PropertyVisibilityDependencyAttribute>();
                if (attribute != null)
                {
                    dictionary.Add(propertyInfo.Name, attribute);
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Отображение описания свойства при получении элементом фокуса
        /// </summary>
        private void _OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                ShowDescription(element.Tag.ToString());
            }
        }

        /// <summary>
        /// Очистка поля вывода описания свойства при пропадании фокуса с элемента
        /// </summary>
        private void _OnLostFocus(object sender, RoutedEventArgs e)
        {
            ShowDescription(string.Empty);
        }

        /// <summary>
        /// Получение локализованного (отображаемого) имени свойства
        /// </summary>
        /// <param name="summaryProperty">Суммарное свойство</param>
        private string GetPropertyDisplayName(SummaryProperty summaryProperty)
        {
            try
            {
                var displayName = ModPlusAPI.Language.GetItem(Invariables.LangItem, summaryProperty.DisplayNameLocalizationKey);
                if (!string.IsNullOrEmpty(displayName))
                {
                    return displayName;
                }
            }
            catch
            {
                // ignore
            }

            return string.Empty;
        }

        /// <summary>
        /// Получение локализованного описания свойства
        /// </summary>
        /// <param name="summaryProperty">Суммарное свойство</param>
        private string GetPropertyDescription(SummaryProperty summaryProperty)
        {
            try
            {
                var description = ModPlusAPI.Language.GetItem(Invariables.LangItem, summaryProperty.DescriptionLocalizationKey);
                if (!string.IsNullOrEmpty(description))
                {
                    return description;
                }
            }
            catch
            {
                // ignore
            }

            return string.Empty;
        }

        /// <summary>
        /// Добавление в Grid разделителя GridSplitter
        /// </summary>
        /// <param name="rowSpan">Количество пересекаемых строк</param>
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

        /// <summary>
        /// Создание двусторонней привязки к суммарному свойству
        /// </summary>
        /// <param name="summaryProperty">Суммарное свойство</param>
        /// <param name="converter">Конвертер (при необходимости)</param>
        /// <returns>Объект типа <see cref="Binding"/></returns>
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
            {
                binding.Converter = converter;
            }

            return binding;
        }

        /// <summary>
        /// Создание привязки для первой колонки в Grid. Позволяет менять ширину сразу всех колонок в текущем UserControl
        /// </summary>
        /// <returns>Объект типа <see cref="Binding"/></returns>
        private Binding CreateBindingForColumnWidth()
        {
            Binding b = new Binding
            {
                Source = mpESKD.Properties.Settings.Default,
                Path = new PropertyPath("GridColumnWidth"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new ColumnWidthConverter()
            };
            return b;
        }

        /// <summary>
        /// Отобразить описание свойства в специальном поле палитры
        /// </summary>
        /// <param name="description">Описание свойства</param>
        public void ShowDescription(string description)
        {
            TbDescription.Text = description;
        }

        /// <summary>
        /// Отображение диалогового окна AutoCAD с выбором типа линии
        /// </summary>
        private void LineType_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            using (AcadHelpers.Document.LockDocument())
            {
                var ltd = new LinetypeDialog { IncludeByBlockByLayer = false };
                if (ltd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!ltd.Linetype.IsNull)
                    {
                        using (var tr = AcadHelpers.Document.TransactionManager.StartOpenCloseTransaction())
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
            {
                AcadHelpers.Document.SendStringToExecute("mpStyleEditor ", true, false, false);
            }
        }
    }
}
