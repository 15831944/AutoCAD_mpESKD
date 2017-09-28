#if ac2010
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
#elif ac2013
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#endif
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using mpESKD.Base.Properties;

namespace mpESKD.Base.Styles
{
    public static class StyleHelpers
    {
        public static object GetPropertyValue(MPCOBaseProperty property)
        {
            if (property is MPCOIntProperty intProperty)
                return intProperty.Value;
            if (property is MPCODoubleProperty doubleProperty)
                return doubleProperty.Value;
            if (property is MPCOStringProperty stringProperty)
                return stringProperty.Value;
            if (property is MPCOScaleProperty scaleProperty)
                return scaleProperty.ScaleName;
            if (property is MPCOTypeProperty<object> typeProperty)
                return typeProperty.Value;
            return null;
        }
        public static int GetPropertyValue(IMPCOStyle style, string propName, int defaultValue)
        {
            if (style.Properties != null && style.Properties.Any())
                foreach (var property in style.Properties)
                {
                    if (property.Name == propName)
                        return ((MPCOIntProperty) property).Value;
                }
            return defaultValue;
        }
        public static double GetPropertyValue(IMPCOStyle style, string propName, double defaultValue)
        {
            if (style.Properties != null && style.Properties.Any())
                foreach (var property in style.Properties)
                {
                    if (property.Name == propName)
                        return ((MPCODoubleProperty)property).Value;
                }
            return defaultValue;
        }
        public static string GetPropertyValue(IMPCOStyle style, string propName, string defaultValue)
        {
            if (style.Properties != null && style.Properties.Any())
                foreach (var property in style.Properties)
                {
                    if (property.Name == propName)
                        return ((MPCOStringProperty)property).Value;
                }
            return defaultValue;
        }
        public static T GetPropertyValue<T>(IMPCOStyle style, string propName, T defaultValue)
        {
            if (style.Properties != null && style.Properties.Any())
                foreach (var property in style.Properties)
                {
                    if (property.Name == propName)
                        return ((MPCOTypeProperty<T>)property).Value;
                }
            return defaultValue;
        }

        public static MPCOIntProperty CreateProperty(int value, MPCOIntProperty descriptiveProperty)
        {
            return new MPCOIntProperty
            {
                PropertyType = descriptiveProperty.PropertyType,
                Name = descriptiveProperty.Name,
                DisplayName = descriptiveProperty.DisplayName,
                DefaultValue = descriptiveProperty.DefaultValue,
                Maximum = descriptiveProperty.Maximum,
                Minimum = descriptiveProperty.Minimum,
                Description = descriptiveProperty.Description,
                Value = value
            };
        }
        public static MPCODoubleProperty CreateProperty(double value, MPCODoubleProperty descriptiveProperty)
        {
            return new MPCODoubleProperty
            {
                PropertyType = descriptiveProperty.PropertyType,
                Name = descriptiveProperty.Name,
                DisplayName = descriptiveProperty.DisplayName,
                DefaultValue = descriptiveProperty.DefaultValue,
                Maximum = descriptiveProperty.Maximum,
                Minimum = descriptiveProperty.Minimum,
                Description = descriptiveProperty.Description,
                Value = value
            };
        }
        public static MPCOStringProperty CreateProperty(string value, MPCOStringProperty descriptiveProperty)
        {
            return new MPCOStringProperty
            {
                PropertyType = descriptiveProperty.PropertyType,
                Name = descriptiveProperty.Name,
                DisplayName = descriptiveProperty.DisplayName,
                DefaultValue = descriptiveProperty.DefaultValue,
                Description = descriptiveProperty.Description,
                Value = value
            };
        }
        public static MPCOTypeProperty<T> CreateProperty<T>(T value, MPCOTypeProperty<T> descriptiveProperty)
        {
            return new MPCOTypeProperty<T>
            {
                PropertyType = descriptiveProperty.PropertyType,
                Name = descriptiveProperty.Name,
                DisplayName = descriptiveProperty.DisplayName,
                DefaultValue = descriptiveProperty.DefaultValue,
                Description = descriptiveProperty.Description,
                Value = value
            };
        }

        private static MPCOStyleType GetStyleTypeByString(string type)
        {
            if (type == "System")
                return MPCOStyleType.System;
            if (type == "User")
                return MPCOStyleType.User;
            return MPCOStyleType.System;
        }
        
    }

    //public class MPCOstyle : IMPCOStyle
    //{
    //    public MPCOstyle()
    //    {
    //        Properties = new List<MPCOBaseProperty>();
    //    }
    //    public string Name { get; set; }
    //    public string FunctionName { get; set; }
    //    public string Description { get; set; }
    //    public string Guid { get; set; }
    //    public MPCOStyleType StyleType { get; set; }
    //    public List<MPCOBaseProperty> Properties { get; set; }
    //}

    public static class StyleEditorWork
    {
        private static StyleEditor _styleEditor;
        [CommandMethod("ModPlus", "mpStyleEditor", CommandFlags.Modal)]
        public static void OpenStyleEditor()
        {
            if (_styleEditor == null)
            {
                _styleEditor = new StyleEditor();
                _styleEditor.Closed += styleEditor_Closed;
            }
            if (_styleEditor.IsLoaded) _styleEditor.Activate();
            else AcApp.ShowModalWindow(AcApp.MainWindow.Handle, _styleEditor, false);
        }

        static void styleEditor_Closed(object sender, EventArgs e)
        {
            _styleEditor = null;
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
                return annotationScale.Name;
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Parsers.AnnotationScaleFromString(value?.ToString());
        }
    }
}
