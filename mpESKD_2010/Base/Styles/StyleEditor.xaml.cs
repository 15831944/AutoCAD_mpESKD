using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using mpESKD.Functions.mpBreakLine;
using mpESKD.Functions.mpBreakLine.Styles;
using ModPlusAPI.Windows;
using ModPlusAPI.Windows.Helpers;

namespace mpESKD.Base.Styles
{
    public partial class StyleEditor
    {
        public StyleEditor()
        {
            InitializeComponent();
            this.OnWindowStartUp();
            Loaded += StyleEditor_OnLoaded;
            MouseLeftButtonDown += StyleEditor_OnMouseLeftButtonDown;
            PreviewKeyDown += StyleEditor_OnPreviewKeyDown;
            ContentRendered += StyleEditor_ContentRendered;
        }

        #region Window work
        private void StyleEditor_OnLoaded(object sender, RoutedEventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
        }

        private void StyleEditor_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void StyleEditor_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }
        #endregion

        private ObservableCollection<StyleToBind> _styles;
        private void StyleEditor_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                _styles = new ObservableCollection<StyleToBind>();
                GetSyles();
                TvStyles.ItemsSource = _styles;
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
                FunctionLocalName = BreakLineFunction.MPCOEntDisplayName,
                FunctionName = BreakLineFunction.MPCOEntName
            };
            var breakLineStyles = BreakLineStylesManager.GetStylesForEditor();
            foreach (BreakLineStyleForEditor style in breakLineStyles)
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

                if (styleForEditor is BreakLineStyleForEditor breakLineStyle)
                {
                    BorderProperties.Child = new BreakLineStyleProperties(breakLineStyle.LayerName) { DataContext = item };
                    SetImage(BreakLineFunction.MPCOEntName);
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
                if (styleToBind.FunctionLocalName == BreakLineFunction.MPCOEntDisplayName)
                    styleToBind.Styles.Add(new BreakLineStyleForEditor(styleToBind));
            }
            if (selected is BreakLineStyleForEditor breakLineStyleForEditor)
            {
                breakLineStyleForEditor.Parent.Styles.Add(new BreakLineStyleForEditor(breakLineStyleForEditor.Parent));
            }
        }

        private void SetImage(string imageName)
        {
            if (string.IsNullOrEmpty(imageName)) VbImage.Child = null;
            else
            {
                if (Resources["Image" + imageName] is Canvas imgResorce)
                    VbImage.Child = imgResorce;
            }
        }
        // delete style
        private void BtRemoveStyle_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = TvStyles.SelectedItem;
            if (selected == null) return;
            if (selected is MPCOStyleForEditor style && style.CanEdit)
                if (ModPlusAPI.Windows.MessageBox.ShowYesNo("Выбранный стиль будет удален безвозратно! Продолжить?", MessageBoxIcon.Question))
                {
                    if (style.IsCurrent)
                    {
                        var selectedIndex = style.Parent.Styles.IndexOf(style);
                        // set current to previus
                        SetCurrentStyle(style.Parent.Styles[selectedIndex - 1]);
                    }
                    // remove from collection
                    style.Parent.Styles.Remove(style);
                    // remove from file
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
                    if(!styleNames.Contains(style.Name))
                        styleNames.Add(style.Name);
                    else
                    {
                        ModPlusAPI.Windows.MessageBox.Show("Группа стилей \"" + styleToBind.FunctionLocalName +
                                                           "\" содержит стили с одинаковым именем \"" + style.Name +
                                                           "\"!" + Environment.NewLine +
                                                           "Переименуйте стили так, чтобы не было одинаковых названий", MessageBoxIcon.Alert);
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
            // save styles
            // break line style
            BreakLineStylesManager.SaveStylesToXml(
                _styles.Single(s => s.FunctionLocalName == BreakLineFunction.MPCOEntDisplayName)
                .Styles.Where(s => s.CanEdit).Cast<BreakLineStyleForEditor>().ToList());
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
            BtExpandImage.Visibility = Visibility.Collapsed;
            BtCollapseImage.Visibility = Visibility.Visible;
            VerticalGridSplitter.Visibility = Visibility.Collapsed;
            HorizontalGridSplitter.Visibility = Visibility.Collapsed;
        }

        private void BtCollapseImage_OnClick(object sender, RoutedEventArgs e)
        {
            TopRow.MinHeight = 50.0;
            RightColumn.MinWidth = 200.0;
            TopRow.Height = _topRowHeight;
            RightColumn.Width = _rightColumnWidth;
            BtExpandImage.Visibility = Visibility.Visible;
            BtCollapseImage.Visibility = Visibility.Collapsed;
            VerticalGridSplitter.Visibility = Visibility.Visible;
            HorizontalGridSplitter.Visibility = Visibility.Visible;
        }

        
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
