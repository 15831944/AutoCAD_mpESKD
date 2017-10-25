using System;
using Autodesk.AutoCAD.DatabaseServices;
using mpESKD.Base.Helpers;

// ReSharper disable InconsistentNaming
#pragma warning disable CS0618

namespace mpESKD.Functions.mpAxis.Properties
{
    public class AxisPropertiesData
    {
        private ObjectId _blkRefObjectId;

        private string _markersPosition;
        /// <summary>Позиция маркеров</summary>
        public string MarkersPosition
        {
            get => _markersPosition;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = AxisXDataHelper.GetAxisFromEntity(blkRef))
                        {
                            axis.MarkersPosition = AxisPropertiesHelpers.GetAxisMarkersPositionByLocalName(value);
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
        private int _markersCount;
        /// <summary>Позиция маркеров</summary>
        public int MarkersCount
        {
            get => _markersCount;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = AxisXDataHelper.GetAxisFromEntity(blkRef))
                        {
                            axis.MarkersCount = value;
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

        private int _fracture;
        /// <summary>Излом</summary>
        public int Fracture
        {
            get => _fracture;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = AxisXDataHelper.GetAxisFromEntity(blkRef))
                        {
                            axis.Fracture = value;
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

        private int _bottomFractureOffset;
        /// <summary>Нижний отступ излома</summary>
        public int BottomFractureOffset
        {
            get => _bottomFractureOffset;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = AxisXDataHelper.GetAxisFromEntity(blkRef))
                        {
                            axis.BottomFractureOffset = value;
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

        private int _topFractureOffset;
        /// <summary>Верхний отступ излома</summary>
        public int TopFractureOffset
        {
            get => _topFractureOffset;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = AxisXDataHelper.GetAxisFromEntity(blkRef))
                        {
                            axis.TopFractureOffset = value;
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

        private int _markersDiameter;

        public int MarkersDiameter
        {
            get => _markersDiameter;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = AxisXDataHelper.GetAxisFromEntity(blkRef))
                        {
                            axis.MarkersDiameter = value;
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
        /// <summary>Масштаб</summary>
        public string Scale
        {
            get => _scale;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = AxisXDataHelper.GetAxisFromEntity(blkRef))
                        {
                            axis.Scale = AcadHelpers.GetAnnotationScaleByName(value);
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
                        using (var axis = AxisXDataHelper.GetAxisFromEntity(blkRef))
                        {
                            axis.LineTypeScale = value;
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

        public bool IsValid { get; set; }

        public AxisPropertiesData(ObjectId blkRefObjectId)
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
        private void BlkRef_Modified(object sender, EventArgs e)
        {
            BlockReference blkRef = sender as BlockReference;
            if (blkRef != null)
                Update(blkRef);
        }

        void Update(BlockReference blkReference)
        {
            if (blkReference == null)
            {
                _blkRefObjectId = ObjectId.Null;
                return;
            }
            var axis = AxisXDataHelper.GetAxisFromEntity(blkReference);
            if (axis != null)
            {
                _markersCount = axis.MarkersCount;
                _markersDiameter = axis.MarkersDiameter;
                _fracture = axis.Fracture;
                _bottomFractureOffset = axis.BottomFractureOffset;
                _topFractureOffset = axis.TopFractureOffset;
                _markersPosition = AxisPropertiesHelpers.GetLocalAxisMarkersPositionName(axis.MarkersPosition);
                _scale = axis.Scale.Name;
                _layerName = blkReference.Layer;
                _lineTypeScale = axis.LineTypeScale;
                AnyPropertyChangedReise();
            }
        }

        static bool Verify(ObjectId breakLineObjectId)
        {
            return !breakLineObjectId.IsNull &&
                   breakLineObjectId.IsValid &
                   !breakLineObjectId.IsErased &
                   !breakLineObjectId.IsEffectivelyErased;
        }

        public event EventHandler AnyPropertyChanged;
        /// <summary>Вызов события изменения какого-либо свойства</summary>
        protected void AnyPropertyChangedReise()
        {
            AnyPropertyChanged?.Invoke(this, null);
        }
    }
}
