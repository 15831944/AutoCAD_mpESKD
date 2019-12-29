namespace mpESKD.Base.Attributes
{
    using System;
    using System.Runtime.CompilerServices;
    using Enums;
    using JetBrains.Annotations;

    /// <summary>
    /// Атрибут используется для настройки свойств интеллектуальных объектов
    /// </summary>
    public class EntityPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityPropertyAttribute"/> class.
        /// </summary>
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
        public string Name { get; }

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
        public string DescriptionLocalizationKey { get; }

        /// <summary>
        /// Значение по умолчанию
        /// </summary>
        [NotNull]
        public object DefaultValue { get; }

        /// <summary>
        /// Минимальное значение. Для свойств типа int, double
        /// </summary>
        [CanBeNull]
        public object Minimum { get; }

        /// <summary>
        /// Максимальное значение. Для свойств типа int, double
        /// </summary>
        [CanBeNull]
        public object Maximum { get; }

        /// <summary>
        /// Область видимости свойства
        /// </summary>
        public PropertyScope PropertyScope { get; }
    }
}