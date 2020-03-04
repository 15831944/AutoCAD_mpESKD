namespace mpESKD.Functions.mpGroundLine.Overrules.Grips
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using Base.Overrules;
    using ModPlusAPI;
    using ModPlusAPI.Windows;

    /// <summary>
    /// Ручка вершин
    /// </summary>
    public class GroundLineVertexGrip : IntellectualEntityGripData
    {
        public GroundLineVertexGrip(GroundLine groundLine, int index)
        {
            GroundLine = groundLine;
            GripIndex = index;
            GripType = GripType.Point;

            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;
        }

        /// <summary>
        /// Экземпляр класса Section
        /// </summary>
        public GroundLine GroundLine { get; }

        /// <summary>
        /// Индекс точки
        /// </summary>
        public int GripIndex { get; }

        // Подсказка в зависимости от имени ручки
        public override string GetTooltip()
        {
            return Language.GetItem(Invariables.LangItem, "gp1"); // stretch
        }

        // Временное значение ручки
        private Point3d _gripTmp;

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
                    using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var blkRef = tr.GetObject(GroundLine.BlockId, OpenMode.ForWrite, true, true);
                        using (var resBuf = GroundLine.GetDataForXData())
                        {
                            blkRef.XData = resBuf;
                        }

                        tr.Commit();
                    }

                    GroundLine.Dispose();
                }

                // При отмене перемещения возвращаем временные значения
                if (newStatus == Status.GripAbort)
                {
                    if (_gripTmp != null)
                    {
                        if (GripIndex == 0)
                        {
                            GroundLine.InsertionPoint = _gripTmp;
                        }
                        else if (GripIndex == GroundLine.MiddlePoints.Count + 1)
                        {
                            GroundLine.EndPoint = _gripTmp;
                        }
                        else
                        {
                            GroundLine.MiddlePoints[GripIndex - 1] = _gripTmp;
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