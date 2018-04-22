using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace mpESKD.Base.Properties.Converters
{
    public class TextInputToVisibilityConverterForInt : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values[0] == null || values[0] is string || values[0] is int)
                && values[1] is string
                && values[2] is bool)
            {
                object val = values[0];
                bool hasTxt = !(string.IsNullOrEmpty((string)values[1]));
                bool focused = (bool)values[2];

                bool valIsNull = val == null;
                bool valIsEmptyString = val is string && val.Equals(string.Empty);
                bool valIsNanDouble = val is int && double.IsNaN((int)val);

                if ((!valIsNull && !valIsEmptyString && !valIsNanDouble)
                    || hasTxt
                    || focused)
                {
                    return Visibility.Collapsed;
                }
            }
            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <inheritdoc />
    /// <summary>Конвертер для определения: отображать ли текст с подсказкой в поле ввода</summary>
    public class TextInputToVisibilityConverterForDouble : IMultiValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values">
        /// Массив объектов.
        /// Первый элемент - значение свойства.
        /// Второй элемент - значение из поля ввода.
        /// Третий элемент - имеет ли поле для ввода фокус (ввода с клавиатуры).
        /// </param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert
            (object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values[0] == null || values[0] is string || values[0] is double)
                && values[1] is string
                && values[2] is bool)
            {
                object val = values[0];
                bool hasTxt = !(string.IsNullOrEmpty((string)values[1]));
                bool focused = (bool)values[2];

                bool valIsNull = val == null;
                bool valIsEmptyString = val is string && val.Equals(string.Empty);
                bool valIsNanDouble = val is double && double.IsNaN((double)val);

                if ((!valIsNull && !valIsEmptyString && !valIsNanDouble)
                    || hasTxt
                    || focused)
                {
                    return Visibility.Collapsed;
                }
            }
            return Visibility.Visible;
        }

        public object[] ConvertBack
            (object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
