// ReSharper disable InconsistentNaming
#pragma warning disable CS0618

namespace mpESKD.Functions.mpAxis.Properties
{
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Base.Helpers;
    using Styles;
    using ModPlusAPI;
    using Base;
    using Base.Styles;

    public class AxisPropertiesData : BasePropertiesData
    {
        public AxisPropertiesData(ObjectId blockReferenceObjectId)
            : base(blockReferenceObjectId)
        {

        }

        #region General properties

        private string _style;

        /// <inheritdoc />
        public override string Style
        {
            get => _style;
            set => ChangeStyleProperty(
                //todo old
                EntityReaderFactory.Instance.GetFromEntity<Axis>,
                StyleManager.GetStyles<AxisStyle>().FirstOrDefault(s => s.Name.Equals(value)));
        }

        private string _scale;
        
        /// <inheritdoc />
        public override string Scale
        {
            get => _scale;
            set => ChangeProperty(
                EntityReaderFactory.Instance.GetFromEntity<Axis>,
                axis =>
                {
                    var oldScale = axis.GetScale();
                    axis.Scale = AcadHelpers.GetAnnotationScaleByName(value);
                    if (MainStaticSettings.Settings.AxisLineTypeScaleProportionScale)
                    {
                        var newScale = axis.GetScale();
                        axis.LineTypeScale = axis.LineTypeScale * newScale / oldScale;
                    }
                });
        }

        private double _lineTypeScale;

        /// <inheritdoc />
        public override double LineTypeScale
        {
            get => _lineTypeScale;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.LineTypeScale = value);
        }

        private string _lineType;

        /// <inheritdoc />
        public override string LineType
        {
            get => _lineType;
            set => ChangeLineTypeProperty(value);
        }

        private string _layerName;

        /// <inheritdoc />
        public override string LayerName
        {
            get => _layerName;
            set => ChangeLayerNameProperty(value);
        }

        #endregion

        private string _markersPosition;
        /// <summary>Позиция маркеров</summary>
        public string MarkersPosition
        {
            get => _markersPosition;
            set => ChangeProperty(
                EntityReaderFactory.Instance.GetFromEntity<Axis>,
                axis => axis.MarkersPosition = AxisPropertiesHelpers.GetAxisMarkersPositionByLocalName(value));
        }

