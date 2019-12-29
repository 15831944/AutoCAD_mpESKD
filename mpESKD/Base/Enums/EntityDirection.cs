namespace mpESKD.Base.Enums
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Свойство "Направление" для примитивов ЕСКД. Может использоваться различными примитивами
    /// </summary>
    public enum EntityDirection
    {
        /// <summary>
        /// Слева на право
        /// </summary>
        LeftToRight,

        /// <summary>
        /// Справа на лево
        /// </summary>
        RightToLeft,

        /// <summary>
        /// Сверху вниз
        /// </summary>
        UpToBottom,

        /// <summary>
        /// Снизу вверх
        /// </summary>
        BottomToUp
    }

    public static class EntityDirectionHelper
    {
        /// <summary>
        /// Список локализованных значений перечисления
        /// </summary>
        public static List<string> LocalNames = new List<string>
        {
            "Слева на право",
            "Справа на лево",
            "Сверху вниз",
            "Снизу вверх"
        };

        /// <summary>
        /// Получить значение перечислителя по локализованному значению 
        /// </summary>
        public static EntityDirection GetByLocalName(string local)
        {
            if (local == LocalNames[0])
            {
                return EntityDirection.LeftToRight;
            }

            if (local == LocalNames[1])
            {
                return EntityDirection.RightToLeft;
            }

            if (local == LocalNames[2])
            {
                return EntityDirection.UpToBottom;
            }

            if (local == LocalNames[3])
            {
                return EntityDirection.BottomToUp;
            }

            return EntityDirection.LeftToRight;
        }

        /// <summary>
        /// Получить локализованное значение по перечислителю
        /// </summary>
        public static string GetLocalName(EntityDirection entityDirection)
        {
            if (entityDirection == EntityDirection.LeftToRight)
            {
                return LocalNames[0];
            }

            if (entityDirection == EntityDirection.RightToLeft)
            {
                return LocalNames[1];
            }

            if (entityDirection == EntityDirection.UpToBottom)
            {
                return LocalNames[2];
            }

            if (entityDirection == EntityDirection.BottomToUp)
            {
                return LocalNames[3];
            }

            return LocalNames[0];
        }

        /// <summary>
        /// Парсинг из строки
        /// </summary>
        public static EntityDirection Parse(string str)
        {
            if (str == "LeftToRight")
            {
                return EntityDirection.LeftToRight;
            }

            if (str == "RightToLeft")
            {
                return EntityDirection.RightToLeft;
            }

            if (str == "UpToBottom")
            {
                return EntityDirection.UpToBottom;
            }

            if (str == "BottomToUp")
            {
                return EntityDirection.BottomToUp;
            }

            return EntityDirection.LeftToRight;
        }
    }

    public class EntityDirectionValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EntityDirection entityDirection)
            {
                return EntityDirectionHelper.GetLocalName(entityDirection);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return EntityDirectionHelper.GetByLocalName(value?.ToString());
        }
    }
}
