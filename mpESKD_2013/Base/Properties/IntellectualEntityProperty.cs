namespace mpESKD.Base.Properties
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Enums;
    using ModPlusAPI.Annotations;

    public class IntellectualEntityProperty : INotifyPropertyChanged
    {
        private object _value;
        private double _doubleValue;
        private int _intValue;

        public IntellectualEntityProperty(
            EntityPropertyAttribute attribute,
            [CanBeNull] PropertyNameKeyInStyleEditor propertyNameKeyInStyleEditor,
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
            if (propertyNameKeyInStyleEditor != null)
                DisplayNameLocalizationKeyForStyleEditor = propertyNameKeyInStyleEditor.LocalizationKey;
            DescriptionLocalizationKey = attribute.DescriptionLocalizationKey;
            if (value != null && value.GetType() == typeof(AnnotationScale))
                DefaultValue = new AnnotationScale
                {
                    Name = attribute.DefaultValue.ToString(), 
                    DrawingUnits = double.Parse(attribute.DefaultValue.ToString().Split(':')[0]), 
                    PaperUnits = double.Parse(attribute.DefaultValue.ToString().Split(':')[1])
                };
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
                DoubleValue = d;
            if (value is int i)
                IntValue = i;

            PropertyScope = attribute.PropertyScope;
        }

        public Type EntityType { get; }
        
        /// <summary>
        /// Идентификатор блока-владельца. Свойство используется при работе палитры.
        /// При работе со стилями свойство равно ObjectId.Null
        /// </summary>
        public ObjectId OwnerObjectId { get; }

        public PropertiesCategory Category { get; }

        public int OrderIndex { get; }

        public string Name { get; }

        public string DisplayNameLocalizationKey { get; }

        public string DisplayNameLocalizationKeyForStyleEditor { get; } = string.Empty;

        public string DescriptionLocalizationKey { get; }
        
        public object DefaultValue { get; }

        public object Value
        {
            get => _value;
            set
            {
                if (Equals(value, _value)) return;
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
                if (value.Equals(_doubleValue)) return;
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
                if (value == _intValue) return;
                _intValue = value;
                _value = value;
                OnPropertyChanged();
            }
        }

        [CanBeNull]
        public object Minimum { get; }

        [CanBeNull]
        public object Maximum { get; }

        public PropertyScope PropertyScope { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}