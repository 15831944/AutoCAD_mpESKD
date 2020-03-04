namespace mpESKD.Functions.mpSection.Overrules.Grips
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base.Enums;
    using Base.Helpers;
    using Base.Overrules;
    using Section = mpSection.Section;

    /// <summary>
    /// Ручка реверса разреза
    /// </summary>
    public class SectionReverseGrip : IntellectualEntityGripData
    {
        public SectionReverseGrip(Section section)
        {
            Section = section;
            GripType = GripType.Mirror;

            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;
        }

        /// <summary>
        /// Экземпляр класса Section
        /// </summary>
        public Section Section { get; }

        public override ReturnValue OnHotGrip(ObjectId entityId, Context contextFlags)
        {
            using (Section)
            {
                Point3d newInsertionPoint = Section.EndPoint;
                Section.EndPoint = Section.InsertionPoint;
                Section.InsertionPoint = newInsertionPoint;
                Section.MiddlePoints.Reverse();

                // swap direction
                Section.EntityDirection = Section.EntityDirection == EntityDirection.LeftToRight
                    ? EntityDirection.RightToLeft
                    : EntityDirection.LeftToRight;
                Section.BlockTransform = Section.BlockTransform.Inverse();

                // swap text offsets
                var tmp = Section.AcrossBottomShelfTextOffset;
                Section.AcrossBottomShelfTextOffset = Section.AcrossTopShelfTextOffset;
                Section.AcrossTopShelfTextOffset = tmp;
                tmp = Section.AlongBottomShelfTextOffset;
                Section.AlongBottomShelfTextOffset = Section.AlongTopShelfTextOffset;
                Section.AlongTopShelfTextOffset = tmp;

                Section.UpdateEntities();
                Section.BlockRecord.UpdateAnonymousBlocks();
                using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    var blkRef = tr.GetObject(Section.BlockId, OpenMode.ForWrite, true, true);
                    ((BlockReference)blkRef).Position = newInsertionPoint;
                    using (var resBuf = Section.GetDataForXData())
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