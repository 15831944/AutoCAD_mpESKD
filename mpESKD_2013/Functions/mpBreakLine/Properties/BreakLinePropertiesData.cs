// ReSharper disable InconsistentNaming
#pragma warning disable CS0618

namespace mpESKD.Functions.mpBreakLine.Properties
{
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Base;
    using Base.Enums;
    using Base.Styles;
    using Styles;

    public class BreakLinePropertiesData : BasePropertiesData
    {
        public BreakLinePropertiesData(ObjectId blockReferenceObjectId)
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
                //BreakLine.GetBreakLineFromEntity,
                EntityReaderFactory.Instance.GetFromEntity<BreakLine>,
                StyleManager.GetStyles<BreakLineStyle>().FirstOrDefault(s => s.Name.Equals(value)));
        }

        private string _scale;

        /// <inheritdoc />
        public override string Scale
        {
            get => _scale;
            set => ChangeScaleProperty(
                //BreakLine.GetBreakLineFromEntity,
                EntityReaderFactory.Instance.GetFromEntity<BreakLine>,
                value);
        }

        private double _lineTypeScale;

        /// <inheritdoc />
        public override double LineTypeScale
        {
            get => _lineTypeScale;
            set => ChangeProperty(
                //BreakLine.GetBreakLineFromEntity, 
                EntityReaderFactory.Instance.GetFromEntity<BreakLine>,
                breakLine => breakLine.LineTypeScale = value);
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

        private int _overhang;
        public int Overhang
        {
            get => _overhang;
            set => ChangeProperty(
                //BreakLine.GetBreakLineFromEntity, 
                EntityReaderFactory.Instance.GetFromEntity<BreakLine>,
                breakLine => breakLine.Overhang = value);
        }

        private int _breakHeight;
        public int BreakHeight
        {
            get => _breakHeight;
            set => ChangeProperty(
                //BreakLine.GetBreakLineFromEntity, 
                EntityReaderFactory.Instance.GetFromEntity<BreakLine>,
                breakLine => breakLine.BreakHeight = value);
        }

        private int _breakWidth;
        public int BreakWidth
        {
            get => _breakWidth;
            set => ChangeProperty(
                //BreakLine.GetBreakLineFromEntity, 
                EntityReaderFactory.Instance.GetFromEntity<BreakLine>,
                breakLine => breakLine.BreakWidth = value);
        }

        private string _breakLineType;
        public string BreakLineType
        {
            get => _breakLineType;
            set => ChangeProperty(
                //BreakLine.GetBreakLineFromEntity,
                EntityReaderFactory.Instance.GetFromEntity<BreakLine>,
                breakLine => breakLine.BreakLineType = BreakLineTypeHelper.GetByLocalName(value));
        }
        
        public override void Update(BlockReference blkReference)
        {
            if (blkReference == null)
            {
                BlkRefObjectId = ObjectId.Null;
                return;
            }
            var breakLine = EntityReaderFactory.Instance.GetFromEntity<BreakLine>(blkReference);
            if (breakLine != null)
            {
                _style = StyleManager.GetStyles<BreakLineStyle>().FirstOrDefault(s => s.Guid.Equals(breakLine.StyleGuid))?.Name;
                _overhang = breakLine.Overhang;
                _breakHeight = breakLine.BreakHeight;
                _breakWidth = breakLine.BreakWidth;
                _breakLineType = BreakLineTypeHelper.GetLocalName(breakLine.BreakLineType);
                _scale = breakLine.Scale.Name;
                _layerName = blkReference.Layer;
                _lineType = blkReference.Linetype;
                _lineTypeScale = breakLine.LineTypeScale;
                AnyPropertyChangedReise();
            }
        }
    }
}
