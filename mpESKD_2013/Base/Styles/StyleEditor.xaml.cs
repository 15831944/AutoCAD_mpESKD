namespace mpESKD.Base.Styles
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Helpers;
    using Functions.mpAxis;
    using Functions.mpAxis.Styles;
    using Functions.mpBreakLine;
    using Functions.mpBreakLine.Styles;
    using Functions.mpGroundLine;
    using Functions.mpGroundLine.Styles;
    using ModPlusAPI;
    using ModPlusAPI.Windows;

    public partial class StyleEditor
    {
        public StyleEditor()
        {
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem(MainFunction.LangItem, "tab4");
            Loaded += StyleEditor_OnLoaded;
            ContentRendered += StyleEditor_ContentRendered;
            // check style files
            StyleManager.CheckStylesFile<BreakLineStyle>();
            StyleManager.CheckStylesFile<AxisStyle>();
            StyleManager.CheckStylesFile<GroundLineStyle>();
        }

        private void StyleEditor_OnLoaded(object sender, RoutedEventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
        }

        private ObservableCollection<StyleToBind> _styles;
        private void StyleEditor_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                _styles = new ObservableCollection<StyleToBind>();
                GetSyles();
                TvStyles.ItemsSource = _styles;
                if (_styles.Any())
                    BtCreateStyleFromEntity.IsEnabled = true;
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
        /// <summary>Получение списка стилей. Системных и пользовательских</summary>
        private void GetSyles()
        {
            // т.к. подфункции у меня явные, то для каждой подфункции добавляем стили

            #region mpBreakLine
            var styleToBind = new StyleToBind
            {
                FunctionLocalName = BreakLineInterface.LName,
                FunctionName = BreakLineInterface.Name
            };
            var breakLineStyles = BreakLineStyleForEditor.GetStylesForEditor();
            foreach (BreakLineStyleForEditor style in breakLineStyles)
            {
                style.Parent = styleToBind;
                styleToBind.Styles.Add(style);
            }
            _styles.Add(styleToBind);
            #endregion

            #region Axis

            styleToBind = new StyleToBind
            {
                FunctionLocalName = AxisInterface.LName,
                FunctionName = AxisInterface.Name
            };
            var axisStyles = AxisStyleForEditor.GetStylesForEditor();
            foreach (AxisStyleForEditor style in axisStyles)
            {
                style.Parent = styleToBind;
                styleToBind.Styles.Add(style);
            }
            _styles.Add(styleToBind);

            #endregion

            #region Ground line

            styleToBind = new StyleToBind()
            {
                FunctionLocalName = GroundLineInterface.LName,
                FunctionName = GroundLineInterface.Name
            };
            var groundLineStyles = GroundLineStyleForEditor.GetStylesForEditor();
            foreach (GroundLineStyleForEditor style in groundLineStyles)
            {
                style.Parent = styleToBind;
                styleToBind.Styles.Add(style);
            }
            _styles.Add(styleToBind);

            #endregion
        }

        private void TvStyles_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (BorderProperties.Child != null) BorderProperties.Child = null;

            var item = e.NewValue;
            if (item != null)
                BtAddNewStyle.IsEnabled = true;
            if (item is StyleToBind styleToBind)
            {
                BtRemoveStyle.IsEnabled = false;
                // image
                SetImage(styleToBind.FunctionName);
            }
            else if (item is MPCOStyleForEditor styleForEditor)
            {
                BtRemoveStyle.IsEnabled = styleForEditor.CanEdit;
                BtSetCurrentStyle.IsEnabled = !styleForEditor.IsCurrent;
                // break line
                if (styleForEditor is BreakLineStyleForEditor breakLineStyle)
                {
                    BorderProperties.Child = new BreakLineStyleProperties(breakLineStyle.LayerName) { DataContext = item };
                    SetImage(BreakLineInterface.Name);
                }
                // axis
                if (styleForEditor is AxisStyleForEditor axisStyle)
                {
                    BorderProperties.Child = new AxisStyleProperties(axisStyle.LayerName) { DataContext = item };
                    SetImage(AxisInterface.Name);
                }
                // ground line
                if (styleForEditor is GroundLineStyleForEditor groundLineStyle)
                {
                    BorderProperties.Child = new GroundLineStyleProperties(groundLineStyle.LayerName) { DataContext = item };
                    SetImage(GroundLineInterface.Name);
                }
            }
            else SetImage(string.Empty);
        }
        // add new style
        private void BtAddNewStyle_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = TvStyles.SelectedItem;
            if (selected == null) return;
            if (selected is StyleToBind styleToBind)
            {
                if (styleToBind.FunctionName == BreakLineInterface.Name)
                    styleToBind.Styles.Add(new BreakLineStyleForEditor(styleToBind));
                if (styleToBind.FunctionName == AxisInterface.Name)
                    styleToBind.Styles.Add(new AxisStyleForEditor(styleToBind));
                if (styleToBind.FunctionName == GroundLineInterface.Name)
                    styleToBind.Styles.Add(new GroundLineStyleForEditor(styleToBind));
            }
            // break line
            if (selected is BreakLineStyleForEditor breakLineStyleForEditor)
            {
                breakLineStyleForEditor.Parent.Styles.Add(new BreakLineStyleForEditor(breakLineStyleForEditor.Parent));
            }
            // axis
            if (selected is AxisStyleForEditor axisStyleForEditor)
            {
                axisStyleForEditor.Parent.Styles.Add(new AxisStyleForEditor(axisStyleForEditor.Parent));
            }
            // ground line
            if (selected is GroundLineStyleForEditor groundLineStyleForEditor)
            {
                groundLineStyleForEditor.Parent.Styles.Add(new GroundLineStyleForEditor(groundLineStyleForEditor.Parent));
            }
        }

        private void SetImage(string imageName)
        {
            if (string.IsNullOrEmpty(imageName)) VbImage.Child = null;
            else
            {
                {
                    if (Resources["Image" + imageName] is Canvas imgResource)
                        VbImage.Child = imgResource;
                }
                {
                    if (Resources["Image" + imageName] is Viewbox imgResource)
                        VbImage.Child = imgResource;
                }
            }
        }
        // delete style
        private void BtRemoveStyle_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = TvStyles.SelectedItem;
            if (selected == null) return;
            if (selected is MPCOStyleForEditor style && style.CanEdit)
                if (ModPlusAPI.Windows.MessageBox.ShowYesNo(ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h69"), MessageBoxIcon.Question))
                {
                    if (style.IsCurrent)
                    {
                        var selectedIndex = style.Parent.Styles.IndexOf(style);
                        // set current to previous
                        SetCurrentStyle(style.Parent.Styles[selectedIndex - 1]);
                    }

                    // remove from style manager
                    StyleManager.RemoveStyle(style.Guid);

                    // remove from collection
                    style.Parent.Styles.Remove(style);
                }
        }
        // set current style
        private void BtSetCurrentStyle_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = TvStyles.SelectedItem;
            if (selected == null) return;
            if (selected is MPCOStyleForEditor style)
            {
                SetCurrentStyle(style);
                BtSetCurrentStyle.IsEnabled = false;
            }
        }

        private void TvStyles_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = TvStyles.SelectedItem;
            if (selected == null) return;
            if (selected is MPCOStyleForEditor style)
            {
                SetCurrentStyle(style);
                BtSetCurrentStyle.IsEnabled = false;
            }
        }
        private void SetCurrentStyle(MPCOStyleForEditor style)
        {
            foreach (MPCOStyleForEditor styleForEditor in style.Parent.Styles)
            {
                styleForEditor.IsCurrent = false;
            }
            style.IsCurrent = true;
        }
        private void StyleEditor_OnClosing(object sender, CancelEventArgs e)
        {
            foreach (StyleToBind styleToBind in _styles)
            {
                var styleNames = new List<string>();
                foreach (var style in styleToBind.Styles)
                {
                    if (!styleNames.Contains(style.Name))
                        styleNames.Add(style.Name);
                    else
                    {
                        ModPlusAPI.Windows.MessageBox.Show(
                            ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h70") + " \"" + styleToBind.FunctionLocalName +
                            "\" " + ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h71") + " \"" + style.Name +
                            "\"!" + Environment.NewLine +
                            ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h72"), MessageBoxIcon.Alert);
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }
        // При закрытии сохраняю все стили в файлы
        private void StyleEditor_OnClosed(object sender, EventArgs e)
        {
            // reload static settings
            MainStaticSettings.ReloadSettings();
            // save styles IsCurrent
            foreach (StyleToBind styleToBind in _styles)
            {
                if (styleToBind.FunctionName == BreakLineInterface.Name)
                {
                    var currentStyle = styleToBind.Styles.FirstOrDefault(s => s.IsCurrent);
                    if (currentStyle != null)
                        UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpBreakLine", "CurrentStyleGuid", currentStyle.Guid, true);
                }
                if (styleToBind.FunctionName == AxisInterface.Name)
                {
                    var currentStyle = styleToBind.Styles.FirstOrDefault(s => s.IsCurrent);
                    if (currentStyle != null)
                        UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpAxis", "CurrentStyleGuid", currentStyle.Guid, true);
                }

                if (styleToBind.FunctionName == GroundLineInterface.Name)
                {
                    var currentStyle = styleToBind.Styles.FirstOrDefault(s => s.IsCurrent);
                    if (currentStyle != null)
                        UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpGroundLine", "CurrentStyleGuid", currentStyle.Guid, true);
                }
            }
            // save styles
            // break line style
            StyleManager.SaveStylesToXml<BreakLineStyle, BreakLineStyleForEditor>(
                _styles.Single(s => s.FunctionName == BreakLineInterface.Name)
                    .Styles.Where(s => s.CanEdit).Cast<BreakLineStyleForEditor>().ToList(),
                BreakLineStyleForEditor.ConvertStyleForEditorToXElement);
            StyleManager.ReloadStyles(
                BreakLineStyle.Instance.CreateSystemStyles<BreakLineStyle>(),
                BreakLineStyle.Instance.ParseStyleFromXElement<BreakLineStyle>);

            // axis styles
            StyleManager.SaveStylesToXml<AxisStyle, AxisStyleForEditor>(
                _styles.Single(s => s.FunctionName == AxisInterface.Name)
                    .Styles.Where(s => s.CanEdit).Cast<AxisStyleForEditor>().ToList(),
                AxisStyleForEditor.ConvertStyleForEditorToXElement);
            StyleManager.ReloadStyles(
                AxisStyle.Instance.CreateSystemStyles<AxisStyle>(),
                AxisStyle.Instance.ParseStyleFromXElement<AxisStyle>);
            
            // ground line styles
            StyleManager.SaveStylesToXml<GroundLineStyle, GroundLineStyleForEditor>(
                _styles.Single(s => s.FunctionName == GroundLineInterface.Name)
                    .Styles.Where(s => s.CanEdit).Cast<GroundLineStyleForEditor>().ToList(),
                GroundLineStyleForEditor.ConvertStyleForEditorToXElement);
            StyleManager.ReloadStyles(
                GroundLineStyle.Instance.CreateSystemStyles<GroundLineStyle>(), 
                GroundLineStyle.Instance.ParseStyleFromXElement<GroundLineStyle>);
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
                            // mpBreakLine
                            if (ExtendedDataHelpers.IsApplicable(obj, BreakLineInterface.Name))
                                newStyleGuid = AddStyleFromBreakLine(blockReference);
                            // mpAxis
                            if (ExtendedDataHelpers.IsApplicable(obj, AxisInterface.Name))
                                newStyleGuid = AddStyleFromAxis(blockReference);
                            // mpGroundLine
                            if (ExtendedDataHelpers.IsApplicable(obj, GroundLineInterface.Name))
                                newStyleGuid = AddStyleFromGroundLine(blockReference);
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
            finally { Show(); }
        }

        /// <summary>Создание нового стиля из BreakLine</summary>
        /// <param name="blkReference">Блок, представляющий BreakLine</param>
        /// <returns>Guid нового стиля</returns>
        private string AddStyleFromBreakLine(BlockReference blkReference)
        {
            var styleGuid = string.Empty;
            var breakLine = EntityReaderFactory.Instance.GetFromEntity<BreakLine>(blkReference);
            if (breakLine != null)
            {
                var styleToBind = _styles.FirstOrDefault(s => s.FunctionName == BreakLineInterface.Name);
                if (styleToBind != null)
                {
                    var styleForEditor = new BreakLineStyleForEditor(styleToBind)
                    {
                        // general
                        LayerName = blkReference.Layer,
                        Overhang = breakLine.Overhang,
                        Scale = breakLine.Scale,
                        //
                        BreakHeight = breakLine.BreakHeight,
                        BreakWidth = breakLine.BreakWidth,
                        LineTypeScale = breakLine.LineTypeScale
                    };
                    styleGuid = styleForEditor.Guid;
                    styleToBind.Styles.Add(styleForEditor);
                }
            }
            return styleGuid;
        }

        /// <summary>Создание нового стиля из Axis</summary>
        /// <param name="blkReference">Блок, представляющий BreakLine</param>
        /// <returns>Guid нового стиля</returns>
        private string AddStyleFromAxis(BlockReference blkReference)
        {
            var styleGuid = string.Empty;
            var axis = EntityReaderFactory.Instance.GetFromEntity<Axis>(blkReference);
            if (axis != null)
            {
                var styleToBind = _styles.FirstOrDefault(s => s.FunctionName == AxisInterface.Name);
                if (styleToBind != null)
                {
                    var styleForEditor = new AxisStyleForEditor(styleToBind)
                    {
                        // general
                        LayerName = blkReference.Layer,
                        LineTypeScale = axis.LineTypeScale,
                        Scale = axis.Scale,
                        //
                        LineType = blkReference.Linetype,
                        //
                        //MarkersPosition = axis.MarkersPosition,
                        MarkersDiameter = axis.MarkersDiameter,
                        MarkersCount = axis.MarkersCount,
                        ////FirstMarkerType = axis.FirstMarkerType,
                        ////SecondMarkerType = axis.SecondMarkerType,
                        ////ThirdMarkerType = axis.ThirdMarkerType,
                        ////OrientMarkerType = axis.OrientMarkerType,
                        //Fracture = axis.Fracture,
                        //BottomFractureOffset = axis.BottomFractureOffset,
                        //TopFractureOffset = axis.TopFractureOffset,
                        //ArrowsSize = axis.ArrowsSize,
                        ////TextStyle = axis.TextStyle,
                        ////TextHeight = axis.TextHeight
                    };
                    styleGuid = styleForEditor.Guid;
                    styleToBind.Styles.Add(styleForEditor);
                }
            }
            return styleGuid;
        }

        /// <summary>
        /// Создание нового стиля из GroundLine
        /// </summary>
        /// <param name="blkReference">Блок, представляющий GroundLine</param>
        /// <returns>Guid нового стиля</returns>
        private string AddStyleFromGroundLine(BlockReference blkReference)
        {
            var styleGuid = string.Empty;
            //todo old
            //var groundLine = GroundLine.GetGroundLineFromEntity(blkReference);
            var groundLine = EntityReaderFactory.Instance.GetFromEntity<GroundLine>(blkReference);
            if (groundLine != null)
            {
                var styleToBind = _styles.FirstOrDefault(s => s.FunctionName == GroundLineInterface.Name);
                if (styleToBind != null)
                {
                    var styleForEditor = new GroundLineStyleForEditor(styleToBind)
                    {
                        // general
                        LayerName = blkReference.Layer,
                        LineTypeScale = groundLine.LineTypeScale,
                        Scale = groundLine.Scale,
                        //
                        LineType = blkReference.Linetype,
                        //
                        FirstStrokeOffset = groundLine.FirstStrokeOffset,
                        StrokeLength = groundLine.StrokeLength,
                        StrokeOffset = groundLine.StrokeOffset,
                        StrokeAngle = groundLine.StrokeAngle,
                        Space = groundLine.Space
                    };
                    styleGuid = styleForEditor.Guid;
                    styleToBind.Styles.Add(styleForEditor);
                }
            }

            return styleGuid;
        }

        /// <summary>Поиск и выбор в TreeView стиля по Guid</summary>
        private void SearchInTreeViewByGuid(string styleGuid)
        {
            foreach (object item in TvStyles.Items)
            {
                if (item is StyleToBind styleToBind)
                {
                    var collapseIt = true;
                    foreach (var style in styleToBind.Styles)
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

    /// <summary>
    /// Класс, описывающий Стиль (группу стилей для каждого примитива) для использования в биндинге
    /// </summary>
    public class StyleToBind
    {
        public StyleToBind()
        {
            Styles = new ObservableCollection<MPCOStyleForEditor>();
        }

        public string FunctionLocalName { get; set; }

        public string FunctionName { get; set; }

        public ObservableCollection<MPCOStyleForEditor> Styles { get; set; }
    }
}
