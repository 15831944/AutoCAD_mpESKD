namespace mpESKD.Functions.mpBreakLine.Overrules.Grips
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Helpers;
    using Base.Overrules;
    using ModPlusAPI;
    using ModPlusAPI.Windows;

    /// <summary>Описание ручки линии обрыва</summary>
    public class BreakLineGrip : IntellectualEntityGripData // <-- Там будут определены типы точек и их ViewportDraw в зависимости от типа. Пока ничего этого нет
    {
        public BreakLineGrip()
        {
            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;
        }

        // Экземпляр класса breakLine, связанный с этой ручкой
        public BreakLine BreakLine { get; set; }

        // Имя ручки
        public BreakLineGripName GripName { get; set; }

        // Подсказка в зависимости от имени ручки
        public override string GetTooltip()
        {
            switch (GripName)
            {
                case BreakLineGripName.StartGrip:
                case BreakLineGripName.EndGrip:
                {
                    return Language.GetItem(Invariables.LangItem, "gp1"); // stretch
                }

                case BreakLineGripName.MiddleGrip: return Language.GetItem(Invariables.LangItem, "gp2"); // move
            }

            return base.GetTooltip();
        }

        // Временное значение первой ручки
        private Point3d _startGripTmp;

        // временное значение последней ручки
        private Point3d _endGripTmp;

        // Обработка изменения статуса ручки
        public override void OnGripStatusChanged(ObjectId entityId, Status newStatus)
        {
            try
            {
                // При начале перемещения запоминаем первоначальное положение ручки
                // Запоминаем начальные значения
                if (newStatus == Status.GripStart)
                {
                    if (GripName == BreakLineGripName.StartGrip)
                    {
                        _startGripTmp = GripPoint;
                    }

                    if (GripName == BreakLineGripName.EndGrip)
                    {
                        _endGripTmp = GripPoint;
                    }

                    if (GripName == BreakLineGripName.MiddleGrip)
                    {
                        _startGripTmp = BreakLine.InsertionPoint;
                        _endGripTmp = BreakLine.EndPoint;
                    }
                }

                // При удачном перемещении ручки записываем новые значения в расширенные данные
                // По этим данным я потом получаю экземпляр класса breakline
                if (newStatus == Status.GripEnd)
                {
                    using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var blkRef = tr.GetObject(BreakLine.BlockId, OpenMode.ForWrite, true, true);
                        using (var resBuf = BreakLine.GetDataForXData())
                        {
                            blkRef.XData = resBuf;
                        }

                        tr.Commit();
                    }

                    BreakLine.Dispose();
                }

                // При отмене перемещения возвращаем временные значения
                if (newStatus == Status.GripAbort)
                {
                    if (_startGripTmp != null & GripName == BreakLineGripName.StartGrip)
                    {
                        BreakLine.InsertionPoint = GripPoint;
                    }

                    if (GripName == BreakLineGripName.MiddleGrip & _startGripTmp != null & _endGripTmp != null)
                    {
                        BreakLine.InsertionPoint = _startGripTmp;
                        BreakLine.EndPoint = _endGripTmp;
                    }

                    if (_endGripTmp != null & GripName == BreakLineGripName.EndGrip)
                    {
                        BreakLine.EndPoint = GripPoint;
                    }
                }

                base.OnGripStatusChanged(entityId, newStatus);
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
    }
}