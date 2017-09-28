using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using mpESKD.Base.Helpers;

namespace mpESKD.Functions.mpBreakLine
{
    public class BreakLineJig : EntityJig
    {
        public BreakLineJigState JigState { get; set; } = BreakLineJigState.PromptInsertPoint;
        private readonly BreakLine _breakLine;
        private readonly PointSampler _insertionPoint = new PointSampler(Point3d.Origin);
        private readonly PointSampler _endPoint = new PointSampler(new Point3d(15, 0, 0));

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
                        return _insertionPoint.Acquire(prompts, "\nВведите точку вставки:", value =>
                        {
                            _breakLine.InsertionPoint = value;

                        });
                    case BreakLineJigState.PromptEndPoint:
                        return _endPoint.Acquire(prompts, "\nВведите конечную точку:", _insertionPoint.Value, value =>
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

    public class PointSampler
    {
        //private static readonly Tolerance Tolerance;
        public Point3d Value { get; set; }

        static PointSampler()
        {
            //Tolerance = new Tolerance(1E-1, 1E-1);
        }

        public PointSampler(Point3d value)
        {
            Value = value;
        }
        public SamplerStatus Acquire(JigPrompts prompts, string message, Action<Point3d> updater)
        {
            return Acquire(prompts, GetDefaultOptions(message), updater);
        }
        public SamplerStatus Acquire(JigPrompts prompts, string message, Point3d basePoint, Action<Point3d> updater)
        {
            return Acquire(prompts, GetDefaultOptions(message, basePoint), updater);
        }

        public SamplerStatus Acquire(JigPrompts prompts, JigPromptPointOptions options, Action<Point3d> updater)
        {
            var promptPointResult = prompts.AcquirePoint(options);
            if (promptPointResult.Status != PromptStatus.OK)
            {
                if (promptPointResult.Status == PromptStatus.Other)
                {
                    return 0;
                }
                return SamplerStatus.Cancel;
            }
            if (Value.IsEqualTo(promptPointResult.Value/*, Tolerance*/))
            {
                return SamplerStatus.NoChange;
            }
            var value = promptPointResult.Value;
            var point3D = value;
            Value = value;
            updater(point3D);
            return 0;
        }

        public static JigPromptPointOptions GetDefaultOptions(string message)
        {
            var jigPromptPointOption = new JigPromptPointOptions(message);
            jigPromptPointOption.UserInputControls = (UserInputControls)2272;
            return jigPromptPointOption;
        }
        public static JigPromptPointOptions GetDefaultOptions(string message, Point3d basePoint)
        {
            var jigPromptPointOption = new JigPromptPointOptions(message);
            jigPromptPointOption.BasePoint = basePoint;
            jigPromptPointOption.UseBasePoint = true;
            jigPromptPointOption.UserInputControls = 
                UserInputControls.GovernedByUCSDetect | 
                UserInputControls.GovernedByOrthoMode |
                UserInputControls.NoDwgLimitsChecking |
                UserInputControls.NoNegativeResponseAccepted | 
                UserInputControls.Accept3dCoordinates |
                UserInputControls.AcceptOtherInputString |
                UserInputControls.UseBasePointElevation;
            return jigPromptPointOption;
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
