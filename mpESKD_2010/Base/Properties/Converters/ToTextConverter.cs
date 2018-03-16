using System;
using System.Globalization;
using System.Windows.Data;
using ModPlusAPI;

namespace mpESKD.Base.Properties.Converters
{
    public class IntToTextConverter : IValueConverter
    {
        private const string LangItem = "mpESKD";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Если целевой тип - строка
            if(targetType == typeof(string) && 
                // и значение является int, или null
                (value is int || value == null))
            {
                // Если числового значения нет
                if (value == null || double.IsNaN((int) value))
                {
                    return string.Empty;
                }
                // Если числовое значение есть, то преобразовываем его в строку
                return System.Convert.ToString((int) value, CultureInfo.InvariantCulture);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Если целевой тип - число
            if ((targetType.Equals(typeof(int)) || targetType.Equals(typeof(int?)))
                // И значение является строкой или равно null
                && (value is string || value == null))
            {
                // Если значение пустое - исключение
                if (string.IsNullOrEmpty((string)value))
                {
                    throw new ApplicationException(Language.GetItem(LangItem, "err3")); // Значение не может быть пустым!
                }
                // Иначе
                else
                {
                    // Пробуем преобразовать строку в число, если удачно
                    int res;
                    if (int.TryParse((string)value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out res))
                    {
                        // Возвращаем число
                        return res;
                    }
                    // Иначе - исключение
                    else
                    {
                        throw new ApplicationException(Language.GetItem(LangItem, "err4")); // Недопустимое значение! Введите число!
                    }
                }
            }
            // Если целевой тип не число - исключение
            else
                throw new NotImplementedException();
        }
    }
    /// <inheritdoc />
    /// <summary>Конвертер для отображения значения в основном поле ввода</summary>
    public class DoubleToTextConverter : IValueConverter
    {
        private const string LangItem = "mpESKD";
        /// <inheritdoc />
        /// <summary>
        /// Преобразование значения для отображения в поле ввода
        /// </summary>
        /// <param name="value">Значение (д.б. всгда double или double?)</param>
        /// <param name="targetType">Целевой тип (д.б. всегда string)</param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Если целевой тип - строка
            if (targetType.Equals(typeof(string))
                // и значение является double, или null
                && (value is double || value == null))
            {
                // Если числового значения нет
                if (value == null || double.IsNaN((double)value))
                {
                    // Пустая строка
                    return string.Empty;
                }
                // Если числовое значение есть
                else
                {
                    // Преобразуем его в строку
                    return System.Convert.ToString
                        ((double)value, CultureInfo.InvariantCulture);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Преобразование значения из поля ввода в числовое
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Если целевой тип - число
            if ((targetType.Equals(typeof(double)) || targetType.Equals(typeof(double?)))
                // И значение является строкой или равно null
                && (value is string || value == null))
            {
                // Если значение пустое - исключение
                if (string.IsNullOrEmpty((string)value))
                {
                    throw new ApplicationException(Language.GetItem(LangItem, "err3")); // Значение не может быть пустым!
                }
                // Иначе
                else
                {
                    // Пробуем преобразовать строку в число, если удачно
                    double res;
                    if (double.TryParse((string)value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out res))
                    {
                        // Возвращаем число
                        return res;
                    }
                    // Иначе - исключение
                    else
                    {
                        throw new ApplicationException(Language.GetItem(LangItem, "err4")); // Недопустимое значение! Введите число!
                    }
                }
            }
            // Если целевой тип не число - исключение
            else
                throw new NotImplementedException();
        }
    }
}
