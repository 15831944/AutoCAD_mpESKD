// ReSharper disable InconsistentNaming
#pragma warning disable CS0618
namespace mpESKD.Functions.mpGroundLine.Properties
{
    using System;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Base;
    using Base.Helpers;
    using Styles;

    public class GroundLinePropertiesData : BasePropertiesData
    {
        public GroundLinePropertiesData(ObjectId blkRefObjectId)
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

        private string _style;

        public string Style
        {
            get => _style;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var groundLine = GroundLine.GetGroundLineFromEntity(blkRef))
                        {
                            var style = GroundLineStyleManager.Styles.FirstOrDefault(s => s.Name.Equals(value));
                            if (style != null)
                            {
                                groundLine.StyleGuid = style.Guid;
                                groundLine.ApplyStyle(style);
                                groundLine.UpdateEntities();
                                groundLine.GetBlockTableRecordWithoutTransaction(blkRef);
                                using (var resBuf = groundLine.GetParametersForXData())
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
        }

        #region General
        
        private string _scale;

        public string Scale
        {
            get => _scale;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var groundLine = GroundLine.GetGroundLineFromEntity(blkRef))
                        {
                            groundLine.Scale = AcadHelpers.GetAnnotationScaleByName(value);
                            groundLine.UpdateEntities();
                            groundLine.GetBlockTableRecordWithoutTransaction(blkRef);
                            using (var resBuf = groundLine.GetParametersForXData())
                            {
                                if (blkRef != null) blkRef.XData = resBuf;
                            }
                        }
                        if (blkRef != null) blkRef.ResetBlock();
                    }
                }
                Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
            }
        }

        private double _lineTypeScale;

        /// <summary>Масштаб типа линии</summary>
        public double LineTypeScale
        {
            get => _lineTypeScale;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var groundLineFromEntity = GroundLine.GetGroundLineFromEntity(blkRef))
                        {
                            groundLineFromEntity.LineTypeScale = value;
                            groundLineFromEntity.UpdateEntities();
                            groundLineFromEntity.GetBlockTableRecordWithoutTransaction(blkRef);
                            using (var resBuf = groundLineFromEntity.GetParametersForXData())
                            {
                                if (blkRef != null) blkRef.XData = resBuf;
                            }
                        }
                        if (blkRef != null) blkRef.ResetBlock();
                    }
                }
                Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
            }
        }

        private string _layerName;

        /// <summary>Слой</summary>
        public string LayerName
        {
            get => _layerName;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        if (blkRef != null) blkRef.Layer = value;
                    }
                }
                Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
            }
        }

        #endregion

        private void BlkRef_Modified(object sender, EventArgs e)
        {
            BlockReference blkRef = sender as BlockReference;
            if (blkRef != null)
                Update(blkRef);
        }

        private void Update(BlockReference blkReference)
        {
            if (blkReference == null)
            {
                _blkRefObjectId = ObjectId.Null;
                return;
            }
            var breakLine = GroundLine.GetGroundLineFromEntity(blkReference);
            if (breakLine != null)
            {
                _style = GroundLineStyleManager.Styles.FirstOrDefault(s => s.Guid.Equals(breakLine.StyleGuid))?.Name;
                ////_overgang = breakLine.Overhang;
                ////_breakHeight = breakLine.BreakHeight;
                ////_breakWidth = breakLine.BreakWidth;
                ////_breakLineType = BreakLinePropertiesHelpers.GetLocalBreakLineTypeName(breakLine.BreakLineType);
                _scale = breakLine.Scale.Name;
                _layerName = blkReference.Layer;
                _lineTypeScale = breakLine.LineTypeScale;
                AnyPropertyChangedReise();
            }
        }
    }
}
