namespace mpESKD.Functions.mpGroundLine.Overrules.Grips
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using Base.Overrules;
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

            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;
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
                AcadHelpers.Editor.TurnForcedPickOn();
                AcadHelpers.Editor.PointMonitor += AddNewVertex_EdOnPointMonitor;
            }

            if (newStatus == Status.GripEnd)
            {
                AcadHelpers.Editor.TurnForcedPickOff();
                AcadHelpers.Editor.PointMonitor -= AddNewVertex_EdOnPointMonitor;
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
                    using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
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
                AcadHelpers.Editor.TurnForcedPickOff();
                AcadHelpers.Editor.PointMonitor -= AddNewVertex_EdOnPointMonitor;
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