using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ModPlus.Helpers;
using mpESKD.Base.Helpers;
using mpESKD.Base.Styles;
using Exception = System.Exception;
using mpESKD.Base.Overrules;
using ModPlusAPI.Windows;

namespace mpESKD.Functions.mpBreakLine.Overrules
{
    /// <summary>
    /// Класс, создающий и обрабатывающий переопределение ручек
    /// </summary>
    public class BreakLineGripPointsOverrule : GripOverrule
    {
        protected static BreakLineGripPointsOverrule _breakLineGripPointOverrule;
        public static BreakLineGripPointsOverrule Instance()
        {
            return _breakLineGripPointOverrule ?? (_breakLineGripPointOverrule = new BreakLineGripPointsOverrule());
        }

        /// <summary>
        /// Получение ручек для примитива
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="grips"></param>
        /// <param name="curViewUnitSize"></param>
        /// <param name="gripSize"></param>
        /// <param name="curViewDir"></param>
        /// <param name="bitFlags"></param>
        public override void GetGripPoints(Entity entity, GripDataCollection grips, double curViewUnitSize, int gripSize, Vector3d curViewDir,
            GetGripPointsFlags bitFlags)
        {
            try
            {
                // Чтобы "отключить" точку вставки блока, нужно получить сначал блок
                // Т.к. мы точно знаем для какого примитива переопределение, то получаем блок:
                BlockReference blkRef = (BlockReference)entity;
                // Получение базовых ручек
                base.GetGripPoints(entity, grips, curViewUnitSize, gripSize, curViewDir, bitFlags);
                // Если это примитив плагина (проверка по наличию XData)
                if (IsApplicable(entity))
                {
                    // Удаляем стандартную ручку позиции блока (точки вставки)
                    GripData toRemove = null;
                    foreach (GripData gd in grips)
                    {
                        if (gd.GripPoint == blkRef.Position)
                        {
                            toRemove = gd;
                            break;
                        }
                    }
                    if (toRemove != null) grips.Remove(toRemove);
                    // Получаем экземпляр класса, который описывает как должен выглядеть примитив
                    // т.е. правила построения графики внутри блока
                    // Информация соберается по XData и свойствам самого блока
                    var breakLine = BreakLineXDataHelper.GetBreakLineFromEntity(entity);
                    // Параноя программиста =)
                    if (breakLine != null)
                    {
                        // Получаем первую ручку (совпадает с точкой вставки блока)
                        var gp = new BreakLineGrip
                        {
                            GripType = MPCOGrips.MPCOEntityGripType.Point,
                            breakLine = breakLine,
                            GripName = BreakLineGripName.StartGrip,
                            GripPoint = breakLine.StartGrip // вот эта точка из экземпляра класса breakline
                        };
                        grips.Add(gp);
                        // получаем среднюю ручку
                        gp = new BreakLineGrip
                        {
                            GripType = MPCOGrips.MPCOEntityGripType.Point,
                            breakLine = breakLine,
                            GripName = BreakLineGripName.MiddleGrip,
                            GripPoint = breakLine.MiddleGrip
                        };
                        grips.Add(gp);
                        // получаем конечную ручку
                        gp = new BreakLineGrip
                        {
                            GripType = MPCOGrips.MPCOEntityGripType.Point,
                            breakLine = breakLine,
                            GripName = BreakLineGripName.EndGrip,
                            GripPoint = breakLine.EndGrip
                        };
                        grips.Add(gp);
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        /// <summary>
        /// Перемещение ручек
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="grips"></param>
        /// <param name="offset"></param>
        /// <param name="bitFlags"></param>
        public override void MoveGripPointsAt(Entity entity, GripDataCollection grips, Vector3d offset, MoveGripPointsFlags bitFlags)
        {
            try
            {
                // Проходим по коллекции ручек
                foreach (GripData gripData in grips)
                {
                    // Приводим ручку к моему классу
                    var gripPoint = gripData as BreakLineGrip;
                    // Проверяем, что это та ручка, что мне нужна. На всякий случай сверяю ObjectId блока, записанный в экземпляре
                    // класса breakline с ObjectId примитива, передаваемого в данный метод
                    if (gripPoint != null)
                    {
                        // Далее, в зависимости от имени ручки произвожу действия
                        if (gripPoint.GripName == BreakLineGripName.StartGrip)
                        {
                            // Переношу точку вставки блока, и точку, описывающую первую точку в примитиве
                            // Все точки всегда совпадают (+ ручка)
                            var newPt = gripPoint.GripPoint + offset;
                            var length = gripPoint.breakLine.EndPoint.DistanceTo(newPt);
                            var scale = gripPoint.breakLine.GetScale();
                            if (length < gripPoint.breakLine.BreakLineMinLength * scale * gripPoint.breakLine.BlockTransform.GetScale())
                            {
                                /* Если новая точка получается на расстоянии меньше минимального, то
                                 * переносим ее в направлении между двумя точками на минимальное расстояние
                                 */
                                var tmpInsertionPoint = GeometryHelpers.Point3dAtDirection(
                                    gripPoint.breakLine.EndPoint, newPt, gripPoint.breakLine.EndPoint,
                                    gripPoint.breakLine.BreakLineMinLength * scale * gripPoint.breakLine.BlockTransform.GetScale());

                                if (gripPoint.breakLine.EndPoint.Equals(newPt))
                                {
                                    // Если точки совпали, то задаем минимальное значение
                                    tmpInsertionPoint = new Point3d(gripPoint.breakLine.EndPoint.X + gripPoint.breakLine.BreakLineMinLength * scale * gripPoint.breakLine.BlockTransform.GetScale(), gripPoint.breakLine.EndPoint.Y, gripPoint.breakLine.EndPoint.Z);
                                }
                                
                                ((BlockReference)entity).Position = tmpInsertionPoint;
                                gripPoint.breakLine.InsertionPoint = tmpInsertionPoint;
                            }
                            else
                            {
                                ((BlockReference) entity).Position = gripPoint.GripPoint + offset;
                                gripPoint.breakLine.InsertionPoint = gripPoint.GripPoint + offset;
                            }
                        }
                        if (gripPoint.GripName == BreakLineGripName.MiddleGrip)
                        {
                            // Т.к. средняя точка нужна для переноса примитива, но не соответсвует точки вставки блока
                            // и получается как средняя точка между InsertionPoint и EndPoint, то я переношу
                            // точку вставки
                            var lenghtVector = (gripPoint.breakLine.InsertionPoint - gripPoint.breakLine.EndPoint) / 2;
                            ((BlockReference)entity).Position = gripPoint.GripPoint + offset + lenghtVector;
                        }
                        if (gripPoint.GripName == BreakLineGripName.EndGrip)
                        {
                            var newPt = gripPoint.GripPoint + offset;
                            if (newPt.Equals(((BlockReference) entity).Position))
                            {
                                var scale = gripPoint.breakLine.GetScale();
                                gripPoint.breakLine.EndPoint = new Point3d(
                                    ((BlockReference)entity).Position.X + gripPoint.breakLine.BreakLineMinLength * scale * gripPoint.breakLine.BlockTransform.GetScale(),
                                    ((BlockReference)entity).Position.Y, ((BlockReference)entity).Position.Z);
                            }
                            // С конечной точкой все просто
                            else gripPoint.breakLine.EndPoint = gripPoint.GripPoint + offset;
                        }
                        // Вот тут происходит перерисовка примитивов внутри блока
                        gripPoint.breakLine.UpdateEntities();
                        gripPoint.breakLine.BlockRecord.UpdateAnonymousBlocks();
                    }
                    else base.MoveGripPointsAt(entity, grips, offset, bitFlags);
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
        
        // Проверка поддерживаемости примитива
        // Проверка происходит по наличию XData с определенным AppName
        public override bool IsApplicable(RXObject overruledSubject)
        {
            return ExtendedDataHelpers.IsApplicable(overruledSubject, BreakLineFunction.MPCOEntName);
        }
    }
    /* Так как у линии обрыва все точки одинаковы, то достаточно создать одно переопределени
     * Если есть сильная разница, то можно создавать несколько GripData. Однако нужны тесты
     */
    /// <summary>
    /// Описание ручки линии обрыва
    /// </summary>
    public class BreakLineGrip : MPCOGrips.MPCOGripData //<-- Там будут определны типы точек и их ViewportDraw в зависимости от типа. Пока ничего этого нет
    {
        // Экземпляр класса breakline, связанный с этой ручкой
        public BreakLine breakLine { get; set; }
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
                        return "Растянуть";
                    }
                case BreakLineGripName.MiddleGrip: return "Переместить";
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
                //AcadHelpers.Editor.WriteMessage("\n OnGripStatusChanged in GripData entity id: " + entity.ObjectId);
                // При начале перемещения запоминаем первоначальное положение ручки
                // Запоминаем начальные значения
                if (newStatus == Status.GripStart)
                {
                    if (GripName == BreakLineGripName.StartGrip)
                        _startGripTmp = GripPoint;
                    if (GripName == BreakLineGripName.EndGrip)
                        _endGripTmp = GripPoint;
                    if (GripName == BreakLineGripName.MiddleGrip)
                    {
                        _startGripTmp = breakLine.StartGrip;
                        _endGripTmp = breakLine.EndGrip;
                    }
                }

                // При удачном перемещении ручки записываем новые значения в расширенные данные
                // По этим данным я потом получаю экземпляр класса breakline
                if (newStatus == Status.GripEnd)
                {
                    using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var blkRef = tr.GetObject(breakLine.BlockId, OpenMode.ForWrite);
                        using (var resBuf = breakLine.GetParametersForXData())
                        {
                            blkRef.XData = resBuf;
                        }
                        tr.Commit();
                    }
                    breakLine.Dispose();
                }
                // При отмене перемещения возвращаем временные значения
                if (newStatus == Status.GripAbort)
                {
                    if (_startGripTmp != null & GripName == BreakLineGripName.StartGrip)
                        breakLine.EndPoint = GripPoint;
                    if (GripName == BreakLineGripName.MiddleGrip & _startGripTmp != null & _endGripTmp != null)
                    {
                        breakLine.InsertionPoint = _startGripTmp;
                        breakLine.EndPoint = _endGripTmp;
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
    // Имена точек. Использую вместо индекса
    public enum BreakLineGripName
    {
        StartGrip,
        MiddleGrip,
        EndGrip
    }
}
