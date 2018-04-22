using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ModPlusAPI;

namespace mpESKD.Base.Properties.Converters
{
    public class IntValueConverter : IValueConverter
    {
        private const string LangItem = "mpESKD";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Если преобразовываем в строку
            if (targetType == typeof(string)
                && (value == null || value is int))
            {
                if (value == null)
                    return "*" + Language.GetItem(LangItem, "vc1") + "*"; // РАЗЛИЧНЫЕ
                if (double.IsNaN((int) value))
                    return "*" + Language.GetItem(LangItem, "vc2") + "*"; // НЕ ОПРЕДЕЛЕНО
                return string.Empty;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <inheritdoc />
    /// <summary>
    /// Вспомогательное преобразование числового значения в строковое
    /// для использования во вспомогательном текстовом окне.
    /// Оно отображается, только когда значение не может быть отображено
    /// </summary>
    public class DoubleValueConverter : IValueConverter
    {
        private const string LangItem = "mpESKD";
        public object Convert
            (object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Если преобразовываем в строку
            if (targetType == typeof(string)
                && (value == null || value is double))
            {
                if (value == null)
                    return "*" + Language.GetItem(LangItem, "vc1") + "*"; // РАЗЛИЧНЫЕ
                if (double.IsNaN((double) value))
                    return "*" + Language.GetItem(LangItem, "vc2") + "*"; // НЕ ОПРЕДЕЛЕНО
                return string.Empty;
            }

            return null;
        }

        public object ConvertBack
            (object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <inheritdoc />
    /// <summary>Конвертер ширины колонки у Grid для использования в одновременном изменении
    /// ширины колонок у всех UserControl в палитре</summary>
    public class ColumnWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !str.Equals("*"))
                return new GridLength(double.Parse(str));
            return new GridLength(1, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GridLength gridLength)
                return gridLength.Value.ToString(CultureInfo.InvariantCulture);
            return "*";
        }
    }
}
