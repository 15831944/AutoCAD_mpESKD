namespace mpESKD.Functions.mpBreakLine
{
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Base.Helpers;
    using ModPlusAPI;

    public class BreakLineJig : EntityJig
    {
        public BreakLineJigState JigState { get; set; } = BreakLineJigState.PromptInsertPoint;
        
        private readonly BreakLine _breakLine;
        
        private readonly JigHelper.PointSampler _insertionPoint = new JigHelper.PointSampler(Point3d.Origin);
        
        private readonly JigHelper.PointSampler _endPoint = new JigHelper.PointSampler(new Point3d(15, 0, 0));

        public BreakLineJig(BreakLine breakLine, BlockReference reference) : base(reference)
        {
            _breakLine = breakLine;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            try
            {
                switch (JigState)
                {
                    case BreakLineJigState.PromptInsertPoint:
                        return _insertionPoint.Acquire(prompts, "\n" + Language.GetItem(MainFunction.LangItem, "msg1"), value =>
                        {
                            _breakLine.InsertionPoint = value;
                        });
                    case BreakLineJigState.PromptEndPoint:
                        return _endPoint.Acquire(prompts, "\n" + Language.GetItem(MainFunction.LangItem, "msg2"), _insertionPoint.Value, value =>
                        {
                            _breakLine.EndPoint = value;
                        });
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
                        //obj.Erase(false);
                        obj.Position = _breakLine.InsertionPoint;
                        obj.BlockUnit = AcadHelpers.Database.Insunits;
                        tr.Commit();
                    }
                    _breakLine.UpdateEntities();
                    _breakLine.BlockRecord.UpdateAnonymousBlocks();
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

    // Варианты состояния
    public enum BreakLineJigState
    {
        PromptInsertPoint = 1, // Запрос точки вставки
        PromptEndPoint = 2, // Запрос второй (конечной) точки
        Done = 3 // Завершено
    }
}
