namespace mpESKD.Base
{
    using System;
    using Enums;
    using ModPlusAPI.Annotations;

    public class EntityPropertyAttribute : Attribute
    {
        public EntityPropertyAttribute(
            PropertiesCategory category, 
            int orderIndex,
            string name,
            string displayNameLocalizationKey,
            string descriptionLocalizationKey,
            object defaultValue,
            object minimum, 
            object maximum, 
            PropertyScope propertyScope = PropertyScope.PaletteAndStyleEditor)
        {
            Category = category;
            OrderIndex = orderIndex;
            Name = name;
            DisplayNameLocalizationKey = displayNameLocalizationKey;
            DescriptionLocalizationKey = descriptionLocalizationKey;
            DefaultValue = defaultValue;
            Minimum = minimum;
            Maximum = maximum;
            PropertyScope = propertyScope;
        }
        
        /// <summary>
        /// Категория свойства
        /// </summary>
        public PropertiesCategory Category { get; }

        /// <summary>
        /// Имя свойства
        /// </summary>
        public string Name { get;  }

        /// <summary>
        /// Индекс для сортировки
        /// </summary>
        public int OrderIndex { get; }

        /// <summary>
        /// Ключ локализации для отображаемого имени свойства
        /// </summary>
        public string DisplayNameLocalizationKey { get; }

        /// <summary>
        /// Ключ локализации для описания свойства
        /// </summary>
        public string DescriptionLocalizationKey { get;  }
        
        /// <summary>
        /// Значение по умолчанию
        /// </summary>
        [NotNull]
        public object DefaultValue { get;  }

        /// <summary>
        /// Минимальное значение. Для свойств типа int, double
        /// </summary>
        [CanBeNull]
        public object Minimum { get;  }

        /// <summary>
        /// Максимальное значение. Для свойств типа int, double
        /// </summary>
        [CanBeNull]
        public object Maximum { get;  }

        /// <summary>
        /// Область видимости свойства
        /// </summary>
        public PropertyScope PropertyScope { get; }
    }

    /// <summary>
    /// Атрибут, указывающий ключ локализации для имени интеллектуального примитива
    /// </summary>
    public class IntellectualEntityDisplayNameKeyAttribute : Attribute
    {
        public IntellectualEntityDisplayNameKeyAttribute(string localizationKey)
        {
            LocalizationKey = localizationKey;
        }

        public string LocalizationKey { get; }
    }

    /// <summary>
    /// Атрибут, указывающий ключ локализации для поля перечислителя, используемого в свойствах примитива
    /// </summary>
    public class EnumPropertyDisplayValueKeyAttribute : Attribute
    {
        public EnumPropertyDisplayValueKeyAttribute(string localizationKey)
        {
            LocalizationKey = localizationKey;
        }

        public string LocalizationKey { get; }
    }

    /// <summary>
    /// Атрибут, указывающий, что точка является источником для привязки
    /// </summary>
    public class PointForOsnapAttribute : Attribute
    {

    }

    /// <summary>
    /// Атрибут, указывающий, что список точек является источником для привязки
    /// </summary>
    public class ListOfPointsForOsnapAttribute : Attribute
    {

    }
}
