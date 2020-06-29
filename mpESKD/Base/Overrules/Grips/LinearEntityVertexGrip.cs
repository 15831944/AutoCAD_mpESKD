namespace mpESKD.Base.Overrules.Grips
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Enums;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using Overrules;
    using Utils;

    /// <summary>
    /// Ручка вершин линейного интеллектуального объекта
    /// </summary>
    public class LinearEntityVertexGrip : IntellectualEntityGripData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinearEntityVertexGrip"/> class.
        /// </summary>
        /// <param name="intellectualEntity">Instance of <see cref="Base.IntellectualEntity"/> that implement <see cref="ILinearEntity"/></param>
        /// <param name="index">Grip index</param>
        public LinearEntityVertexGrip(IntellectualEntity intellectualEntity, int index)
        {
            IntellectualEntity = intellectualEntity;
            GripIndex = index;
            GripType = GripType.Point;
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
            return Language.GetItem(Invariables.LangItem, "gp1"); // stretch
        }

        // Временное значение ручки
        private Point3d _gripTmp;

        /// <inheritdoc />
        public override void OnGripStatusChanged(ObjectId entityId, Status newStatus)
        {
            try
            {
                // При начале перемещения запоминаем первоначальное положение ручки
                // Запоминаем начальные значения
                if (newStatus == Status.GripStart)
                {
                    _gripTmp = GripPoint;
                }

                // При удачном перемещении ручки записываем новые значения в расширенные данные
                // По этим данным я потом получаю экземпляр класса groundLine
                if (newStatus == Status.GripEnd)
                {
                    using (var tr = AcadUtils.Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var blkRef = tr.GetObject(IntellectualEntity.BlockId, OpenMode.ForWrite, true, true);
                        using (var resBuf = IntellectualEntity.GetDataForXData())
                        {
                            blkRef.XData = resBuf;
                        }

                        tr.Commit();
                    }

                    IntellectualEntity.Dispose();
                }

                // При отмене перемещения возвращаем временные значения
                if (newStatus == Status.GripAbort)
                {
                    if (_gripTmp != null)
                    {
                        if (GripIndex == 0)
                        {
                            IntellectualEntity.InsertionPoint = _gripTmp;
                        }
                        else if (GripIndex == ((ILinearEntity)IntellectualEntity).MiddlePoints.Count + 1)
                        {
                            IntellectualEntity.EndPoint = _gripTmp;
                        }
                        else
                        {
                            ((ILinearEntity)IntellectualEntity).MiddlePoints[GripIndex - 1] = _gripTmp;
                        }
                    }
                }

                base.OnGripStatusChanged(entityId, newStatus);
            }
            catch (Exception exception)
            {
                if (exception.ErrorStatus != ErrorStatus.NotAllowedForThisProxy)
                    ExceptionBox.Show(exception);
            }
        }
    }
}