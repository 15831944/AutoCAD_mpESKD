namespace mpESKD.Functions.mpGroundLine.Grips
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base.Enums;
    using Base.Overrules;
    using Base.Utils;

    /// <summary>
    /// Ручка реверса линии грунта
    /// </summary>
    public class GroundLineReverseGrip : IntellectualEntityGripData
    {
        public GroundLineReverseGrip(GroundLine groundLine)
        {
            GroundLine = groundLine;
            GripType = GripType.Mirror;
        }

        /// <summary>
        /// Экземпляр класса Section
        /// </summary>
        public GroundLine GroundLine { get; }

        /// <inheritdoc />
        public override ReturnValue OnHotGrip(ObjectId entityId, Context contextFlags)
        {
            using (GroundLine)
            {
                Point3d newInsertionPoint = GroundLine.EndPoint;
                GroundLine.EndPoint = GroundLine.InsertionPoint;
                GroundLine.InsertionPoint = newInsertionPoint;
                GroundLine.MiddlePoints.Reverse();
                GroundLine.BlockTransform = GroundLine.BlockTransform.Inverse();

                GroundLine.UpdateEntities();
                GroundLine.BlockRecord.UpdateAnonymousBlocks();
                using (var tr = AcadUtils.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    var blkRef = tr.GetObject(GroundLine.BlockId, OpenMode.ForWrite, true, true);
                    ((BlockReference)blkRef).Position = newInsertionPoint;
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