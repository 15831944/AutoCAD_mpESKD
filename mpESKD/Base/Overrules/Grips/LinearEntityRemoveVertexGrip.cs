namespace mpESKD.Base.Overrules.Grips
{
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Enums;
    using ModPlusAPI;
    using Overrules;
    using Utils;

    /// <summary>
    /// Ручка удаления вершины линейного интеллектуального объекта
    /// </summary>
    public class LinearEntityRemoveVertexGrip : IntellectualEntityGripData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinearEntityRemoveVertexGrip"/> class.
        /// </summary>
        /// <param name="intellectualEntity">Instance of <see cref="Base.IntellectualEntity"/> that implement <see cref="ILinearEntity"/></param>
        /// <param name="index">Grip index</param>
        public LinearEntityRemoveVertexGrip(IntellectualEntity intellectualEntity, int index)
        {
            IntellectualEntity = intellectualEntity;
            GripIndex = index;
            GripType = GripType.Minus;
        }

        /// <summary>
        /// Экземпляр интеллектуального объекта
        /// </summary>
        public IntellectualEntity IntellectualEntity { get; }

        /// <summary>
        /// Индекс точки
        /// </summary>
        public int GripIndex { get; }

        /// <inheritdoc />
        public override string GetTooltip()
        {
            return Language.GetItem(Invariables.LangItem, "gp3"); // "Удалить вершину";
        }

        /// <inheritdoc />
        public override ReturnValue OnHotGrip(ObjectId entityId, Context contextFlags)
        {
            using (IntellectualEntity)
            {
                Point3d? newInsertionPoint = null;

                var linearEntity = (ILinearEntity)IntellectualEntity;

                if (GripIndex == 0)
                {
                    IntellectualEntity.InsertionPoint = linearEntity.MiddlePoints[0];
                    newInsertionPoint = linearEntity.MiddlePoints[0];
                    linearEntity.MiddlePoints.RemoveAt(0);
                }
                else if (GripIndex == linearEntity.MiddlePoints.Count + 1)
                {
                    IntellectualEntity.EndPoint = linearEntity.MiddlePoints.Last();
                    linearEntity.MiddlePoints.RemoveAt(linearEntity.MiddlePoints.Count - 1);
                }
                else
                {
                    linearEntity.MiddlePoints.RemoveAt(GripIndex - 1);
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

            return ReturnValue.GetNewGripPoints;
        }
    }
}