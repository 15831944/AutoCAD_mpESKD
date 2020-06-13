namespace mpESKD.Functions.mpGroundLine.Overrules.Grips
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Base.Enums;
    using Base.Overrules;
    using Base.Utils;
    using ModPlusAPI;

    /// <summary>
    /// Ручка добавления вершины
    /// </summary>
    public class GroundLineAddVertexGrip : IntellectualEntityGripData
    {
        public GroundLineAddVertexGrip(GroundLine groundLine, Point3d? leftPoint, Point3d? rightPoint)
        {
            GroundLine = groundLine;
            GripLeftPoint = leftPoint;
            GripRightPoint = rightPoint;
            GripType = GripType.Plus;
            RubberBandLineDisabled = true;
        }

        /// <summary>
        /// Экземпляр класса Section
        /// </summary>
        public GroundLine GroundLine { get; }

        /// <summary>
        /// Левая точка
        /// </summary>
        public Point3d? GripLeftPoint { get; }

        /// <summary>
        /// Правая точка
        /// </summary>
        public Point3d? GripRightPoint { get; }

        public Point3d NewPoint { get; set; }

        public override string GetTooltip()
        {
            return Language.GetItem(Invariables.LangItem, "gp4"); // "Добавить вершину";
        }

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
                using (GroundLine)
                {
                    Point3d? newInsertionPoint = null;

                    if (GripLeftPoint == GroundLine.InsertionPoint)
                    {
                        GroundLine.MiddlePoints.Insert(0, NewPoint);
                    }
                    else if (GripLeftPoint == null)
                    {
                        GroundLine.MiddlePoints.Insert(0, GroundLine.InsertionPoint);
                        GroundLine.InsertionPoint = NewPoint;
                        newInsertionPoint = NewPoint;
                    }
                    else if (GripRightPoint == null)
                    {
                        GroundLine.MiddlePoints.Add(GroundLine.EndPoint);
                        GroundLine.EndPoint = NewPoint;
                    }
                    else
                    {
                        GroundLine.MiddlePoints.Insert(GroundLine.MiddlePoints.IndexOf(GripLeftPoint.Value) + 1, NewPoint);
                    }

                    GroundLine.UpdateEntities();
                    GroundLine.BlockRecord.UpdateAnonymousBlocks();
                    using (var tr = AcadUtils.Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var blkRef = tr.GetObject(GroundLine.BlockId, OpenMode.ForWrite, true, true);
                        if (newInsertionPoint.HasValue)
                        {
                            ((BlockReference)blkRef).Position = newInsertionPoint.Value;
                        }

                        using (var resBuf = GroundLine.GetDataForXData())
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
                    Line leftLine = new Line(GripLeftPoint.Value, pointMonitorEventArgs.Context.ComputedPoint)
                    {
                        ColorIndex = 150
                    };
                    pointMonitorEventArgs.Context.DrawContext.Geometry.Draw(leftLine);
                }

                if (GripRightPoint.HasValue)
                {
                    Line rightLine = new Line(pointMonitorEventArgs.Context.ComputedPoint, GripRightPoint.Value)
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