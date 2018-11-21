// Редактор стилей реализую без использования паттерна Mvvm, так как на событиях в данном случае удобней
namespace mpESKD.Base.Styles
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Windows;
    using Enums;
    using Helpers;
    using ModPlusAPI.Windows;
    using ModPlusStyle.Controls;
    using Properties;

    public partial class StyleEditor
    {
        public StyleEditor()
        {
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem(MainFunction.LangItem, "tab4");
            Loaded += StyleEditor_OnLoaded;
            ContentRendered += StyleEditor_ContentRendered;
        }

        private void StyleEditor_OnLoaded(object sender, RoutedEventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
        }

        private ObservableCollection<EntityStyles> _styles;

        private void StyleEditor_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                _styles = new ObservableCollection<EntityStyles>();
                TypeFactory.Instance.GetEntityTypes().ForEach(entityType =>
                {
                    var currentStyleGuidForEntity = StyleManager.GetCurrentStyleGuidForEntity(entityType);
                    var entityStyles = new EntityStyles(entityType);
                    StyleManager.GetStyles(entityType).ForEach(style =>
                    {
                        if (style.Guid == currentStyleGuidForEntity)
                            style.IsCurrent = true;
                        entityStyles.Styles.Add(style);
                    });
                    _styles.Add(entityStyles);
                });
                TvStyles.ItemsSource = _styles;
                if (_styles.Any())
                    BtCreateStyleFromEntity.IsEnabled = true;
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        private void TvStyles_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (BorderProperties.Child != null)
                BorderProperties.Child = null;

            var item = e.NewValue;
            if (item != null)
                BtAddNewStyle.IsEnabled = true;
            if (item is EntityStyles entityStyles)
            {
                BtRemoveStyle.IsEnabled = false;

                // image
                SetImage(entityStyles.EntityType.Name);
            }
            else if (item is IntellectualEntityStyle style)
            {
                BtRemoveStyle.IsEnabled = style.CanEdit;
                BtSetCurrentStyle.IsEnabled = !style.IsCurrent;

                ShowStyleProperties(style);
                SetImage(style.EntityType.Name);
            }
            else SetImage(string.Empty);
        }

        private void ShowStyleProperties(IntellectualEntityStyle style)
        {
            Grid topGrid = new Grid();
            topGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            topGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            topGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            topGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            topGrid.RowDefinitions.Add(new RowDefinition());

            #region Set main data

            TextBlock headerName = new TextBlock
            {
                Margin = (Thickness)FindResource("ModPlusDefaultMargin"),
                Text = ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h54")
            };
            Grid.SetRow(headerName, 0);
            topGrid.Children.Add(headerName);

            TextBox tbName = new TextBox { IsEnabled = style.StyleType == StyleType.User };
            Grid.SetRow(tbName, 1);
            Binding binding = new Binding
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Source = style,
                Path = new PropertyPath("Name")
            };
            BindingOperations.SetBinding(tbName, TextBox.TextProperty, binding);
            topGrid.Children.Add(tbName);

            TextBlock headerDescription = new TextBlock
            {
                Margin = (Thickness)FindResource("ModPlusDefaultMargin"),
                Text = ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h55")
            };
            Grid.SetRow(headerDescription, 2);
            topGrid.Children.Add(headerDescription);

            TextBox tbDescription = new TextBox { IsEnabled = style.StyleType == StyleType.User };
            Grid.SetRow(tbDescription, 3);
            binding = new Binding
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Source = style,
                Path = new PropertyPath("Description")
            };
            BindingOperations.SetBinding(tbDescription, TextBox.TextProperty, binding);
            topGrid.Children.Add(tbDescription);

            #endregion

            Grid propertiesGrid = new Grid();
            propertiesGrid.ColumnDefinitions.Add(new ColumnDefinition());
            propertiesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            propertiesGrid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetRow(propertiesGrid, 4);

            List<IGrouping<PropertiesCategory, IntellectualEntityProperty>> groupsByCategory = style.Properties.GroupBy(p => p.Category).ToList();
            groupsByCategory.Sort((g1, g2) => g1.Key.CompareTo(g2.Key));
            var rowIndex = 0;
            foreach (IGrouping<PropertiesCategory, IntellectualEntityProperty> categoryGroup in groupsByCategory)
            {
                propertiesGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                TextBox categoryHeader = new TextBox { Text = LocalizationHelper.GetCategoryLocalizationName(categoryGroup.Key) };
                Grid.SetRow(categoryHeader, rowIndex);
                Grid.SetColumn(categoryHeader, 0);
                Grid.SetColumnSpan(categoryHeader, 3);
                categoryHeader.Style = Resources["HeaderTextBoxForStyleEditor"] as Style;
                propertiesGrid.Children.Add(categoryHeader);
                rowIndex++;
                var gridSplitterStartIndex = rowIndex;
                foreach (IntellectualEntityProperty property in categoryGroup.OrderBy(p => p.OrderIndex))
                {
                    propertiesGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    // property name
                    var propertyDescription = GetPropertyDescription(property);
                    var propertyHeader = new TextBox
                    {
                        Text = GetPropertyDisplayName(property),
                        Style = Resources["PropertyHeaderInStyleEditor"] as Style
                    };
                    SetDescription(propertyHeader, propertyDescription);
                    Grid.SetColumn(propertyHeader, 0);
                    Grid.SetRow(propertyHeader, rowIndex);
                    propertiesGrid.Children.Add(propertyHeader);

                    if (property.Name == "LayerName")
                    {
                        try
                        {
                            ComboBox cb = new ComboBox { IsEnabled = style.StyleType == StyleType.User };
                            Grid.SetColumn(cb, 2);
                            Grid.SetRow(cb, rowIndex);
                            var layers = AcadHelpers.Layers;
                            layers.Insert(0, ModPlusAPI.Language.GetItem(MainFunction.LangItem, "defl")); // "По умолчанию"
                            if (!layers.Contains(style.GetLayerNameProperty()))
                                layers.Insert(1, style.GetLayerNameProperty());
                            cb.ItemsSource = layers;
                            cb.Style = Resources["PropertyValueComboBoxForStyleEditor"] as Style;
                            SetDescription(cb, propertyDescription);
                            BindingOperations.SetBinding(cb, Selector.SelectedItemProperty, CreateTwoWayBindingForProperty(property));
                            propertiesGrid.Children.Add(cb);
                        }
                        catch (Exception exception)
                        {
                            ExceptionBox.Show(exception);
                        }
                    }
                    else if (property.Name == "Scale")
                    {
                        try
                        {
                            ComboBox cb = new ComboBox { IsEnabled = style.StyleType == StyleType.User };
                            Grid.SetColumn(cb, 2);
                            Grid.SetRow(cb, rowIndex);
                            cb.ItemsSource = AcadHelpers.Scales;
                            cb.Style = Resources["PropertyValueComboBoxForStyleEditor"] as Style;
                            SetDescription(cb, propertyDescription);
                            BindingOperations.SetBinding(cb, ComboBox.TextProperty, CreateTwoWayBindingForProperty(property, new AnnotationScaleValueConverter()));
                            propertiesGrid.Children.Add(cb);
                        }
                        catch (Exception exception)
                        {
                            ExceptionBox.Show(exception);
                        }
                    }
                    else if (property.Name == "LineType")
                    {
                        try
                        {
                            TextBox tb = new TextBox { IsEnabled = style.StyleType == StyleType.User };
                            Grid.SetColumn(tb, 2);
                            Grid.SetRow(tb, rowIndex);
                            tb.Cursor = Cursors.Hand;
                            tb.Style = Resources["PropertyValueTextBoxForStyleEditor"] as Style;
                            tb.PreviewMouseDown += LineType_OnPreviewMouseDown;
                            SetDescription(tb, propertyDescription);
                            BindingOperations.SetBinding(tb, TextBox.TextProperty, CreateTwoWayBindingForProperty(property));
                            propertiesGrid.Children.Add(tb);
                        }
                        catch (Exception exception)
                        {
                            ExceptionBox.Show(exception);
                        }
                    }
                    else if (property.Name.Contains("TextStyle"))
                    {
                        try
                        {
                            ComboBox cb = new ComboBox { IsEnabled = style.StyleType == StyleType.User };
                            Grid.SetColumn(cb, 2);
                            Grid.SetRow(cb, rowIndex);
                            cb.ItemsSource = AcadHelpers.TextStyles;
                            cb.Style = Resources["PropertyValueComboBoxForStyleEditor"] as Style;
                            SetDescription(cb, propertyDescription);
                            BindingOperations.SetBinding(cb, Selector.SelectedItemProperty, CreateTwoWayBindingForProperty(property));
                            propertiesGrid.Children.Add(cb);
                        }
                        catch (Exception exception)
                        {
                            ExceptionBox.Show(exception);
                        }
                    }
                    else if (property.Value is Enum)
                    {
                        try
                        {
                            ComboBox cb = new ComboBox { IsEnabled = style.StyleType == StyleType.User };
                            Grid.SetColumn(cb, 2);
                            Grid.SetRow(cb, rowIndex);
                            cb.Style = Resources["PropertyValueComboBoxForStyleEditor"] as Style;
                            Type type = property.Value.GetType();
                            SetDescription(cb, propertyDescription);
                            cb.ItemsSource = LocalizationHelper.GetEnumPropertyLocalizationFields(type);

                            BindingOperations.SetBinding(cb, ComboBox.TextProperty,
                                CreateTwoWayBindingForProperty(property, new EnumPropertyValueConverter()));

                            propertiesGrid.Children.Add(cb);
                        }
                        catch (Exception exception)
                        {
                            ExceptionBox.Show(exception);
                        }
                    }
                    else if (property.Value is int)
                    {
                        try
                        {
                            NumericBox tb = new NumericBox { IsEnabled = style.StyleType == StyleType.User };
                            Grid.SetColumn(tb, 2);
                            Grid.SetRow(tb, rowIndex);
                            tb.Minimum = Convert.ToInt32(property.Minimum);
                            tb.Maximum = Convert.ToInt32(property.Maximum);
                            tb.Style = Resources["PropertyValueIntTextBoxForStyleEditor"] as Style;
                            SetDescription(tb, propertyDescription);
                            BindingOperations.SetBinding(tb, NumericBox.ValueProperty, CreateTwoWayBindingForPropertyForNumericValue(property, true));
                            propertiesGrid.Children.Add(tb);
                        }
                        catch (Exception exception)
                        {
                            ExceptionBox.Show(exception);
                        }
                    }
                    else if (property.Value is double)
                    {
                        try
                        {
                            NumericBox tb = new NumericBox { IsEnabled = style.StyleType == StyleType.User };
                            Grid.SetColumn(tb, 2);
                            Grid.SetRow(tb, rowIndex);
                            tb.Minimum = Convert.ToDouble(property.Minimum);
                            tb.Maximum = Convert.ToDouble(property.Maximum);
                            tb.Style = Resources["PropertyValueDoubleTextBoxForStyleEditor"] as Style;
                            SetDescription(tb, propertyDescription);
                            BindingOperations.SetBinding(tb, NumericBox.ValueProperty, CreateTwoWayBindingForPropertyForNumericValue(property, false));

                            propertiesGrid.Children.Add(tb);
                        }
                        catch (Exception exception)
                        {
                            ExceptionBox.Show(exception);
                        }
                    }
                    else if (property.Value is bool)
                    {
                        try
                        {
                            CheckBox chb = new CheckBox { IsEnabled = style.StyleType == StyleType.User };
                            SetDescription(chb, propertyDescription);
                            BindingOperations.SetBinding(chb, ToggleButton.IsCheckedProperty, CreateTwoWayBindingForProperty(property));

                            Border outterBorder = new Border();
                            outterBorder.Style = Resources["PropertyBorderForCheckBoxForStyleEditor"] as Style;
                            Grid.SetColumn(outterBorder, 2);
                            Grid.SetRow(outterBorder, rowIndex);

                            outterBorder.Child = chb;
                            propertiesGrid.Children.Add(outterBorder);
                        }
                        catch (Exception exception)
                        {
                            ExceptionBox.Show(exception);
                        }
                    }
                    else if (property.Value is string)
                    {
                        try
                        {
                            TextBox tb = new TextBox { IsEnabled = style.StyleType == StyleType.User };
                            Grid.SetColumn(tb, 2);
                            Grid.SetRow(tb, rowIndex);
                            tb.Style = Resources["PropertyValueTextBoxForStyleEditor"] as Style;
                            SetDescription(tb, propertyDescription);
                            BindingOperations.SetBinding(tb, TextBox.TextProperty, CreateTwoWayBindingForProperty(property));

                            propertiesGrid.Children.Add(tb);
                        }
                        catch (Exception exception)
                        {
                            ExceptionBox.Show(exception);
                        }
                    }

                    rowIndex++;
                }

                propertiesGrid.Children.Add(CreateGridSplitter(gridSplitterStartIndex, rowIndex - gridSplitterStartIndex));
            }

            topGrid.Children.Add(propertiesGrid);
            BorderProperties.Child = topGrid;
        }

        /// <summary>
        /// Добавление в Grid разделителя GridSplitter
        /// </summary>
        /// <param name="startRowIndex">Индекс первой строки</param>
        /// <param name="rowSpan">Количество пересекаемых строк</param>
        private GridSplitter CreateGridSplitter(int startRowIndex, int rowSpan)
        {
            GridSplitter gridSplitter = new GridSplitter
            {
                BorderThickness = new Thickness(2, 0, 0, 0),
                BorderBrush = (Brush)Resources["MidGrayBrush"],
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Grid.SetColumn(gridSplitter, 1);
            Grid.SetRow(gridSplitter, startRowIndex);
            Grid.SetRowSpan(gridSplitter, rowSpan);
            return gridSplitter;
        }

        /// <summary>
        /// Получение локализованного (отображаемого) имени свойства с учётом двух атрибутов
        /// </summary>
        /// <param name="property">Свойство</param>
        private string GetPropertyDisplayName(IntellectualEntityProperty property)
        {
            try
            {
                if (!string.IsNullOrEmpty(property.DisplayNameLocalizationKeyForStyleEditor))
                {
                    var displayName = ModPlusAPI.Language.GetItem(MainFunction.LangItem, property.DisplayNameLocalizationKeyForStyleEditor);
                    if (!string.IsNullOrEmpty(displayName))
                        return displayName;
                }

                {
                    var displayName = ModPlusAPI.Language.GetItem(MainFunction.LangItem, property.DisplayNameLocalizationKey);
                    if (!string.IsNullOrEmpty(displayName))
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
        /// <param name="property">Свойство</param>
        private string GetPropertyDescription(IntellectualEntityProperty property)
        {
            try
            {
                var description = ModPlusAPI.Language.GetItem(MainFunction.LangItem, property.DescriptionLocalizationKey);
                if (!string.IsNullOrEmpty(description))
                    return description;
            }
            catch
            {
                // ignore
            }

            return string.Empty;
        }

        /// <summary>
        /// Создание двусторонней привязки к свойству
        /// </summary>
        /// <param name="entityProperty">Свойство</param>
        /// <param name="converter">Конвертер (при необходимости)</param>
        /// <returns>Объект типа <see cref="Binding"/></returns>
        private Binding CreateTwoWayBindingForProperty(IntellectualEntityProperty entityProperty, IValueConverter converter = null)
        {
            Binding binding = new Binding
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Source = entityProperty,
                Path = new PropertyPath("Value")
            };
            if (converter != null)
                binding.Converter = converter;
            return binding;
        }

        /// <summary>
        /// Создание двусторонней привязки для использования в элементе <see cref="NumericBox"/>
        /// По какой-то причине при привязке к типу object не работает. В связи с этим делаю такой вот
        /// лайфхак - добавляю в класс <see cref="IntellectualEntityProperty"/> два свойства конкретного типа.
        /// Это нужно, чтобы решить эту специфическую проблему в данном проекте и не менять из-за этого
        /// библиотеку оформления
        /// </summary>
        /// <param name="entityProperty"></param>
        /// <param name="isInteger"></param>
        /// <returns></returns>
        private Binding CreateTwoWayBindingForPropertyForNumericValue(IntellectualEntityProperty entityProperty, bool isInteger)
        {
            Binding binding = new Binding
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Source = entityProperty,
                Path = isInteger ? new PropertyPath("IntValue") : new PropertyPath("DoubleValue")
            };
            return binding;
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
        /// Отображение описания свойства при получении элементом фокуса
        /// </summary>
        private void _OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
                ShowDescription(element.Tag.ToString());
        }

        /// <summary>
        /// Очистка поля вывода описания свойства при пропадании фокуса с элемента
        /// </summary>
        private void _OnLostFocus(object sender, RoutedEventArgs e)
        {
            ShowDescription(string.Empty);
        }

        /// <summary>
        /// Отобразить описание свойства в специальном поле
        /// </summary>
        /// <param name="description">Описание свойства</param>
        public void ShowDescription(string description)
        {
            TbPropertyDescription.Text = description;
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

        // add new style
        private void BtAddNewStyle_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = TvStyles.SelectedItem;
            if (selected == null)
                return;
            if (selected is EntityStyles entityStyles)
            {
                var newStyle = new IntellectualEntityStyle(entityStyles.EntityType, true)
                {
                    Name = ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h13"),
                    StyleType = StyleType.User
                };
                entityStyles.Styles.Add(newStyle);
                StyleManager.AddStyle(newStyle);
            }
            else if (selected is IntellectualEntityStyle style)
            {
                var newStyle = new IntellectualEntityStyle(style.EntityType, true)
                {
                    Name = ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h13"),
                    StyleType = StyleType.User
                };
                _styles.Single(es => es.Styles.Contains(style)).Styles.Add(newStyle);
                StyleManager.AddStyle(newStyle);
            }
        }

        private void SetImage(string entityTypeName)
        {
            if (string.IsNullOrEmpty(entityTypeName)) VbImage.Child = null;
            else
            {
                {
                    if (Resources["Image" + entityTypeName] is Canvas imgResource)
                        VbImage.Child = imgResource;
                }
                {
                    if (Resources["Image" + entityTypeName] is Viewbox imgResource)
                        VbImage.Child = imgResource;
                }
            }
        }

        // delete style
        private void BtRemoveStyle_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = TvStyles.SelectedItem;
            if (selected == null)
                return;
            if (selected is IntellectualEntityStyle style && style.CanEdit)
                if (ModPlusAPI.Windows.MessageBox.ShowYesNo(ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h69"), MessageBoxIcon.Question))
                {
                    foreach (EntityStyles entityStyles in _styles)
                    {
                        if (entityStyles.Styles.Contains(style))
                        {
                            if (style.IsCurrent)
                            {
                                var index = entityStyles.Styles.IndexOf(style);
                                entityStyles.Styles[index - 1].IsCurrent = true;
                            }
                            entityStyles.Styles.Remove(style);
                            break;
                        }
                    }
                    StyleManager.RemoveStyle(style);
                }
        }

        // set current style
        private void BtSetCurrentStyle_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = TvStyles.SelectedItem;
            if (selected == null)
                return;
            if (selected is IntellectualEntityStyle style)
            {
                _styles.Single(es => es.EntityType == style.EntityType).SetCurrent(style);
                BtSetCurrentStyle.IsEnabled = false;
            }
        }

        private void TvStyles_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = TvStyles.SelectedItem;
            if (selected == null)
                return;
            if (selected is IntellectualEntityStyle style)
            {
                _styles.Single(es => es.EntityType == style.EntityType).SetCurrent(style);
                BtSetCurrentStyle.IsEnabled = false;
            }
        }

        private void StyleEditor_OnClosing(object sender, CancelEventArgs e)
        {
            foreach (EntityStyles entityStyles in _styles)
            {
                if (entityStyles.HasStylesWithSameName)
                {
                    ModPlusAPI.Windows.MessageBox.Show(
                        ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h70") + " \"" + entityStyles.DisplayName +
                        "\" " + ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h71") + "\"!" + Environment.NewLine +
                        ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h72"), MessageBoxIcon.Alert);
                    e.Cancel = true;
                    return;
                }
            }
        }

        // При закрытии сохраняю все стили в файлы
        private void StyleEditor_OnClosed(object sender, EventArgs e)
        {
            // reload static settings
            MainStaticSettings.ReloadSettings();

            // save styles
            foreach (EntityStyles entityStyles in _styles)
            {
                StyleManager.SaveStylesToXml(entityStyles.EntityType);
                foreach (IntellectualEntityStyle style in entityStyles.Styles)
                {
                    if (style.IsCurrent)
                    {
                        StyleManager.SaveCurrentStyleToSettings(style);
                    }
                }
            }
        }

        private void BtExpandCollapseImage_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button bt) bt.Opacity = 1.0;
        }

        private void BtExpandCollapseImage_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button bt) bt.Opacity = 0.4;
        }

        private GridLength _topRowHeight;
        private GridLength _rightColumnWidth;

        private void BtExpandImage_OnClick(object sender, RoutedEventArgs e)
        {
            _topRowHeight = TopRow.Height;
            _rightColumnWidth = RightColumn.Width;
            TopRow.MinHeight = 0.0;
            RightColumn.MinWidth = 0.0;
            TopRow.Height = new GridLength(0);
            RightColumn.Width = new GridLength(0);
            BtExpandImage.Visibility = System.Windows.Visibility.Collapsed;
            BtCollapseImage.Visibility = System.Windows.Visibility.Visible;
            VerticalGridSplitter.Visibility = System.Windows.Visibility.Collapsed;
            HorizontalGridSplitter.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void BtCollapseImage_OnClick(object sender, RoutedEventArgs e)
        {
            TopRow.MinHeight = 50.0;
            RightColumn.MinWidth = 200.0;
            TopRow.Height = _topRowHeight;
            RightColumn.Width = _rightColumnWidth;
            BtExpandImage.Visibility = System.Windows.Visibility.Visible;
            BtCollapseImage.Visibility = System.Windows.Visibility.Collapsed;
            VerticalGridSplitter.Visibility = System.Windows.Visibility.Visible;
            HorizontalGridSplitter.Visibility = System.Windows.Visibility.Visible;
        }

        #region Create styles from entities

        private void BtCreateStyleFromEntity_OnClick(object sender, RoutedEventArgs e)
        {
            /* Созданный стиль нужно еще найти в TreeView и выбрать его, раскрыв дерево
             * для этого можно искать по гуиду, а сам поиск взять в плагине mpDwgBase
             */
            try
            {
                Hide();
                PromptEntityOptions promptEntityOptions =
                    new PromptEntityOptions("\n" + ModPlusAPI.Language.GetItem(MainFunction.LangItem, "msg3"));
                promptEntityOptions.SetRejectMessage("\nWrong entity");
                promptEntityOptions.AllowNone = false;
                promptEntityOptions.AddAllowedClass(typeof(BlockReference), true);
                promptEntityOptions.AllowObjectOnLockedLayer = true;
                var selectionResult = AcadHelpers.Document.Editor.GetEntity(promptEntityOptions);
                if (selectionResult.Status == PromptStatus.OK)
                {
                    var newStyleGuid = string.Empty;
                    using (OpenCloseTransaction tr = new OpenCloseTransaction())
                    {
                        var obj = tr.GetObject(selectionResult.ObjectId, OpenMode.ForRead);
                        if (obj is BlockReference blockReference)
                        {
                            var entity = EntityReaderFactory.Instance.GetFromEntity(blockReference);
                            var newStyle = new IntellectualEntityStyle(entity.GetType())
                            {
                                Name = ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h13"),
                                StyleType = StyleType.User,
                                Guid = Guid.NewGuid().ToString()
                            };
                            newStyle.GetPropertiesFromEntity(entity, blockReference);
                            newStyleGuid = newStyle.Guid;
                            foreach (EntityStyles entityStyles in _styles)
                            {
                                if (entityStyles.EntityType == entity.GetType())
                                {
                                    entityStyles.Styles.Add(newStyle);
                                    StyleManager.AddStyle(newStyle);
                                    break;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(newStyleGuid))
                        SearchInTreeViewByGuid(newStyleGuid);
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
            finally
            {
                Show();
            }
        }

        /// <summary>Поиск и выбор в TreeView стиля по Guid</summary>
        private void SearchInTreeViewByGuid(string styleGuid)
        {
            foreach (object item in TvStyles.Items)
            {
                if (item is EntityStyles entityStyles)
                {
                    var collapseIt = true;
                    foreach (IntellectualEntityStyle style in entityStyles.Styles)
                    {
                        if (style.Guid == styleGuid)
                        {
                            if (TvStyles.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem treeViewItem)
                            {
                                treeViewItem.IsExpanded = true;
                                treeViewItem.UpdateLayout();
                                if (treeViewItem.ItemContainerGenerator.ContainerFromIndex(treeViewItem.Items.Count - 1) is TreeViewItem tvi)
                                    tvi.IsSelected = true;
                                collapseIt = false;
                                break;
                            }
                        }
                    }
                    if (collapseIt && TvStyles.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem tvItem)
                        tvItem.IsExpanded = false;
                }
            }
        }

        #endregion
    }
}
