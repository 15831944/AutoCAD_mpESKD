#pragma warning disable CS0618
namespace mpESKD.Base.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Autodesk.AutoCAD.DatabaseServices;
    using Helpers;
    using ModPlusAPI.Annotations;

    public class EntityPropertyData
    {
        public EntityPropertyData(ObjectId blkRefObjectId)
        {
            if (Verify(blkRefObjectId))
            {
                IsValid = true;
                BlkRefObjectId = blkRefObjectId;
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

        public ObjectId BlkRefObjectId;

        public List<EntityProperty> Properties { get; } = new List<EntityProperty>();

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
                BlkRefObjectId = ObjectId.Null;
                return;
            }
            var mpcoEntity  = new EntityReaderFactory().GetFromEntity(blockReference);
            if (mpcoEntity != null)
            {
                var type = mpcoEntity.GetType();
                foreach (var propertyInfo in type.GetProperties().Where(x => x.GetCustomAttribute<EntityPropertyAttribute>() != null))
                {
                    var att = propertyInfo.GetCustomAttribute<EntityPropertyAttribute>();
                    AcadHelpers.WriteMessageInDebug($"Attr category: {att.Category}");
                    AcadHelpers.WriteMessageInDebug($"Attr name: {att.Name}");

                    var value = propertyInfo.GetValue(mpcoEntity);
                    AcadHelpers.WriteMessageInDebug($"Property value: {value}");
                }

                //    _style = StyleManager.GetStyles<GroundLineStyle>().FirstOrDefault(s => s.Guid.Equals(groundLine.StyleGuid))?.Name;
                //    _firstStrokeOffset = GroundLinePropertiesHelpers.GetLocalFirstStrokeOffsetName(groundLine.FirstStrokeOffset);
                //    _strokeLength = groundLine.StrokeLength;
                //    _strokeOffset = groundLine.StrokeOffset;
                //    _strokeAngle = groundLine.StrokeAngle;
                //    _space = groundLine.Space;
                //    _scale = groundLine.Scale.Name;
                //    _layerName = blockReference.Layer;
                //    _lineTypeScale = groundLine.LineTypeScale;
                //    _lineType = blockReference.Linetype;
                //    AnyPropertyChangedReise();
            }
        }
    }

    //todo Move to file
    public class EntityProperty
    {
        public EntityProperty(EntityPropertyAttribute attribute, object value)
        {
            Category = attribute.Category;
            Name = attribute.Name;
            DisplayName = attribute.DisplayName;
            Description = attribute.Description;
            ValueType = typeof(object);
            DefaultValue = attribute.DefaultValue;
            Minimum = attribute.Minimum;
            Maximum = attribute.Maximum;
            Value = value;
        }
        public string Category { get; }

        public string Name { get;  }

        public string DisplayName { get; }

        public string Description { get;  }

        public Type ValueType { get;  }
        
        public object DefaultValue { get;  }

        //todo notify?!
        public object Value { get; set; }

        [CanBeNull]
        public object Minimum { get;  }

        [CanBeNull]
        public object Maximum { get;  }
    }
}