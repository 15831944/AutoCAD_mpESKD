#pragma warning disable CS0618
namespace mpESKD.Base.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Enums;
    using Helpers;
    using ModPlusAPI.Annotations;

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
                        Update(blkRef);
                    }
                }
            }
            else IsValid = false;
        }

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
                    var value = propertyInfo.GetValue(intellectualEntity);
                    if (att != null && value != null)
                    {
                        IntellectualEntityProperty property = new IntellectualEntityProperty(att, value);
                        property.PropertyChanged += Property_PropertyChanged;
                        Properties.Add(property);
                    }
                }
                AnyPropertyChangedRaise();
            }
        }

        private void Property_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            using (AcadHelpers.Document.LockDocument())
            {
                using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                {
                    //todo wrong!
                    _intellectualEntity.GetType().GetProperty(e.PropertyName).SetValue(_intellectualEntity, 10);
                    _intellectualEntity.UpdateEntities();
                    _intellectualEntity.GetBlockTableRecordWithoutTransaction(blkRef);
                    using (var resBuf = _intellectualEntity.GetParametersForXData())
                    {
                        if (blkRef != null) blkRef.XData = resBuf;
                    }
                    if (blkRef != null)
                        blkRef.ResetBlock();
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

        public IntellectualEntityProperty(EntityPropertyAttribute attribute, object value)
        {
            Category = attribute.Category;
            Name = attribute.Name;
            DisplayNameLocalizationKey = attribute.DisplayNameLocalizationKey;
            DescriptionLocalizationKey = attribute.DescriptionLocalizationKey;
            ValueType = typeof(object);
            DefaultValue = attribute.DefaultValue;
            Minimum = attribute.Minimum;
            Maximum = attribute.Maximum;
            Value = value;
        }

        public PropertiesCategory Category { get; }

        public string Name { get; }

        public string DisplayNameLocalizationKey { get; }

        public string DescriptionLocalizationKey { get; }

        //todo need?
        public Type ValueType { get; }

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