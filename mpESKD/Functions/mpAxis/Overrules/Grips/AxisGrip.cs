namespace mpESKD.Functions.mpAxis.Overrules.Grips
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Base.Helpers;
    using Base.Overrules;
    using ModPlusAPI;
    using ModPlusAPI.Windows;

    /// <summary>Описание ручки линии обрыва</summary>
    public class AxisGrip : IntellectualEntityGripData
    {
        public AxisGrip()
        {
            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;
        }

        // Экземпляр класса Axis, связанный с этой ручкой
        public Axis Axis { get; set; }

        // Имя ручки
        public AxisGripName GripName { get; set; }

        // Подсказка в зависимости от имени ручки
        public override string GetTooltip()
        {
            switch (GripName)
            {
                case AxisGripName.StartGrip:
                case AxisGripName.EndGrip:
                case AxisGripName.BottomMarkerGrip:
                case AxisGripName.TopMarkerGrip:
                case AxisGripName.BottomOrientGrip:
                case AxisGripName.TopOrientGrip:
                {
                    return Language.GetItem(Invariables.LangItem, "gp1"); // stretch
                }

                case AxisGripName.MiddleGrip: return Language.GetItem(Invariables.LangItem, "gp2"); // move
            }

            return base.GetTooltip();
        }

        // Временное значение первой ручки
        private Point3d _startGripTmp;

        // временное значение последней ручки
        private Point3d _endGripTmp;

        // other points
        private Point3d _bottomMarkerGripTmp;
        private Point3d _topMarkerGripTmp;
        private Point3d _bottomOrientGripTmp;
        private Point3d _topOrientGripTmp;

        // Обработка изменения статуса ручки
        public override void OnGripStatusChanged(ObjectId entityId, Status newStatus)
        {
            try
            {
                // Запоминаем начальные значения
                if (newStatus == Status.GripStart)
                {
                    _startGripTmp = Axis.InsertionPoint;
                    _endGripTmp = Axis.EndPoint;
                    _bottomMarkerGripTmp = Axis.BottomMarkerPoint;
                    _topMarkerGripTmp = Axis.TopMarkerPoint;
                    _bottomOrientGripTmp = Axis.BottomOrientPoint;
                    _topOrientGripTmp = Axis.TopOrientPoint;
                }

                // При удачном перемещении ручки записываем новые значения в расширенные данные
                // По этим данным я потом получаю экземпляр класса axis
                if (newStatus == Status.GripEnd)
                {
                    using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var blkRef = tr.GetObject(Axis.BlockId, OpenMode.ForWrite, true, true);
                        using (var resBuf = Axis.GetDataForXData())
                        {
                            blkRef.XData = resBuf;
                        }

                        tr.Commit();
                    }

                    Axis.Dispose();
                }

                // При отмене перемещения возвращаем временные значения
                if (newStatus == Status.GripAbort)
                {
                    Axis.InsertionPoint = _startGripTmp;
                    Axis.EndPoint = _endGripTmp;
                    Axis.BottomMarkerPoint = _bottomMarkerGripTmp;
                    Axis.TopMarkerPoint = _topMarkerGripTmp;
                    Axis.BottomOrientPoint = _bottomOrientGripTmp;
                    Axis.TopOrientPoint = _topOrientGripTmp;
                }

                base.OnGripStatusChanged(entityId, newStatus);
            }
            catch (System.Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
    }
}