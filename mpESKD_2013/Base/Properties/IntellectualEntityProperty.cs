namespace mpESKD.Base.Properties
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Enums;
    using Helpers;
    using ModPlusAPI.Annotations;

    public class IntellectualEntityProperty : INotifyPropertyChanged
    {
        private object _value;

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
            DescriptionLocalizationKey = attribute.DescriptionLocalizationKey;
            if (value != null && value.GetType() == typeof(AnnotationScale))
                DefaultValue = new AnnotationScale
                {
                    Name = attribute.DefaultValue.ToString(), 
                    DrawingUnits = double.Parse(attribute.DefaultValue.ToString().Split(':')[0]), 
                    PaperUnits = double.Parse(attribute.DefaultValue.ToString().Split(':')[1])
                };
            else DefaultValue = attribute.DefaultValue;

            Minimum = attribute.Minimum;
            Maximum = attribute.Maximum;
            Value = value;
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

        [CanBeNull]
        public object Minimum { get; }

        [CanBeNull]
        public object Maximum { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}