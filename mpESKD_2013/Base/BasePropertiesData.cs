#pragma warning disable CS0618
//todo remove it
namespace mpESKD.Base
{
    using System;
    using Autodesk.AutoCAD.DatabaseServices;
    using Helpers;
    using Properties;
    using Styles;

    public abstract class BasePropertiesData : IGeneralProperties
    {
        protected BasePropertiesData(ObjectId blkRefObjectId)
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

        public bool IsValid { get; set; }
        
        public bool Verify(ObjectId breakLineObjectId)
        {
            return !breakLineObjectId.IsNull &&
                   breakLineObjectId.IsValid &
                   !breakLineObjectId.IsErased &
                   !breakLineObjectId.IsEffectivelyErased;
        }

        #region General abstract properties

        /// <inheritdoc />
        public abstract string Style { get; set; }

        /// <inheritdoc />
        public abstract string Scale { get; set; }

        /// <inheritdoc />
        public abstract double LineTypeScale { get; set; }

        /// <inheritdoc />
        public abstract string LineType { get; set; }

        /// <inheritdoc />
        public abstract string LayerName { get; set; }

        #endregion

        /// <summary>
        /// Изменение свойства
        /// </summary>
        /// <typeparam name="T">Экземпляр класса, унаследованный от <see cref="IntellectualEntity"/></typeparam>
        /// <param name="getEntityFunc">Метод получения экземпляра класса из вставки блока</param>
        /// <param name="updateProp">Метод обновления свойства</param>
        public void ChangeProperty<T> (Func<BlockReference, T> getEntityFunc, Action<T> updateProp) where T: IntellectualEntity
        {
            using (AcadHelpers.Document.LockDocument())
            {
                using (var blkRef = BlkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                {
                    using (T entity = getEntityFunc(blkRef))
                    {
                        updateProp(entity);
                        entity.UpdateEntities();
                        entity.GetBlockTableRecordWithoutTransaction(blkRef);
                        using (var resBuf = entity.GetParametersForXData())
                        {
                            if (blkRef != null) blkRef.XData = resBuf;
                        }
                    }
                    if (blkRef != null) blkRef.ResetBlock();
                }
            }
            Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
        }

        public void ChangeScaleProperty<T>(Func<BlockReference, T> getEntityFunc, string scaleValue) where T : IntellectualEntity
        {
            using (AcadHelpers.Document.LockDocument())
            {
                using (var blkRef = BlkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                {
                    using (T entity = getEntityFunc(blkRef))
                    {
                        entity.Scale = AcadHelpers.GetAnnotationScaleByName(scaleValue);
                        entity.UpdateEntities();
                        entity.GetBlockTableRecordWithoutTransaction(blkRef);
                        using (var resBuf = entity.GetParametersForXData())
                        {
                            if (blkRef != null) blkRef.XData = resBuf;
                        }
                    }
                    if (blkRef != null) blkRef.ResetBlock();
                }
            }
            Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
        }

        public void ChangeStyleProperty<T>(
            Func<BlockReference, T> getEntityFunc,
            MPCOStyle style) where T: IntellectualEntity
        {
            using (AcadHelpers.Document.LockDocument())
            {
                using (var blkRef = BlkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                {
                    using (T entity = getEntityFunc(blkRef))
                    {
                        if (style != null)
                        {
                            entity.StyleGuid = style.Guid;
                            //entity.ApplyStyle(style);
                            entity.UpdateEntities();
                            entity.GetBlockTableRecordWithoutTransaction(blkRef);
                            using (var resBuf = entity.GetParametersForXData())
                            {
                                if (blkRef != null) blkRef.XData = resBuf;
                            }
                        }
                    }
                    if (blkRef != null) blkRef.ResetBlock();
                }
            }
            Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
        }

        public void ChangeLineTypeProperty(string lineTypeValue)
        {
            using (AcadHelpers.Document.LockDocument())
            {
                using (var blkRef = BlkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                {
                    if (blkRef != null) 
                        blkRef.Linetype = lineTypeValue;
                }
            }
            Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
        }

        public void ChangeLayerNameProperty(string layerNameValue)
        {
            using (AcadHelpers.Document.LockDocument())
            {
                using (var blkRef = BlkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                {
                    if (blkRef != null)
                        blkRef.Layer = layerNameValue;
                }
            }
            Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
        }

        private void BlkRef_Modified(object sender, EventArgs e)
        {
            BlockReference blkRef = sender as BlockReference;
            if (blkRef != null)
                Update(blkRef);
        }

        public abstract void Update(BlockReference blockReference);

        public event EventHandler AnyPropertyChanged;

        /// <summary>Вызов события изменения какого-либо свойства</summary>
        protected void AnyPropertyChangedReise()
        {
            AnyPropertyChanged?.Invoke(this, null);
        }
    }
}
