﻿namespace mpESKD.Base.Properties
{
    using System;
    using Attributes;
    using Autodesk.AutoCAD.DatabaseServices;
    using Enums;
    using JetBrains.Annotations;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Свойство интеллектуального объекта
    /// </summary>
    public class IntellectualEntityProperty : VmBase
    {
        private object _value;
        private double _doubleValue;
        private int _intValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntellectualEntityProperty"/> class.
        /// </summary>
        /// <param name="attribute">Атрибут <see cref="EntityPropertyAttribute"/></param>
        /// <param name="entityType">Тип интеллектуального объекта</param>
        /// <param name="value">Значение свойства</param>
        /// <param name="ownerObjectId">Идентификатор блока</param>
        public IntellectualEntityProperty(
            EntityPropertyAttribute attribute,
            Type entityType,
            object value,
            ObjectId ownerObjectId)
        {
            EntityType = entityType;
            OwnerObjectId = ownerObjectId;
            Category = attribute.Category;
            OrderIndex = attribute.OrderIndex;
            Name = attribute.Name;
            DisplayNameLocalizationKey = attribute.DisplayNameLocalizationKey;
            NameSymbolForStyleEditor = attribute.NameSymbol;
            DescriptionLocalizationKey = attribute.DescriptionLocalizationKey;

            if (value != null && value.GetType() == typeof(AnnotationScale))
            {
                DefaultValue = new AnnotationScale
                {
                    Name = attribute.DefaultValue.ToString(),
                    DrawingUnits = double.Parse(attribute.DefaultValue.ToString().Split(':')[0]),
                    PaperUnits = double.Parse(attribute.DefaultValue.ToString().Split(':')[1])
                };
            }
            else if (Name == "LayerName" && string.IsNullOrEmpty(attribute.DefaultValue.ToString()))
            {
                DefaultValue = ModPlusAPI.Language.GetItem(Invariables.LangItem, "defl");
            }
            else
            {
                DefaultValue = attribute.DefaultValue;
            }

            Minimum = attribute.Minimum;
            Maximum = attribute.Maximum;
            Value = value;
            if (value is double d)
            {
                DoubleValue = d;
            }

            if (value is int i)
            {
                IntValue = i;
            }

            PropertyScope = attribute.PropertyScope;
            IsReadOnly = attribute.IsReadOnly;
        }

        /// <summary>
        /// Тип интеллектуального объекта
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// Идентификатор блока-владельца. Свойство используется при работе палитры.
        /// При работе со стилями свойство равно ObjectId.Null
        /// </summary>
        public ObjectId OwnerObjectId { get; }

        /// <summary>
        /// Категория свойства
        /// </summary>
        public PropertiesCategory Category { get; }

        /// <summary>
        /// Индекс порядка расположения свойства в палитре
        /// </summary>
        public int OrderIndex { get; }

        /// <summary>
        /// Имя свойства
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Ключ локализации для отображаемого имени свойства
        /// </summary>
        public string DisplayNameLocalizationKey { get; }

        /// <summary>
        /// Условное обозначение на изображении в редакторе стилей
        /// </summary>
        public string NameSymbolForStyleEditor { get; }

        /// <summary>
        /// Ключ локализации для описания свойства
        /// </summary>
        public string DescriptionLocalizationKey { get; }

        /// <summary>
        /// Значение по умолчанию
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// Значение свойства
        /// </summary>
        public object Value
        {
            get => _value;
            set
            {
                if (Equals(value, _value))
                {
                    return;
                }

                _value = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Подробнее смотри в описании метода <see cref="mpESKD.Base.Styles.StyleEditor.CreateTwoWayBindingForPropertyForNumericValue"/>
        /// </summary>
        public double DoubleValue
        {
            get => _doubleValue;
            set
            {
                if (value.Equals(_doubleValue))
                {
                    return;
                }

                _doubleValue = value;
                _value = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Подробнее смотри в описании метода <see cref="mpESKD.Base.Styles.StyleEditor.CreateTwoWayBindingForPropertyForNumericValue"/>
        /// </summary>
        public int IntValue
        {
            get => _intValue;
            set
            {
                if (value == _intValue)
                {
                    return;
                }

                _intValue = value;
                _value = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Минимальное значение (для int, double)
        /// </summary>
        [CanBeNull]
        public object Minimum { get; }

        /// <summary>
        /// Максимальное значение (для int, double)
        /// </summary>
        [CanBeNull]
        public object Maximum { get; }

        /// <summary>
        /// Область видимости свойства
        /// </summary>
        public PropertyScope PropertyScope { get; }

        /// <summary>
        /// Свойство только для чтения. Используется только в палитре свойств
        /// </summary>
        public bool IsReadOnly { get; }
    }
}