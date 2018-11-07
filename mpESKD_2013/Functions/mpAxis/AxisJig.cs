﻿namespace mpESKD.Functions.mpAxis
{
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Base.Helpers;
    using ModPlusAPI;

    public class AxisJig : EntityJig
    {
        public AxisJigState JigState { get; set; } = AxisJigState.PromptInsertPoint;
        private readonly Axis _axis;
        private readonly JigHelper.PointSampler _insertionPoint = new JigHelper.PointSampler(Point3d.Origin);
        private readonly JigHelper.PointSampler _endPoint = new JigHelper.PointSampler(new Point3d(0.0, -1.0, 0.0));

        internal AxisJig(Axis axis, BlockReference reference) : base(reference)
        {
            _axis = axis;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            try
            {
                switch (JigState)
                {
                    case AxisJigState.PromptInsertPoint:
                        return _insertionPoint.Acquire(prompts, "\n" + Language.GetItem(MainFunction.LangItem, "msg1"), value =>
                        {
                            _axis.InsertionPoint = value;
                        });
                    case AxisJigState.PromptEndPoint:
                        return _endPoint.Acquire(prompts, "\n" + Language.GetItem(MainFunction.LangItem, "msg2"), _insertionPoint.Value, value =>
                        {
                            _axis.EndPoint = value;
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
                        obj.Position = _axis.InsertionPoint;
                        obj.BlockUnit = AcadHelpers.Database.Insunits;
                        tr.Commit();
                    }
                    _axis.UpdateEntities();
                    _axis.BlockRecord.UpdateAnonymousBlocks();
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
    public enum AxisJigState
    {
        PromptInsertPoint = 1, // Запрос точки вставки
        PromptEndPoint = 2, // Запрос второй (конечной) точки
        Done = 3 // Завершено
    }
}
