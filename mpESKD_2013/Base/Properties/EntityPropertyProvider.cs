#pragma warning disable CS0618
namespace mpESKD.Base.Properties
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using Autodesk.AutoCAD.DatabaseServices;
    using Enums;
    using Helpers;
    using Styles;

    public class EntityPropertyProvider
    {
        public EntityPropertyProvider(ObjectId blkRefObjectId)
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

        /// <summary>
        /// Имя примитива. Соответствует имени типа (например, Axis, GroundLine и т.п.)
        /// </summary>
        public string EntityName { get; private set; }

        /// <summary>
        /// Идентификатор вставки блока
        /// </summary>
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
                    var attribute = propertyInfo.GetCustomAttribute<EntityPropertyAttribute>();
                    if (attribute != null)
                    {
                        if (attribute.PropertyScope == PropertyScope.None)
                            continue;
                        if (attribute.Name == "Style")
                        {
                            IntellectualEntityProperty property = new IntellectualEntityProperty(
                                attribute,
                                type,
                                StyleManager.GetStyleNameByGuid(type.Name + "Style",  _intellectualEntity.StyleGuid),
                                _blkRefObjectId);
                            property.PropertyChanged += Property_PropertyChanged;
                            Properties.Add(property);
                        }
                        else if (attribute.Name == "LayerName")
                        {
                            IntellectualEntityProperty property = new IntellectualEntityProperty(attribute, type, blockReference.Layer, _blkRefObjectId);
                            property.PropertyChanged += Property_PropertyChanged;
                            Properties.Add(property);
                        }
                        else if (attribute.Name == "LineType")
                        {
                            IntellectualEntityProperty property = new IntellectualEntityProperty(attribute, type, blockReference.Linetype, _blkRefObjectId);
                            property.PropertyChanged += Property_PropertyChanged;
                            Properties.Add(property);
                        }
                        else if (attribute.Name.Contains("TextStyle"))
                        {
                            //todo release
                        }
                        else
                        {
                            var value = propertyInfo.GetValue(intellectualEntity);
                            if (value != null)
                            {
                                IntellectualEntityProperty property = new IntellectualEntityProperty(attribute, type, value, _blkRefObjectId);
                                property.PropertyChanged += Property_PropertyChanged;
                                Properties.Add(property);
                            }
                        }
                    }
                }
            }
        }

        private bool _isModifiedFromAutocad;

        /// <summary>
        /// Обработка события изменения примитива.
        /// Так как изменения свойства в палитре тоже вызывает изменение примитива, то в этом
        /// методе происходит зацикливание. Чтобы это исправить вводим дополнительную переменную
        /// <see cref="_isModifiedFromAutocad"/> чтобы не вызывать обработку события изменения
        /// свойства <see cref="Property_PropertyChanged"/>
        /// </summary>
        private void Update(BlockReference blockReference)
        {
            _isModifiedFromAutocad = true;
            try
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
                        var attribute = propertyInfo.GetCustomAttribute<EntityPropertyAttribute>();
                        if (attribute != null)
                        {
                            if (attribute.PropertyScope == PropertyScope.None)
                                continue;
                            foreach (IntellectualEntityProperty property in Properties)
                            {
                                if (property.Name == attribute.Name)
                                {
                                    if (attribute.Name == "Style")
                                    {
                                        property.Value = StyleManager.GetStyleNameByGuid(type.Name + "Style", _intellectualEntity.StyleGuid);
                                    }
                                    else if (attribute.Name == "LayerName")
                                    {
                                        property.Value = blockReference.Layer;
                                    }
                                    else if (attribute.Name == "LineType")
                                    {
                                        property.Value = blockReference.Linetype;
                                    }
                                    else
                                    {
                                        property.Value = propertyInfo.GetValue(intellectualEntity);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }
            _isModifiedFromAutocad = false;
        }

        private void Property_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(_isModifiedFromAutocad)
                return;

            IntellectualEntityProperty intellectualEntityProperty = (IntellectualEntityProperty) sender;
            using (AcadHelpers.Document.LockDocument())
            {
                using (var blockReference = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                {
                    Type type = _intellectualEntity.GetType();
                    PropertyInfo propertyInfo = type.GetProperty(intellectualEntityProperty.Name);
                    if (propertyInfo != null)
                    {
                        //todo change by intellectualStyle with StyleManager
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
                        else if (intellectualEntityProperty.Name == "LayerName")
                        {
                            if (blockReference != null)
                                blockReference.Layer = intellectualEntityProperty.Value.ToString();
                        }
                        else if (intellectualEntityProperty.Name == "LineType")
                        {
                            if (blockReference != null)
                                blockReference.Linetype = intellectualEntityProperty.Value.ToString();
                        }
                        else 
                            propertyInfo.SetValue(_intellectualEntity, intellectualEntityProperty.Value);

                        _intellectualEntity.UpdateEntities();
                        _intellectualEntity.GetBlockTableRecordWithoutTransaction(blockReference);
                        using (var resBuf = _intellectualEntity.GetParametersForXData())
                        {
                            if (blockReference != null)
                                blockReference.XData = resBuf;
                        }

                        if (blockReference != null)
                            blockReference.ResetBlock();
                    }
                }
            }

            Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
        }
    }
}