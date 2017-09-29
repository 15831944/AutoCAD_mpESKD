using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using Autodesk.AutoCAD.DatabaseServices;
using mpESKD.Base.Properties;

// ReSharper disable InconsistentNaming

namespace mpESKD.Functions.mpBreakLine.Properties
{
    // ReSharper disable once InconsistentNaming
    public static class mpBreakLineProperties
    {
        /// <summary>Поле, описывающее свойство "Тип линии обрыва"</summary>
        public static MPCOTypeProperty<BreakLineType> BreakLineTypePropertyDescriptive = new MPCOTypeProperty<BreakLineType>
        {
            PropertyType = MPCOPropertyType.Type,
            Name = "BreakLineType",
            DisplayName = "Тип линии:",
            DefaultValue = BreakLineType.Linear,
            Description = "Тип линии: линейный, криволинейный или цилиндрический"
        };
        /// <summary>Поле, описывающее свойство "Выступы за объект"</summary>
        public static MPCOIntProperty OverhangPropertyDescriptive = new MPCOIntProperty
        {
            PropertyType = MPCOPropertyType.Int,
            Name = "Overhang",
            DisplayName = "Выступы за объект:",
            DefaultValue = 2,
            Minimum = 0,
            Maximum = 10,
            Description = "Для линейного обрыва значение выступа задается в мм. Для криволинейного обрыва значение выступа задается в % от длины между основными точками. Для цилиндрического обрыва значение выступа не используется"
        };
        /// <summary>Поле, описывающее свойство "Высота обрыва"</summary>
        public static MPCOIntProperty BreakHeightPropertyDescriptive = new MPCOIntProperty
        {
            PropertyType = MPCOPropertyType.Int,
            Name = "BreakHeight",
            DisplayName = "Ширина разрыва:",
            DefaultValue = 10,
            Minimum = 1,
            Maximum = 13,
            Description = "Высота разрыва в мм. Только для линии обрыва линейного типа"
        };
        /// <summary>Поле, описывающее свойство "Ширина обрыва"</summary>
        public static MPCOIntProperty BreakWidthPropertyDescriptive = new MPCOIntProperty
        {
            PropertyType = MPCOPropertyType.Int,
            Name = "BreakWidth",
            DisplayName = "Высота разрыва:",
            DefaultValue = 5,
            Minimum = 1,
            Maximum = 20,
            Description = "Ширина разрыва в мм. Только для линии обрыва линейного типа"
        };
        /// <summary>Поле, описывающее свойство "Масштаб"</summary>
        public static MPCOTypeProperty<AnnotationScale> ScalePropertyDescriptive = new MPCOTypeProperty<AnnotationScale>
        {
            PropertyType = MPCOPropertyType.Type,
            Name = "Scale",
            DisplayName = "Масштаб:",
            DefaultValue = new AnnotationScale { Name = "1:1", DrawingUnits = 1.0, PaperUnits = 1.0 },
            Description = "Масштаб линии обрыва"
        };
        /// <summary>Поле, описывающее свойство "Масштаб типа линии"</summary>
        public static MPCODoubleProperty LineTypeScalePropertyDescriptive = new MPCODoubleProperty
        {
            PropertyType = MPCOPropertyType.Double,
            Name = "LineTypeScale",
            DisplayName = "Масштаб типа линий:",
            DefaultValue = 1.0, Minimum = 0, Maximum = double.MaxValue,
            Description = "Масштаб типа линии для заданного в свойствах блока типа линии"
        };
        public static MPCOStringProperty LayerName = new MPCOStringProperty
        {
            PropertyType = MPCOPropertyType.String,
            Name = "LayerName",
            DisplayName = "Слой",
            DefaultValue = "По умолчанию",
            Description = "Слой примитива"
        };
    }
    public static class mpBreakLinePropertiesHelpers
    {
        public static List<string> BreakLineTypeLocalNames = new List<string> { "Линейный", "Криволинейный", "Цилиндрический" };
        #region Methods

        public static BreakLineType GetBreakLineTypeByLocalName(string local)
        {
            if (local == "Линейный") return BreakLineType.Linear;
            if (local == "Криволинейный") return BreakLineType.Curvilinear;
            if (local == "Цилиндрический") return BreakLineType.Cylindrical;
            return BreakLineType.Linear;
        }

        public static string GetLocalBreakLineTypeName(BreakLineType breakLineType)
        {
            if (breakLineType == BreakLineType.Linear) return "Линейный";
            if (breakLineType == BreakLineType.Curvilinear) return "Криволинейный";
            if (breakLineType == BreakLineType.Cylindrical) return "Цилиндрический";
            return "Линейный";
        }

        public static BreakLineType GetBreakLineTypeFromString(string str)
        {
            if (str == "Linear") return BreakLineType.Linear;
            if (str == "Curvilinear") return BreakLineType.Curvilinear;
            if (str == "Cylindrical") return BreakLineType.Cylindrical;
            return BreakLineType.Linear;
        }
        #endregion
    }

    public class BreakLineTypeValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BreakLineType breakLine)
                return mpBreakLinePropertiesHelpers.GetLocalBreakLineTypeName(breakLine);
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return mpBreakLinePropertiesHelpers.GetBreakLineTypeByLocalName(value?.ToString());
        }
    }
    /// <summary>
    /// Тип линии: линейный, криволинейный, цилиндрический
    /// </summary>
    public enum BreakLineType
    {
        Linear = 1,
        Curvilinear = 2,
        Cylindrical = 3
    }
}
