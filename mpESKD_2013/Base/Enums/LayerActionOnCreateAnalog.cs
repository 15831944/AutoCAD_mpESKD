namespace mpESKD.Base.Enums
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Действие со слоем при создании аналога
    /// </summary>
    public enum LayerActionOnCreateAnalog
    {
        /// <summary>
        /// Копировать
        /// </summary>
        Copy = 0,

        /// <summary>
        /// Не копировать
        /// </summary>
        NotCopy = 1,

        /// <summary>
        /// Спросить
        /// </summary>
        Ask = 2
    }

    public class LayerActionOnCreateAnalogValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return (int)(LayerActionOnCreateAnalog)value;
            }

            throw new Exception("Cannot convert LayerActionOnCreateAnalog");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return (LayerActionOnCreateAnalog)(int)value;
            }

            throw new Exception("Cannot convert back LayerActionOnCreateAnalog");
        }
    }
}
