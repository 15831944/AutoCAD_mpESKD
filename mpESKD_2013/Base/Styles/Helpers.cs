namespace mpESKD.Base.Styles
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Runtime;
    using Properties;
    using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

    public static class StyleEditorWork
    {
        private static StyleEditor _styleEditor;

        [CommandMethod("ModPlus", "mpStyleEditor", CommandFlags.Modal)]
        public static void OpenStyleEditor()
        {
            if (_styleEditor == null)
            {
                _styleEditor = new StyleEditor();
                _styleEditor.Closed += (sender, args) => _styleEditor = null;
            }

            if (_styleEditor.IsLoaded)
            {
                _styleEditor.Activate();
            }
            else
            {
                AcApp.ShowModalWindow(AcApp.MainWindow.Handle, _styleEditor, false);
            }
        }

        public static void ShowDescription(string description)
        {
            if (_styleEditor != null)
            {
                _styleEditor.TbPropertyDescription.Text = description;
            }
        }
    }

    public class AnnotationScaleValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AnnotationScale annotationScale)
            {
                return annotationScale.Name;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Parsers.AnnotationScaleFromString(value?.ToString());
        }
    }
}
