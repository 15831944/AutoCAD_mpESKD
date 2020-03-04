namespace mpESKD.Functions.mpSection.Overrules.Grips
{
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using Base.Overrules;
    using ModPlusAPI;
    using Section = mpSection.Section;

    /// <summary>
    /// Ручка удаления вершины
    /// </summary>
    public class SectionRemoveVertexGrip : IntellectualEntityGripData
    {
        public SectionRemoveVertexGrip(Section section, int index)
        {
            Section = section;
            GripIndex = index;
            GripType = GripType.Minus;

            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;
        }

        /// <summary>
        /// Экземпляр класса Section
        /// </summary>
        public Section Section { get; }

        /// <summary>
        /// Индекс точки
        /// </summary>
        public int GripIndex { get; }

        // Подсказка в зависимости от имени ручки
        public override string GetTooltip()
        {
            return Language.GetItem(Invariables.LangItem, "gp3"); // "Удалить вершину";
        }

        public override ReturnValue OnHotGrip(ObjectId entityId, Context contextFlags)
        {
            using (Section)
            {
                Point3d? newInsertionPoint = null;

                if (GripIndex == 0)
                {
                    Section.InsertionPoint = Section.MiddlePoints[0];
                    newInsertionPoint = Section.MiddlePoints[0];
                    Section.MiddlePoints.RemoveAt(0);
                }
                else if (GripIndex == Section.MiddlePoints.Count + 1)
                {
                    Section.EndPoint = Section.MiddlePoints.Last();
                    Section.MiddlePoints.RemoveAt(Section.MiddlePoints.Count - 1);
                }
                else
                {
                    Section.MiddlePoints.RemoveAt(GripIndex - 1);
                }

                Section.UpdateEntities();
                Section.BlockRecord.UpdateAnonymousBlocks();
                using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    var blkRef = tr.GetObject(Section.BlockId, OpenMode.ForWrite, true, true);
                    if (newInsertionPoint.HasValue)
                    {
                        ((BlockReference)blkRef).Position = newInsertionPoint.Value;
                    }

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