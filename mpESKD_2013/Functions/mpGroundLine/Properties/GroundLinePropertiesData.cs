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

        private string _firstStrokeOffset;
        public string FirstStrokeOffset
        {
            get => _firstStrokeOffset;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = GroundLine.GetGroundLineFromEntity(blkRef))
                        {
                            axis.FirstStrokeOffset = GroundLinePropertiesHelpers.GetFirstStrokeOffsetByLocalName(value);
                            axis.UpdateEntities();
                            axis.GetBlockTableRecordWithoutTransaction(blkRef);
                            using (var resBuf = axis.GetParametersForXData())
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

        private int _strokeLength;
        public int StrokeLength
        {
            get => _strokeLength;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = GroundLine.GetGroundLineFromEntity(blkRef))
                        {
                            axis.StrokeLength = value;
                            axis.UpdateEntities();
                            axis.GetBlockTableRecordWithoutTransaction(blkRef);
                            using (var resBuf = axis.GetParametersForXData())
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

        private int _strokeOffset;
        public int StrokeOffset
        {
            get => _strokeOffset;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = GroundLine.GetGroundLineFromEntity(blkRef))
                        {
                            axis.StrokeOffset = value;
                            axis.UpdateEntities();
                            axis.GetBlockTableRecordWithoutTransaction(blkRef);
                            using (var resBuf = axis.GetParametersForXData())
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

        private int _strokeAngle;
        public int StrokeAngle
        {
            get => _strokeAngle;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = GroundLine.GetGroundLineFromEntity(blkRef))
                        {
                            axis.StrokeAngle = value;
                            axis.UpdateEntities();
                            axis.GetBlockTableRecordWithoutTransaction(blkRef);
                            using (var resBuf = axis.GetParametersForXData())
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

        private int _space;
        public int Space
        {
            get => _space;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = GroundLine.GetGroundLineFromEntity(blkRef))
                        {
                            axis.Space = value;
                            axis.UpdateEntities();
                            axis.GetBlockTableRecordWithoutTransaction(blkRef);
                            using (var resBuf = axis.GetParametersForXData())
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

        private string _lineType;

        /// <summary>Слой</summary>
        public string LineType
        {
            get => _lineType;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        if (blkRef != null) blkRef.Linetype = value;
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
            var groundLine = GroundLine.GetGroundLineFromEntity(blkReference);
            if (groundLine != null)
            {
                _style = GroundLineStyleManager.Styles.FirstOrDefault(s => s.Guid.Equals(groundLine.StyleGuid))?.Name;
                _firstStrokeOffset = GroundLinePropertiesHelpers.GetLocalFirstStrokeOffsetName(groundLine.FirstStrokeOffset);
                _strokeLength = groundLine.StrokeLength;
                _strokeOffset = groundLine.StrokeOffset;
                _strokeAngle = groundLine.StrokeAngle;
                _space = groundLine.Space;
                _scale = groundLine.Scale.Name;
                _layerName = blkReference.Layer;
                _lineTypeScale = groundLine.LineTypeScale;
                _lineType = blkReference.Linetype;
                AnyPropertyChangedReise();
            }
        }
    }
}
