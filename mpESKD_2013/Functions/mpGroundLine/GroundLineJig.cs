namespace mpESKD.Functions.mpGroundLine
{
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Base.Helpers;
    using ModPlusAPI;

    public class GroundLineJig : EntityJig
    {
        public GroundLineJigState JigState { get; set; } = GroundLineJigState.PromptInsertPoint;

        private readonly GroundLine _groundLine;

        private readonly JigHelper.PointSampler _insertionPoint = new JigHelper.PointSampler(Point3d.Origin);

        private readonly JigHelper.PointSampler _nextPoint = new JigHelper.PointSampler(new Point3d(20, 0, 0));

        public Point3d? PreviousPoint { get; set; }

        public GroundLineJig(GroundLine groundLine, BlockReference reference) : base(reference)
        {
            _groundLine = groundLine;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            try
            {
                switch (JigState)
                {
                    case GroundLineJigState.PromptInsertPoint:
                        return _insertionPoint.Acquire(prompts, "\n" + Language.GetItem(MainFunction.LangItem, "msg1"), value =>
                        {
                            _groundLine.InsertionPoint = value;
                        });
                    case GroundLineJigState.PromptNextPoint:
                        {
                            var basePoint = _insertionPoint.Value;
                            if (PreviousPoint != null)
                                basePoint = PreviousPoint.Value;
                            return _nextPoint.Acquire(prompts, "\n" + Language.GetItem(MainFunction.LangItem, "msg5"), basePoint, value =>
                            {
                                _groundLine.EndPoint = value;
                            });
                        }
                    default:
                        return SamplerStatus.NoChange;
                }
            }
            catch
            {
                return SamplerStatus.NoChange;
            }
        }

        protected override bool Update()
        {
            try
            {
                using (AcadHelpers.Document.LockDocument(DocumentLockMode.ProtectedAutoWrite, null, null, true))
                {
                    using (var tr = AcadHelpers.Document.TransactionManager.StartTransaction())
                    {
                        var obj = (BlockReference)tr.GetObject(Entity.Id, OpenMode.ForWrite, true);
                        obj.Position = _groundLine.InsertionPoint;
                        obj.BlockUnit = AcadHelpers.Database.Insunits;
                        tr.Commit();
                    }
                    _groundLine.UpdateEntities();
                    _groundLine.BlockRecord.UpdateAnonymousBlocks();
                }
                return true;
            }
            catch
            {
                // ignored
            }
            return false;
        }
    }

    public enum GroundLineJigState
    {
        PromptInsertPoint = 1,
        PromptNextPoint = 2,
        Done = 3
    }
}
