namespace mpESKD.Base
{
    using System;
    using System.Runtime.CompilerServices;
    using Enums;
    using ModPlusAPI.Annotations;

    public class EntityPropertyAttribute : Attribute
    {
        public EntityPropertyAttribute(
            PropertiesCategory category, 
            int orderIndex,
            string displayNameLocalizationKey,
            string descriptionLocalizationKey,
            object defaultValue,
            object minimum, 
            object maximum, 
            PropertyScope propertyScope = PropertyScope.PaletteAndStyleEditor,
            [CallerMemberName] string name = null)
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

        /// <summary>
        /// Ключ получения локализованного значения
        /// </summary>
        public string LocalizationKey { get; }
    }

    /// <summary>
    /// Атрибут, указывающий ключ локализации для имени свойства примитива, отображаемое в
    /// редакторе стилей. Атрибут нужно задавать для тех свойств, отображаемое имя которых отличается
    /// в палитре свойств и в редакторе (обычно, когда в скобках нужно указать значение с картинки)
    /// </summary>
    public class PropertyNameKeyInStyleEditor : Attribute
    {
        public PropertyNameKeyInStyleEditor(string localizationKey)
        {
            LocalizationKey = localizationKey;
        }

        /// <summary>
        /// Ключ получения локализованного значения
        /// </summary>
        public string LocalizationKey { get; }
    }

    /// <summary>
    /// Атрибут, указывающий зависимость видимости свойства от другого свойства.
    /// Принцип работы: в классе примитива нужно создать специальное свойство типа bool к которому
    /// нужно указать данный атрибут. 
    /// В список зависимых свойств DependencyProperties нужно внести имена тех свойств, видимость которых
    /// зависит от свойства, для которого установлен атрибут
    /// Самой свойство, для которого установлен атрибут, должно менять внутри класса примитива
    /// </summary>
    public class PropertyVisibilityDependencyAttribute : Attribute
    {
        public PropertyVisibilityDependencyAttribute(string[] dependencyProperties)
        {
            DependencyProperties = dependencyProperties;
        }
        
        /// <summary>
        /// Свойства, видимость которых зависит от свойства, для которого установлен атрибут
        /// </summary>
        public string[] DependencyProperties { get; }

    }

    /// <summary>
    /// Атрибут указывает, что данное свойство интеллектуального примитива сохраняется в XData блока.
    /// Свойство может быть как публичное, так и приватное. Может отображаться в палитре свойств и редакторе стилей, а может не
    /// отображаться
    /// <remarks>Работает со свойствами типа: int, double, bool, Enum, Point3d, List(Point3d).
    /// Остальные типы желательно добавить в метод <see cref="IntellectualEntity.SetPropertiesValuesFromXData"/></remarks>
    /// </summary>
    public class SaveToXDataAttribute : Attribute
    {
    }
}
