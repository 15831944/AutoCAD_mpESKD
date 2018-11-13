// ReSharper disable InconsistentNaming
#pragma warning disable CS0618
namespace mpESKD.Functions.mpGroundLine.Properties
{
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Base;
    using Base.Styles;
    using Styles;

    public class GroundLinePropertiesData : BasePropertiesData
    {
        public GroundLinePropertiesData(ObjectId blockReferenceObjectId)
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
                //GroundLine.GetGroundLineFromEntity,
                EntityReaderFactory.Instance.GetFromEntity<GroundLine>,
                StyleManager.GetStyles<GroundLineStyle>().FirstOrDefault(s => s.Name.Equals(value)));
        }

        private string _scale;

        public override string Scale
        {
            get => _scale;
            set => ChangeScaleProperty(
                //todo old
                //GroundLine.GetGroundLineFromEntity,
                EntityReaderFactory.Instance.GetFromEntity<GroundLine>,
                value);
        }

        private double _lineTypeScale;

        /// <inheritdoc />
        public override double LineTypeScale
        {
            get => _lineTypeScale;
            set => ChangeProperty(
                //todo old
                //GroundLine.GetGroundLineFromEntity,
                EntityReaderFactory.Instance.GetFromEntity<GroundLine>,
                groundLine => groundLine.LineTypeScale = value);
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

        private string _firstStrokeOffset;
        public string FirstStrokeOffset
        {
            get => _firstStrokeOffset;
            set => ChangeProperty(
                //todo old
                //GroundLine.GetGroundLineFromEntity,
                EntityReaderFactory.Instance.GetFromEntity<GroundLine>,
                groundLine => groundLine.FirstStrokeOffset = GroundLinePropertiesHelpers.GetFirstStrokeOffsetByLocalName(value));
        }

        private int _strokeLength;
        public int StrokeLength
        {
            get => _strokeLength;
            set => ChangeProperty(
                //todo old
                //GroundLine.GetGroundLineFromEntity, 
                EntityReaderFactory.Instance.GetFromEntity<GroundLine>,
                groundLine => groundLine.StrokeLength = value);
        }

        private int _strokeOffset;
        public int StrokeOffset
        {
            get => _strokeOffset;
            set => ChangeProperty(
                //todo old
                //GroundLine.GetGroundLineFromEntity,
                EntityReaderFactory.Instance.GetFromEntity<GroundLine>,
                groundLine => groundLine.StrokeOffset = value);
        }

        private int _strokeAngle;
        public int StrokeAngle
        {
            get => _strokeAngle;
            set => ChangeProperty(
                // todo old
                //GroundLine.GetGroundLineFromEntity,
                EntityReaderFactory.Instance.GetFromEntity<GroundLine>,
                groundLine => groundLine.StrokeAngle = value);
        }

        private int _space;
        public int Space
        {
            get => _space;
            set => ChangeProperty(
                // todo old
                //GroundLine.GetGroundLineFromEntity, 
                EntityReaderFactory.Instance.GetFromEntity<GroundLine>,
                groundLine => groundLine.Space = value);
        }
  
        public override void Update(BlockReference blockReference)
        {
            if (blockReference == null)
            {
                BlkRefObjectId = ObjectId.Null;
                return;
            }
            //var groundLine = GroundLine.GetGroundLineFromEntity(blockReference);
            var groundLine = EntityReaderFactory.Instance.GetFromEntity<GroundLine>(blockReference);
            if (groundLine != null)
            {
                _style = StyleManager.GetStyles<GroundLineStyle>().FirstOrDefault(s => s.Guid.Equals(groundLine.StyleGuid))?.Name;
                _firstStrokeOffset = GroundLinePropertiesHelpers.GetLocalFirstStrokeOffsetName(groundLine.FirstStrokeOffset);
                _strokeLength = groundLine.StrokeLength;
                _strokeOffset = groundLine.StrokeOffset;
                _strokeAngle = groundLine.StrokeAngle;
                _space = groundLine.Space;
                _scale = groundLine.Scale.Name;
                _layerName = blockReference.Layer;
                _lineTypeScale = groundLine.LineTypeScale;
                _lineType = blockReference.Linetype;
                AnyPropertyChangedReise();
            }
        }
    }
}
