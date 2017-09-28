using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
        /// <summary>
        /// Получение списка стилей. Системных и пользовательских
        /// </summary>
        private void GetSyles()
        {
            // т.к. подфункции у меня явные, то для каждой подфункции добавляем стили

            #region mpBreakLine
            var styleToBind = new StyleToBind { MainFunctionName = BreakLineFunction.MPCOEntDisplayName };
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
            if (item is StyleToBind)
            {
                BtRemoveStyle.IsEnabled = false;
            }
            if (item is MPCOStyleForEditor styleForEditor)
            {
                BtRemoveStyle.IsEnabled = styleForEditor.CanEdit;
                BtSetCurrentStyle.IsEnabled = !styleForEditor.IsCurrent;

                if (styleForEditor is BreakLineStyleForEditor)
                    BorderProperties.Child = new BreakLineStyleProperties { DataContext = item };
            }
        }
        // add new style
        private void BtAddNewStyle_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = TvStyles.SelectedItem;
            if (selected == null) return;
            if (selected is StyleToBind styleToBind)
            {
                if (styleToBind.MainFunctionName == BreakLineFunction.MPCOEntDisplayName)
                    styleToBind.Styles.Add(new BreakLineStyleForEditor(styleToBind));
            }
            if (selected is BreakLineStyleForEditor breakLineStyleForEditor)
            {
                breakLineStyleForEditor.Parent.Styles.Add(new BreakLineStyleForEditor(breakLineStyleForEditor.Parent));
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
        // При закрытии сохраняю все стили в файлы
        private void StyleEditor_OnClosed(object sender, EventArgs e)
        {
            // save styles
            // break line style
            BreakLineStylesManager.SaveStylesToXml(
                _styles.Single(s => s.MainFunctionName == BreakLineFunction.MPCOEntDisplayName)
                .Styles.Where(s => s.CanEdit).Cast<BreakLineStyleForEditor>().ToList());
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
        public string MainFunctionName { get; set; }
        public ObservableCollection<MPCOStyleForEditor> Styles { get; set; }
    }
}
