namespace mpESKD.Base.Enums
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;
    using ModPlusAPI;

    /// <summary>
    /// Тип линии: линейный, криволинейный, цилиндрический
    /// </summary>
    public enum BreakLineType
    {
        /// <summary>
        /// Линейный
        /// </summary>
        Linear = 1,

        /// <summary>
        /// Криволинейный
        /// </summary>
        Curvilinear = 2,

        /// <summary>
        /// Цилиндрический
        /// </summary>
        Cylindrical = 3
    }

    public static class BreakLineTypeHelper
    {
        public static List<string> LocalNames = new List<string>
        {
            Language.GetItem(MainFunction.LangItem, "blt1"), // "Линейный",
            Language.GetItem(MainFunction.LangItem, "blt2"), //"Криволинейный",
            Language.GetItem(MainFunction.LangItem, "blt3") //"Цилиндрический"
        };

        public static BreakLineType GetByLocalName(string local)
        {
            if (local == LocalNames[0]) return BreakLineType.Linear;
            if (local == LocalNames[1]) return BreakLineType.Curvilinear;
            if (local == LocalNames[2]) return BreakLineType.Cylindrical;
            return BreakLineType.Linear;
        }

        public static string GetLocalName(BreakLineType breakLineType)
        {
            if (breakLineType == BreakLineType.Linear) return Language.GetItem(MainFunction.LangItem, "blt1");
            if (breakLineType == BreakLineType.Curvilinear) return Language.GetItem(MainFunction.LangItem, "blt2");
            if (breakLineType == BreakLineType.Cylindrical) return Language.GetItem(MainFunction.LangItem, "blt3");
            return Language.GetItem(MainFunction.LangItem, "blt1");
        }

        public static BreakLineType Parse(string str)
        {
            if (str == "Linear") return BreakLineType.Linear;
            if (str == "Curvilinear") return BreakLineType.Curvilinear;
            if (str == "Cylindrical") return BreakLineType.Cylindrical;

            return BreakLineType.Linear;
        }
    }

    public class BreakLineTypeValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BreakLineType breakLine)
                return BreakLineTypeHelper.GetLocalName(breakLine);
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BreakLineTypeHelper.GetByLocalName(value?.ToString());
        }
    }
}