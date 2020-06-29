namespace mpESKD.Base.Overrules.Grips
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Enums;
    using ModPlusAPI;
    using Overrules;
    using Utils;

    /// <summary>
    /// Ручка добавления вершины линейного интеллектуального объекта
    /// </summary>
    public class LinearEntityAddVertexGrip : IntellectualEntityGripData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinearEntityAddVertexGrip"/> class.
        /// </summary>
        /// <param name="intellectualEntity">Instance of <see cref="Base.IntellectualEntity"/> that implement <see cref="ILinearEntity"/></param>
        /// <param name="leftPoint">Точка слева</param>
        /// <param name="rightPoint">Точка справа</param>
        public LinearEntityAddVertexGrip(IntellectualEntity intellectualEntity, Point3d? leftPoint, Point3d? rightPoint)
        {
            IntellectualEntity = intellectualEntity;
            GripLeftPoint = leftPoint;
            GripRightPoint = rightPoint;
            GripType = GripType.Plus;
            RubberBandLineDisabled = true;
        }

        /// <summary>
        /// Экземпляр интеллектуального объекта
        /// </summary>
        public IntellectualEntity IntellectualEntity { get; }

        /// <summary>
        /// Левая точка
        /// </summary>
        public Point3d? GripLeftPoint { get; }

        /// <summary>
        /// Правая точка
        /// </summary>
        public Point3d? GripRightPoint { get; }

        /// <summary>
        /// Новое значение точки вершины
        /// </summary>
        public Point3d NewPoint { get; set; }

        /// <inheritdoc />
        public override string GetTooltip()
        {
            return Language.GetItem(Invariables.LangItem, "gp4"); // "Добавить вершину";
        }

        /// <inheritdoc />
        public override void OnGripStatusChanged(ObjectId entityId, Status newStatus)
        {
            if (newStatus == Status.GripStart)
            {
                AcadUtils.Editor.TurnForcedPickOn();
                AcadUtils.Editor.PointMonitor += AddNewVertex_EdOnPointMonitor;
            }

            if (newStatus == Status.GripEnd)
            {
                AcadUtils.Editor.TurnForcedPickOff();
                AcadUtils.Editor.PointMonitor -= AddNewVertex_EdOnPointMonitor;
                using (IntellectualEntity)
                {
                    Point3d? newInsertionPoint = null;

                    var linearEntity = (ILinearEntity)IntellectualEntity;

                    if (GripLeftPoint == IntellectualEntity.InsertionPoint)
                    {
                        linearEntity.MiddlePoints.Insert(0, NewPoint);
                    }
                    else if (GripLeftPoint == null)
                    {
                        linearEntity.MiddlePoints.Insert(0, IntellectualEntity.InsertionPoint);
                        IntellectualEntity.InsertionPoint = NewPoint;
                        newInsertionPoint = NewPoint;
                    }
                    else if (GripRightPoint == null)
                    {
                        linearEntity.MiddlePoints.Add(IntellectualEntity.EndPoint);
                        IntellectualEntity.EndPoint = NewPoint;
                    }
                    else
                    {
                        linearEntity.MiddlePoints.Insert(
                            linearEntity.MiddlePoints.IndexOf(GripLeftPoint.Value) + 1, NewPoint);
                    }

                    IntellectualEntity.UpdateEntities();
                    IntellectualEntity.BlockRecord.UpdateAnonymousBlocks();
                    using (var tr = AcadUtils.Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var blkRef = tr.GetObject(IntellectualEntity.BlockId, OpenMode.ForWrite, true, true);
                        if (newInsertionPoint.HasValue)
                        {
                            ((BlockReference)blkRef).Position = newInsertionPoint.Value;
                        }

                        using (var resBuf = IntellectualEntity.GetDataForXData())
                        {
                            blkRef.XData = resBuf;
                        }

                        tr.Commit();
                    }
                }
            }

            if (newStatus == Status.GripAbort)
            {
                AcadUtils.Editor.TurnForcedPickOff();
                AcadUtils.Editor.PointMonitor -= AddNewVertex_EdOnPointMonitor;
            }

            base.OnGripStatusChanged(entityId, newStatus);
        }

        private void AddNewVertex_EdOnPointMonitor(object sender, PointMonitorEventArgs pointMonitorEventArgs)
        {
            try
            {
                if (GripLeftPoint.HasValue)
                {
                    var leftLine = new Line(GripLeftPoint.Value, pointMonitorEventArgs.Context.ComputedPoint)
                    {
                        ColorIndex = 150
                    };
                    pointMonitorEventArgs.Context.DrawContext.Geometry.Draw(leftLine);
                }

                if (GripRightPoint.HasValue)
                {
                    var rightLine = new Line(pointMonitorEventArgs.Context.ComputedPoint, GripRightPoint.Value)
                    {
                        ColorIndex = 150
                    };
                    pointMonitorEventArgs.Context.DrawContext.Geometry.Draw(rightLine);
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}