        private int _markersCount;
        /// <summary>Позиция маркеров</summary>
        public int MarkersCount
        {
            get => _markersCount;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.MarkersCount = value);
        }

        private int _fracture;
        /// <summary>Излом</summary>
        public int Fracture
        {
            get => _fracture;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.Fracture = value);
        }

        private int _bottomFractureOffset;
        /// <summary>Нижний отступ излома</summary>
        public int BottomFractureOffset
        {
            get => _bottomFractureOffset;
            set => ChangeProperty(
                EntityReaderFactory.Instance.GetFromEntity<Axis>,
                axis =>
                {
                    var oldFracture = axis.BottomFractureOffset;
                    axis.BottomFractureOffset = value;
                    // нужно сместить зависимые точки
                    var vecNorm = (axis.EndPoint - axis.InsertionPoint).GetNormal() * (value - oldFracture) * axis.GetScale();
                    axis.BottomOrientPoint = axis.BottomOrientPoint + vecNorm;
                });
        }

        private int _topFractureOffset;
        /// <summary>Верхний отступ излома</summary>
        public int TopFractureOffset
        {
            get => _topFractureOffset;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.TopFractureOffset = value);
        }

        private int _markersDiameter;
        public int MarkersDiameter
        {
            get => _markersDiameter;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.MarkersDiameter = value);
        }

        private string _textStyle;
        public string TextStyle
        {
            get => _textStyle;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.TextStyle = value);
        }

        private double _textHeight;
        public double TextHeight
        {
            get => _textHeight;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.TextHeight = value);
        }

        #region Text

        // first text
        private string _firstTextPrefix;
        public string FirstTextPrefix
        {
            get => _firstTextPrefix;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.FirstTextPrefix = value);
        }

        private string _firstTextSuffix;
        public string FirstTextSuffix
        {
            get => _firstTextSuffix;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.FirstTextSuffix = value);
        }

        private string _firstText;
        public string FirstText
        {
            get => _firstText;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.FirstText = value);
        }

        // second text
        private string _secondTextPrefix;
        public string SecondTextPrefix
        {
            get => _secondTextPrefix;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.SecondTextPrefix = value);
        }

        private string _secondTextSuffix;
        public string SecondTextSuffix
        {
            get => _secondTextSuffix;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.SecondTextSuffix = value);
        }

        private string _secondText;
        public string SecondText
        {
            get => _secondText;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.SecondText = value);
        }

        // third text
        private string _thirdTextPrefix;
        public string ThirdTextPrefix
        {
            get => _thirdTextPrefix;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.ThirdTextPrefix = value);
        }

        private string _thirdTextSuffix;
        public string ThirdTextSuffix
        {
            get => _thirdTextSuffix;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.ThirdTextSuffix = value);
        }

        private string _thirdText;
        public string ThirdText
        {
            get => _thirdText;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.ThirdText = value);
        }

        #endregion

        #region Типы маркеров

        private string _firstMarkerType;
        public string FirstMarkerType
        {
            get => _firstMarkerType;
            set => ChangeProperty(
                EntityReaderFactory.Instance.GetFromEntity<Axis>,
                axis => axis.FirstMarkerType = value.Equals(Language.GetItem(MainFunction.LangItem, "type1")) // "Тип 1"
                    ? 0 : 1);
        }

        private string _secondMarkerType;
        public string SecondMarkerType
        {
            get => _secondMarkerType;
            set => ChangeProperty(
                EntityReaderFactory.Instance.GetFromEntity<Axis>,
                axis => axis.SecondMarkerType = value.Equals(Language.GetItem(MainFunction.LangItem, "type1")) // "Тип 1"
                    ? 0 : 1);
        }

        private string _thirdMarkerType;
        public string ThirdMarkerType
        {
            get => _thirdMarkerType;
            set => ChangeProperty(
                EntityReaderFactory.Instance.GetFromEntity<Axis>,
                axis => axis.ThirdMarkerType = value.Equals(Language.GetItem(MainFunction.LangItem, "type1")) // "Тип 1"
                    ? 0 : 1);
        }
        
        #endregion

        #region Маркеры ориентира

        private int _arrowSize;
        public int ArrowSize
        {
            get => _arrowSize;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.ArrowsSize = value);
        }

        private string _bottomOrientText;
        public string BottomOrientText
        {
            get => _bottomOrientText;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.BottomOrientText = value);
        }

        private string _topOrientText;
        public string TopOrientText
        {
            get => _topOrientText;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.TopOrientText = value);
        }

        private bool _bottomOrientMarkerVisible;
        public bool BottomOrientMarkerVisible
        {
            get => _bottomOrientMarkerVisible;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.BottomOrientMarkerVisible = value);
        }

        private bool _topOrientMarkerVisible;
        public bool TopOrientMarkerVisible
        {
            get => _topOrientMarkerVisible;
            set => ChangeProperty(EntityReaderFactory.Instance.GetFromEntity<Axis>, axis => axis.TopOrientMarkerVisible = value);
        }

        private string _orientMarkerType;
        public string OrientMarkerType
        {
            get => _orientMarkerType;
            set => ChangeProperty(
                EntityReaderFactory.Instance.GetFromEntity<Axis>, 
                axis => axis.OrientMarkerType = value.Equals(Language.GetItem(MainFunction.LangItem, "type1")) // "Тип 1"
                ? 0 : 1);
        }

        #endregion
        
        public override void Update(BlockReference blkReference)
        {
            if (blkReference == null)
            {
                BlkRefObjectId = ObjectId.Null;
                return;
            }
            var axis = EntityReaderFactory.Instance.GetFromEntity<Axis>(blkReference);
            if (axis != null)
            {
                //_style = AxisStyleManager.Styles.FirstOrDefault(s => s.Guid.Equals(axis.StyleGuid))?.Name;
                _style = StyleManager.GetStyles<AxisStyle>().FirstOrDefault(s => s.Guid.Equals(axis.StyleGuid))?.Name;
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
