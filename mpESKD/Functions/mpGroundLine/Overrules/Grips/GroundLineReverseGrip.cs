namespace mpESKD.Functions.mpGroundLine.Overrules.Grips
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base.Enums;
    using Base.Helpers;
    using Base.Overrules;

    /// <summary>
    /// Ручка реверса линии грунта
    /// </summary>
    public class GroundLineReverseGrip : IntellectualEntityGripData
    {
        public GroundLineReverseGrip(GroundLine groundLine)
        {
            GroundLine = groundLine;
            GripType = GripType.Mirror;

            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;
        }

        /// <summary>
        /// Экземпляр класса Section
        /// </summary>
        public GroundLine GroundLine { get; }

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
                using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
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