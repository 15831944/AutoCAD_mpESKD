using System;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using mpESKD.Base.Helpers;
using mpESKD.Functions.mpAxis.Styles;

// ReSharper disable InconsistentNaming
#pragma warning disable CS0618

namespace mpESKD.Functions.mpAxis.Properties
{
    public class AxisPropertiesData
    {
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
                        using (var breakLine = AxisXDataHelper.GetAxisFromEntity(blkRef))
                        {
                            var style = AxisStyleManager.Styles.FirstOrDefault(s => s.Name.Equals(value));
                            if (style != null)
                            {
                                breakLine.StyleGuid = style.Guid;
                                breakLine.ApplyStyle(style);
                                breakLine.UpdateEntities();
                                breakLine.GetBlockTableRecordWithoutTransaction(blkRef);
                                using (var resBuf = breakLine.GetParametersForXData())
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

        private string _textStyle;
        public string TextStyle
        {
            get => _textStyle;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = AxisXDataHelper.GetAxisFromEntity(blkRef))
                        {
                            axis.TextStyle = value;
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

        private double _textHeight;
        public double TextHeight
        {
            get => _textHeight;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = AxisXDataHelper.GetAxisFromEntity(blkRef))
                        {
                            axis.TextHeight = value;
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

        #region Типы маркеров

        private string _firstMarkerType;
        public string FirstMarkerType
        {
            get => _firstMarkerType;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = AxisXDataHelper.GetAxisFromEntity(blkRef))
                        {
                            axis.FirstMarkerType = value.Equals("Тип 1") ? 0 : 1;
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

        private string _secondMarkerType;
        public string SecondMarkerType
        {
            get => _secondMarkerType;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = AxisXDataHelper.GetAxisFromEntity(blkRef))
                        {
                            axis.SecondMarkerType = value.Equals("Тип 1") ? 0 : 1;
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

        private string _thirdMarkerType;
        public string ThirdMarkerType
        {
            get => _thirdMarkerType;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = AxisXDataHelper.GetAxisFromEntity(blkRef))
                        {
                            axis.ThirdMarkerType = value.Equals("Тип 1") ? 0 : 1;
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
        #endregion

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
                            var oldScale = axis.GetScale();
                            axis.Scale = AcadHelpers.GetAnnotationScaleByName(value);
                            if (MainStaticSettings.Settings.AxisLineTypeScaleProportionScale)
                            {
                                var newScale = axis.GetScale();
                                axis.LineTypeScale = axis.LineTypeScale * newScale / oldScale;
                            }
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
                _style = AxisStyleManager.Styles.FirstOrDefault(s => s.Guid.Equals(axis.StyleGuid))?.Name;
                _markersCount = axis.MarkersCount;
                _markersDiameter = axis.MarkersDiameter;
                _firstMarkerType = axis.FirstMarkerType == 0 ? "Тип 1" : "Тип 2";
                _secondMarkerType = axis.SecondMarkerType == 0 ? "Тип 1" : "Тип 2";
                _thirdMarkerType = axis.ThirdMarkerType == 0 ? "Тип 1" : "Тип 2";
                _fracture = axis.Fracture;
                _bottomFractureOffset = axis.BottomFractureOffset;
                _topFractureOffset = axis.TopFractureOffset;
                _markersPosition = AxisPropertiesHelpers.GetLocalAxisMarkersPositionName(axis.MarkersPosition);
                _scale = axis.Scale.Name;
                _layerName = blkReference.Layer;
                _lineType = blkReference.Linetype;
                _lineTypeScale = axis.LineTypeScale;
                _textStyle = axis.TextStyle;
                _textHeight = axis.TextHeight;
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
