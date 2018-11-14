#pragma warning disable CS0618
namespace mpESKD.Base.Properties
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Enums;
    using Helpers;
    using ModPlusAPI.Annotations;
    using Styles;

    public class EntityPropertyData
    {
        public EntityPropertyData(ObjectId blkRefObjectId)
        {
            if (Verify(blkRefObjectId))
            {
                IsValid = true;
                _blkRefObjectId = blkRefObjectId;
                using (BlockReference blkRef = blkRefObjectId.Open(OpenMode.ForRead, false, true) as BlockReference)
                {
                    if (blkRef != null)
                    {
                        blkRef.Modified += BlkRef_Modified;
                        Create(blkRef);
                    }
                }
            }
            else IsValid = false;
        }

        public string EntityName { get; private set; }

        private ObjectId _blkRefObjectId;

        private IntellectualEntity _intellectualEntity;

        public ObservableCollection<IntellectualEntityProperty> Properties { get; } = new ObservableCollection<IntellectualEntityProperty>();

        public bool IsValid { get; set; }

        public bool Verify(ObjectId breakLineObjectId)
        {
            return !breakLineObjectId.IsNull &&
                   breakLineObjectId.IsValid &
                   !breakLineObjectId.IsErased &
                   !breakLineObjectId.IsEffectivelyErased;
        }

        private void BlkRef_Modified(object sender, EventArgs e)
        {
            BlockReference blkRef = sender as BlockReference;
            if (blkRef != null)
                Update(blkRef);
        }

        private void Create(BlockReference blockReference)
        {
            if (blockReference == null)
            {
                _blkRefObjectId = ObjectId.Null;
                return;
            }
            var intellectualEntity = new EntityReaderFactory().GetFromEntity(blockReference);
            if (intellectualEntity != null)
            {
                _intellectualEntity = intellectualEntity;

                var type = intellectualEntity.GetType();
                EntityName = type.Name;
                foreach (var propertyInfo in type.GetProperties().Where(x => x.GetCustomAttribute<EntityPropertyAttribute>() != null))
                {
                    var att = propertyInfo.GetCustomAttribute<EntityPropertyAttribute>();
                    if (att != null)
                    {
                        if (att.PropertyScope == PropertyScope.None)
                            continue;
                        if (att.Name == "Style")
                        {
                            IntellectualEntityProperty property = new IntellectualEntityProperty(
                                att,
                                type,
                                StyleManager.GetStyleNameByGuid(type.Name + "Style",  _intellectualEntity.StyleGuid),
                                _blkRefObjectId);
                            property.PropertyChanged += Property_PropertyChanged;
                            Properties.Add(property);
                        }
                        else if (att.Name == "LineType")
                        {
                            IntellectualEntityProperty property = new IntellectualEntityProperty(att, type, blockReference.Linetype, _blkRefObjectId);
                            property.PropertyChanged += Property_PropertyChanged;
                            Properties.Add(property);
                        }
                        else
                        {
                            var value = propertyInfo.GetValue(intellectualEntity);
                            if (value != null)
                            {
                                IntellectualEntityProperty property = new IntellectualEntityProperty(att, type, value, _blkRefObjectId);
                                property.PropertyChanged += Property_PropertyChanged;
                                Properties.Add(property);
                            }
                        }
                    }
                }
                
                AnyPropertyChangedRaise();
            }
        }

        //todo В методе обновления нужно повторно прочитать данные и заменить их, а не заполнить
        // также нужно уведомить об этом палитру. Сейчас не работает никак
        private void Update(BlockReference blockReference)
        {
            if (blockReference == null)
            {
                _blkRefObjectId = ObjectId.Null;
                return;
            }
            var intellectualEntity = new EntityReaderFactory().GetFromEntity(blockReference);
            if (intellectualEntity != null)
            {
                _intellectualEntity = intellectualEntity;

                var type = intellectualEntity.GetType();

                foreach (var propertyInfo in type.GetProperties().Where(x => x.GetCustomAttribute<EntityPropertyAttribute>() != null))
                {
                    var att = propertyInfo.GetCustomAttribute<EntityPropertyAttribute>();
                    if (att != null)
                    {
                        if (att.PropertyScope == PropertyScope.None)
                            continue;
                        if (att.Name == "Style")
                        {
                            IntellectualEntityProperty property = new IntellectualEntityProperty(
                                att,
                                type,
                                StyleManager.GetStyleNameByGuid(type.Name + "Style",  _intellectualEntity.StyleGuid),
                                _blkRefObjectId);
                            property.PropertyChanged += Property_PropertyChanged;
                            Properties.Add(property);
                        }
                        else if (att.Name == "LineType")
                        {
                            IntellectualEntityProperty property = new IntellectualEntityProperty(att, type, blockReference.Linetype, _blkRefObjectId);
                            property.PropertyChanged += Property_PropertyChanged;
                            Properties.Add(property);
                        }
                        else
                        {
                            var value = propertyInfo.GetValue(intellectualEntity);
                            if (value != null)
                            {
                                IntellectualEntityProperty property = new IntellectualEntityProperty(att, type, value, _blkRefObjectId);
                                property.PropertyChanged += Property_PropertyChanged;
                                Properties.Add(property);
                            }
                        }
                    }
                }
                AnyPropertyChangedRaise();
            }
        }

        private void Property_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IntellectualEntityProperty intellectualEntityProperty = (IntellectualEntityProperty) sender;
            using (AcadHelpers.Document.LockDocument())
            {
                using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                {
                    Type type = _intellectualEntity.GetType();
                    PropertyInfo propertyInfo = type.GetProperty(intellectualEntityProperty.Name);
                    if (propertyInfo != null)
                    {
                        if (intellectualEntityProperty.Name == "Style")
                        {
                            var style = StyleManager.GetStyles(type.Name + "Style")
                                .FirstOrDefault(sn => sn.Name.Equals(intellectualEntityProperty.Value.ToString()));
                            if (style != null)
                            {
                                _intellectualEntity.StyleGuid = style.Guid;
                                _intellectualEntity.ApplyStyle(style);
                            }
                        }
                        else if (intellectualEntityProperty.Name == "LineType")
                        {
                            if (blkRef != null)
                                blkRef.Linetype = intellectualEntityProperty.Value.ToString();
                        }
                        else 
                            propertyInfo.SetValue(_intellectualEntity, intellectualEntityProperty.Value);

                        _intellectualEntity.UpdateEntities();
                        _intellectualEntity.GetBlockTableRecordWithoutTransaction(blkRef);
                        using (var resBuf = _intellectualEntity.GetParametersForXData())
                        {
                            if (blkRef != null)
                                blkRef.XData = resBuf;
                        }

                        if (blkRef != null)
                            blkRef.ResetBlock();
                    }
                }
            }

            Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
        }

        public event EventHandler AnyPropertyChanged;

        /// <summary>Вызов события изменения какого-либо свойства</summary>
        protected void AnyPropertyChangedRaise()
        {
            AnyPropertyChanged?.Invoke(this, null);
        }
    }

    //todo Move to file
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
            if (value.GetType() == typeof(AnnotationScale))
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
        
        public ObjectId OwnerObjectId { get; }

        public PropertiesCategory Category { get; }

        public int OrderIndex { get; }

        public string Name { get; }

        public string DisplayNameLocalizationKey { get; }

        public string DescriptionLocalizationKey { get; }
        
        public object DefaultValue { get; }

        //todo notify?!
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