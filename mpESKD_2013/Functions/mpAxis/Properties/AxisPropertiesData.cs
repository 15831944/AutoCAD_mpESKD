// ReSharper disable InconsistentNaming
#pragma warning disable CS0618

namespace mpESKD.Functions.mpAxis.Properties
{
    using System;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Base.Helpers;
    using Styles;
    using ModPlusAPI;
    using Base;

    public class AxisPropertiesData : BasePropertiesData
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
                        using (var breakLine = Axis.GetAxisFromEntity(blkRef))
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
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
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
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
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
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
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
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            var oldFracture = axis.BottomFractureOffset;
                            axis.BottomFractureOffset = value;
                            // нужно сместить зависимые точки
                            var vecNorm = (axis.EndPoint - axis.InsertionPoint).GetNormal() * (value - oldFracture) * axis.GetScale();
                            axis.BottomOrientPoint = axis.BottomOrientPoint + vecNorm;
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
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
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
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
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
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
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
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
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

        #region Text
        // first text
        private string _firstTextPrefix;
        public string FirstTextPrefix
        {
            get => _firstTextPrefix;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.FirstTextPrefix = value;
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

        private string _firstTextSuffix;
        public string FirstTextSuffix
        {
            get => _firstTextSuffix;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.FirstTextSuffix = value;
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

        private string _firstText;
        public string FirstText
        {
            get => _firstText;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.FirstText = value;
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
        // second text
        private string _secondTextPrefix;
        public string SecondTextPrefix
        {
            get => _secondTextPrefix;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.SecondTextPrefix = value;
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

        private string _secondTextSuffix;
        public string SecondTextSuffix
        {
            get => _secondTextSuffix;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.SecondTextSuffix = value;
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

        private string _secondText;
        public string SecondText
        {
            get => _secondText;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.SecondText = value;
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
        // third text
        private string _thirdTextPrefix;
        public string ThirdTextPrefix
        {
            get => _thirdTextPrefix;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.ThirdTextPrefix = value;
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

        private string _thirdTextSuffix;
        public string ThirdTextSuffix
        {
            get => _thirdTextSuffix;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.ThirdTextSuffix = value;
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

        private string _thirdText;
        public string ThirdText
        {
            get => _thirdText;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.ThirdText = value;
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
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.FirstMarkerType = value.Equals(Language.GetItem(MainFunction.LangItem, "type1")) // "Тип 1"
                                ? 0 : 1;
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
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.SecondMarkerType = value.Equals(Language.GetItem(MainFunction.LangItem, "type1")) // "Тип 1"
                                ? 0 : 1;
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
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.ThirdMarkerType = value.Equals(Language.GetItem(MainFunction.LangItem, "type1")) // "Тип 1"
                                ? 0 : 1;
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

        #region Маркеры ориентира

        private int _arrowSize;
        public int ArrowSize
        {
            get => _arrowSize;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.ArrowsSize = value;
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

        private string _bottomOrientText;
        public string BottomOrientText
        {
            get => _bottomOrientText;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.BottomOrientText = value;
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

        private string _topOrientText;
        public string TopOrientText
        {
            get => _topOrientText;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.TopOrientText = value;
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
        
        private bool _bottomOrientMarkerVisible;
        public bool BottomOrientMarkerVisible
        {
            get => _bottomOrientMarkerVisible;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.BottomOrientMarkerVisible = value;
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

        private bool _topOrientMarkerVisible;
        public bool TopOrientMarkerVisible
        {
            get => _topOrientMarkerVisible;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.TopOrientMarkerVisible = value;
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

        private string _orientMarkerType;
        public string OrientMarkerType
        {
            get => _orientMarkerType;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
                        {
                            axis.OrientMarkerType = value.Equals(Language.GetItem(MainFunction.LangItem, "type1")) // "Тип 1"
                                ? 0 : 1;
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
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
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
                        using (var axis = Axis.GetAxisFromEntity(blkRef))
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

        private void Update(BlockReference blkReference)
        {
            if (blkReference == null)
            {
                _blkRefObjectId = ObjectId.Null;
                return;
            }
            var axis = Axis.GetAxisFromEntity(blkReference);
            if (axis != null)
            {
                _style = AxisStyleManager.Styles.FirstOrDefault(s => s.Guid.Equals(axis.StyleGuid))?.Name;
                _markersCount = axis.MarkersCount;
                _markersDiameter = axis.MarkersDiameter;
                _firstMarkerType = axis.FirstMarkerType == 0
                    ? Language.GetItem(MainFunction.LangItem, "type1") // "Тип 1" 
                    : Language.GetItem(MainFunction.LangItem, "type2"); // "Тип 2";
                _secondMarkerType = axis.SecondMarkerType == 0
                    ? Language.GetItem(MainFunction.LangItem, "type1") //"Тип 1" 
                    : Language.GetItem(MainFunction.LangItem, "type2"); //"Тип 2";
                _thirdMarkerType = axis.ThirdMarkerType == 0
                    ? Language.GetItem(MainFunction.LangItem, "type1") // "Тип 1"
                    : Language.GetItem(MainFunction.LangItem, "type2"); // "Тип 2";
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

                #region text

                _firstTextPrefix = axis.FirstTextPrefix;
                _firstTextSuffix = axis.FirstTextSuffix;
                _firstText = axis.FirstText;
                _secondTextPrefix = axis.SecondTextPrefix;
                _secondTextSuffix = axis.SecondTextSuffix;
                _secondText = axis.SecondText;
                _thirdTextPrefix = axis.ThirdTextPrefix;
                _thirdTextSuffix = axis.ThirdTextSuffix;
                _thirdText = axis.ThirdText;

                #endregion

                #region Orient markers

                _arrowSize = axis.ArrowsSize;
                _bottomOrientText = axis.BottomOrientText;
                _topOrientText = axis.TopOrientText;
                _bottomOrientMarkerVisible = axis.BottomOrientMarkerVisible;
                _topOrientMarkerVisible = axis.TopOrientMarkerVisible;
                _orientMarkerType = axis.OrientMarkerType == 0
                    ? Language.GetItem(MainFunction.LangItem, "type1") // "Тип 1"
                    : Language.GetItem(MainFunction.LangItem, "type2"); // "Тип 2";

                #endregion

                AnyPropertyChangedReise();
            }
        }
    }
}
