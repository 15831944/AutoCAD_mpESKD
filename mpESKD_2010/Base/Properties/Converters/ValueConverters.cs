using System;
using System.Globalization;
using System.Windows.Data;

namespace mpESKD.Base.Properties.Converters
{
    public class IntValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Если преобразовываем в строку
            if (targetType == typeof(string)
                && (value == null || value is int))
            {
                if (value == null)
                    return "*РАЗЛИЧНЫЕ*";
                if (double.IsNaN((int) value))
                    return "*НЕ ОПРЕДЕЛЕНО*";
                return string.Empty;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Вспомогательное преобразование числового значения в строковое
    /// для использования во вспомогательном текстовом окне.
    /// Оно отображается, только когда значение не может быть отображено
    /// </summary>
    public class DoubleValueConverter : IValueConverter
    {
        public object Convert
            (object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Если преобразовываем в строку
            if (targetType == typeof(string)
                && (value == null || value is double))
            {
                if (value == null)
                    return "*РАЗЛИЧНЫЕ*";
                if (double.IsNaN((double) value))
                    return "*НЕ ОПРЕДЕЛЕНО*";
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
}
