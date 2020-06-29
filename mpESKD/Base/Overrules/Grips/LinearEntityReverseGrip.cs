namespace mpESKD.Base.Overrules.Grips
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Enums;
    using Utils;

    /// <summary>
    /// Ручка реверса линейного интеллектуального объекта
    /// </summary>
    public class LinearEntityReverseGrip : IntellectualEntityGripData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinearEntityReverseGrip"/> class.
        /// </summary>
        /// <param name="intellectualEntity">Instance of <see cref="IntellectualEntity"/> that implement <see cref="ILinearEntity"/></param>
        public LinearEntityReverseGrip(IntellectualEntity intellectualEntity)
        {
            IntellectualEntity = intellectualEntity;
            GripType = GripType.Mirror;
        }

        /// <summary>
        /// Экземпляр интеллектуального объекта
        /// </summary>
        public IntellectualEntity IntellectualEntity { get; }

        /// <inheritdoc />
        public override ReturnValue OnHotGrip(ObjectId entityId, Context contextFlags)
        {
            using (IntellectualEntity)
            {
                var newInsertionPoint = IntellectualEntity.EndPoint;
                IntellectualEntity.EndPoint = IntellectualEntity.InsertionPoint;
                IntellectualEntity.InsertionPoint = newInsertionPoint;
                ((ILinearEntity)IntellectualEntity).MiddlePoints.Reverse();
                IntellectualEntity.BlockTransform = IntellectualEntity.BlockTransform.Inverse();

                IntellectualEntity.UpdateEntities();
                IntellectualEntity.BlockRecord.UpdateAnonymousBlocks();
                using (var tr = AcadUtils.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    var blkRef = tr.GetObject(IntellectualEntity.BlockId, OpenMode.ForWrite, true, true);
                    ((BlockReference)blkRef).Position = newInsertionPoint;
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
