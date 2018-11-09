// ReSharper disable InconsistentNaming

namespace mpESKD.Functions.mpBreakLine.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;
    using Autodesk.AutoCAD.DatabaseServices;
    using mpESKD.Base.Properties;
    using ModPlusAPI;
    using Base.Enums;

    // ReSharper disable once InconsistentNaming
    public static class BreakLineProperties
    {
        /// <summary>Поле, описывающее свойство "Тип линии обрыва"</summary>
        public static MPCOTypeProperty<BreakLineType> BreakLineType = new MPCOTypeProperty<BreakLineType>
        {
            Name = "BreakLineType",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p1"), // Тип линии
            DefaultValue = Base.Enums.BreakLineType.Linear,
            Description = Language.GetItem(MainFunction.LangItem, "d1")
        };
        /// <summary>Поле, описывающее свойство "Выступы за объект"</summary>
        public static MPCOIntProperty Overhang = new MPCOIntProperty
        {
            Name = "Overhang",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p2"), // Выступы за объект
            DefaultValue = 2,
            Minimum = 0,
            Maximum = 10,
            Description = Language.GetItem(MainFunction.LangItem, "d2")
        };
        /// <summary>Поле, описывающее свойство "Высота обрыва"</summary>
        public static MPCOIntProperty BreakHeight = new MPCOIntProperty
        {
            Name = "BreakHeight",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p4"), // Высота разрыва
            DefaultValue = 10,
            Minimum = 1,
            Maximum = 13,
            Description = Language.GetItem(MainFunction.LangItem, "d4")
        };
        /// <summary>Поле, описывающее свойство "Ширина обрыва"</summary>
        public static MPCOIntProperty BreakWidth = new MPCOIntProperty
        {
            Name = "BreakWidth",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p3"), // Ширина разрыва
            DefaultValue = 5,
            Minimum = 1,
            Maximum = 20,
            Description = Language.GetItem(MainFunction.LangItem, "d3")
        };
        /// <summary>Поле, описывающее свойство "Масштаб"</summary>
        public static MPCOTypeProperty<AnnotationScale> Scale = new MPCOTypeProperty<AnnotationScale>
        {
            Name = "Scale",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p5"), // Масштаб:
            DefaultValue = new AnnotationScale { Name = "1:1", DrawingUnits = 1.0, PaperUnits = 1.0 },
            Description = Language.GetItem(MainFunction.LangItem, "d5")
        };
        /// <summary>Поле, описывающее свойство "Масштаб типа линии"</summary>
        public static MPCODoubleProperty LineTypeScale = new MPCODoubleProperty
        {
            Name = "LineTypeScale",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p6"), // Масштаб типа линий
            DefaultValue = 1.0, Minimum = 0, Maximum = double.MaxValue,
            Description = Language.GetItem(MainFunction.LangItem, "d6")
        };
        public static MPCOStringProperty LayerName = new MPCOStringProperty
        {
            Name = "LayerName",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p7"), // Слой
            DefaultValue = Language.GetItem(MainFunction.LangItem, "defl"), // По умолчанию
            Description = Language.GetItem(MainFunction.LangItem, "d7") // Слой примитива
        };
    }
    public static class BreakLinePropertiesHelpers
    {
        public static List<string> BreakLineTypeLocalNames = new List<string>
        {
            Language.GetItem(MainFunction.LangItem, "blt1"), // "Линейный",
            Language.GetItem(MainFunction.LangItem, "blt2"), //"Криволинейный",
            Language.GetItem(MainFunction.LangItem, "blt3") //"Цилиндрический"
        };

        #region Methods

        public static BreakLineType GetBreakLineTypeByLocalName(string local)
        {
            if (local == Language.GetItem(MainFunction.LangItem, "blt1")) return BreakLineType.Linear;
            if (local == Language.GetItem(MainFunction.LangItem, "blt2")) return BreakLineType.Curvilinear;
            if (local == Language.GetItem(MainFunction.LangItem, "blt3")) return BreakLineType.Cylindrical;
            return BreakLineType.Linear;
        }

        public static string GetLocalBreakLineTypeName(BreakLineType breakLineType)
        {
            if (breakLineType == BreakLineType.Linear) return Language.GetItem(MainFunction.LangItem, "blt1");
            if (breakLineType == BreakLineType.Curvilinear) return Language.GetItem(MainFunction.LangItem, "blt2");
            if (breakLineType == BreakLineType.Cylindrical) return Language.GetItem(MainFunction.LangItem, "blt3");
            return Language.GetItem(MainFunction.LangItem, "blt1");
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
                return BreakLinePropertiesHelpers.GetLocalBreakLineTypeName(breakLine);
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BreakLinePropertiesHelpers.GetBreakLineTypeByLocalName(value?.ToString());
        }
    }
}
