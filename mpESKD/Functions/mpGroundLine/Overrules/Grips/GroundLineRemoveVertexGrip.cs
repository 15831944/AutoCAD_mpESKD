namespace mpESKD.Functions.mpGroundLine.Overrules.Grips
{
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Base.Enums;
    using Base.Overrules;
    using Base.Utils;
    using ModPlusAPI;

    /// <summary>
    /// Ручка удаления вершины
    /// </summary>
    public class GroundLineRemoveVertexGrip : IntellectualEntityGripData
    {
        public GroundLineRemoveVertexGrip(GroundLine groundLine, int index)
        {
            GroundLine = groundLine;
            GripIndex = index;
            GripType = GripType.Minus;
        }

        /// <summary>
        /// Экземпляр класса Section
        /// </summary>
        public GroundLine GroundLine { get; }

        /// <summary>
        /// Индекс точки
        /// </summary>
        public int GripIndex { get; }

        /// <inheritdoc />
        public override string GetTooltip()
        {
            return Language.GetItem(Invariables.LangItem, "gp3"); // "Удалить вершину";
        }

        public override ReturnValue OnHotGrip(ObjectId entityId, Context contextFlags)
        {
            using (GroundLine)
            {
                Point3d? newInsertionPoint = null;

                if (GripIndex == 0)
                {
                    GroundLine.InsertionPoint = GroundLine.MiddlePoints[0];
                    newInsertionPoint = GroundLine.MiddlePoints[0];
                    GroundLine.MiddlePoints.RemoveAt(0);
                }
                else if (GripIndex == GroundLine.MiddlePoints.Count + 1)
                {
                    GroundLine.EndPoint = GroundLine.MiddlePoints.Last();
                    GroundLine.MiddlePoints.RemoveAt(GroundLine.MiddlePoints.Count - 1);
                }
                else
                {
                    GroundLine.MiddlePoints.RemoveAt(GripIndex - 1);
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

            return ReturnValue.GetNewGripPoints;
        }
    }
}