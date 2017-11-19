using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using mpESKD.Base.Helpers;
using mpESKD.Base.Overrules;
using ModPlusAPI.Windows;
using Autodesk.AutoCAD.Runtime;
using mpESKD.Functions.mpAxis.Properties;
using ModPlus.Helpers;

// ReSharper disable InconsistentNaming

namespace mpESKD.Functions.mpAxis.Overrules
{
    public class AxisGripPointsOverrule : GripOverrule
    {
        protected static AxisGripPointsOverrule _axisGripPointOverrule;
        public static AxisGripPointsOverrule Instance()
        {
            if (_axisGripPointOverrule != null) return _axisGripPointOverrule;
            _axisGripPointOverrule = new AxisGripPointsOverrule();
            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _axisGripPointOverrule.SetXDataFilter(AxisFunction.MPCOEntName);
            return _axisGripPointOverrule;
        }
        /// <summary>Получение ручек для примитива</summary>
        public override void GetGripPoints(Entity entity, GripDataCollection grips, double curViewUnitSize, int gripSize, Vector3d curViewDir,
            GetGripPointsFlags bitFlags)
        {
            try
            {
                // Проверка дополнительных условий
                if (IsApplicable(entity))
                {
                    // Чтобы "отключить" точку вставки блока, нужно получить сначал блок
                    // Т.к. мы точно знаем для какого примитива переопределение, то получаем блок:
                    BlockReference blkRef = (BlockReference)entity;
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
                    var axis = AxisXDataHelper.GetAxisFromEntity(entity);
                    // Параноя программиста =)
                    if (axis != null)
                    {
                        // Получаем первую ручку (совпадает с точкой вставки блока)
                        var gp = new AxisGrip
                        {
                            GripType = MPCOGrips.MPCOEntityGripType.Point,
                            Axis = axis,
                            GripName = AxisGripName.StartGrip,
                            GripPoint = axis.StartGrip // вот эта точка из экземпляра класса axis
                        };
                        grips.Add(gp);
                        // получаем среднюю ручку
                        gp = new AxisGrip
                        {
                            GripType = MPCOGrips.MPCOEntityGripType.Point,
                            Axis = axis,
                            GripName = AxisGripName.MiddleGrip,
                            GripPoint = axis.MiddleGrip
                        };
                        grips.Add(gp);
                        // получаем конечную ручку
                        gp = new AxisGrip
                        {
                            GripType = MPCOGrips.MPCOEntityGripType.Point,
                            Axis = axis,
                            GripName = AxisGripName.EndGrip,
                            GripPoint = axis.EndGrip
                        };
                        grips.Add(gp);
                        if (axis.MarkersPosition == AxisMarkersPosition.Both ||
                            axis.MarkersPosition == AxisMarkersPosition.Bottom)
                        {
                            // other points
                            gp = new AxisGrip
                            {
                                GripType = MPCOGrips.MPCOEntityGripType.Point,
                                Axis = axis,
                                GripName = AxisGripName.BottomMarkerGrip,
                                GripPoint = axis.BottomMarkerGrip
                            };
                            grips.Add(gp);
                        }
                        if (axis.MarkersPosition == AxisMarkersPosition.Both ||
                            axis.MarkersPosition == AxisMarkersPosition.Top)
                        {
                            gp = new AxisGrip
                            {
                                GripType = MPCOGrips.MPCOEntityGripType.Point,
                                Axis = axis,
                                GripName = AxisGripName.TopMarkerGrip,
                                GripPoint = axis.TopMarkerGrip
                            };
                            grips.Add(gp);
                        }
                        // orient
                        if (axis.BottomOrientMarkerVisible)
                        {
                            gp = new AxisGrip
                            {
                                GripType = MPCOGrips.MPCOEntityGripType.Point,
                                Axis = axis,
                                GripName = AxisGripName.BottomOrientGrip,
                                GripPoint = axis.BottomOrientGrip
                            };
                            grips.Add(gp);
                        }
                        if (axis.TopOrientMarkerVisible)
                        {
                            gp = new AxisGrip
                            {
                                GripType = MPCOGrips.MPCOEntityGripType.Point,
                                Axis = axis,
                                GripName = AxisGripName.TopOrientGrip,
                                GripPoint = axis.TopOrientGrip
                            };
                            grips.Add(gp);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        /// <summary>Перемещение ручек</summary>
        public override void MoveGripPointsAt(Entity entity, GripDataCollection grips, Vector3d offset, MoveGripPointsFlags bitFlags)
        {
            try
            {
                // Проходим по коллекции ручек
                foreach (GripData gripData in grips)
                {
                    // Приводим ручку к моему классу
                    var gripPoint = gripData as AxisGrip;
                    // Проверяем, что это та ручка, что мне нужна. 
                    if (gripPoint != null)
                    {
                        // Далее, в зависимости от имени ручки произвожу действия
                        if (gripPoint.GripName == AxisGripName.StartGrip)
                        {
                            // Переношу точку вставки блока, и точку, описывающую первую точку в примитиве
                            // Все точки всегда совпадают (+ ручка)
                            var newPt = gripPoint.GripPoint + offset;
                            var length = gripPoint.Axis.EndPoint.DistanceTo(newPt);
                            var scale = gripPoint.Axis.GetScale();
                            if (length < gripPoint.Axis.AxisMinLength * scale * gripPoint.Axis.BlockTransform.GetScale())
                            {
                                /* Если новая точка получается на расстоянии меньше минимального, то
                                 * переносим ее в направлении между двумя точками на минимальное расстояние
                                 */
                                var tmpInsertionPoint = GeometryHelpers.Point3dAtDirection(
                                    gripPoint.Axis.EndPoint, newPt, gripPoint.Axis.EndPoint,
                                    gripPoint.Axis.AxisMinLength * scale * gripPoint.Axis.BlockTransform.GetScale());

                                if (gripPoint.Axis.EndPoint.Equals(newPt))
                                {
                                    // Если точки совпали, то задаем минимальное значение
                                    tmpInsertionPoint = new Point3d(
                                        gripPoint.Axis.EndPoint.X,
                                        gripPoint.Axis.EndPoint.Y - gripPoint.Axis.AxisMinLength * scale * gripPoint.Axis.BlockTransform.GetScale(),
                                        gripPoint.Axis.EndPoint.Z);
                                }

                                ((BlockReference)entity).Position = tmpInsertionPoint;
                                gripPoint.Axis.InsertionPoint = tmpInsertionPoint;
                            }
                            else
                            {
                                ((BlockReference)entity).Position = gripPoint.GripPoint + offset;
                                gripPoint.Axis.InsertionPoint = gripPoint.GripPoint + offset;
                            }
                        }
                        if (gripPoint.GripName == AxisGripName.MiddleGrip)
                        {
                            // Т.к. средняя точка нужна для переноса примитива, но не соответсвует точки вставки блока
                            // и получается как средняя точка между InsertionPoint и EndPoint, то я переношу
                            // точку вставки
                            var lenghtVector = (gripPoint.Axis.InsertionPoint - gripPoint.Axis.EndPoint) / 2;
                            ((BlockReference)entity).Position = gripPoint.GripPoint + offset + lenghtVector;
                        }
                        if (gripPoint.GripName == AxisGripName.EndGrip)
                        {
                            var newPt = gripPoint.GripPoint + offset;
                            if (newPt.Equals(((BlockReference)entity).Position))
                            {
                                var scale = gripPoint.Axis.GetScale();
                                gripPoint.Axis.EndPoint = new Point3d(
                                    ((BlockReference)entity).Position.X,
                                    ((BlockReference)entity).Position.Y - gripPoint.Axis.AxisMinLength * scale * gripPoint.Axis.BlockTransform.GetScale(),
                                    ((BlockReference)entity).Position.Z);
                            }
                            // С конечной точкой все просто
                            else gripPoint.Axis.EndPoint = gripPoint.GripPoint + offset;
                        }
                        if (gripPoint.GripName == AxisGripName.BottomMarkerGrip)
                        {
                            var mainVector = gripPoint.Axis.EndPoint - gripPoint.Axis.InsertionPoint;
                            var v = mainVector.CrossProduct(Vector3d.ZAxis).GetNormal();
                            gripPoint.Axis.BottomMarkerPoint = gripPoint.GripPoint + offset.DotProduct(v) * v;
                            // Меняю также точку маркера-ориентира
                            //gripPoint.Axis.BottomOrientPoint 
                        }
                        if (gripPoint.GripName == AxisGripName.TopMarkerGrip)
                        {
                            var mainVector = gripPoint.Axis.InsertionPoint - gripPoint.Axis.EndPoint;
                            var v = mainVector.CrossProduct(Vector3d.ZAxis).GetNormal();
                            gripPoint.Axis.TopMarkerPoint = gripPoint.GripPoint + offset.DotProduct(v) * v;
                        }
                        if (gripPoint.GripName == AxisGripName.BottomOrientGrip)
                        {
                            var mainVector = gripPoint.Axis.EndPoint - gripPoint.Axis.InsertionPoint;
                            var v = mainVector.CrossProduct(Vector3d.ZAxis).GetNormal();
                            gripPoint.Axis.BottomOrientPoint = gripPoint.GripPoint + offset.DotProduct(v) * v;
                        }
                        // Вот тут происходит перерисовка примитивов внутри блока
                        gripPoint.Axis.UpdateEntities();
                        gripPoint.Axis.BlockRecord.UpdateAnonymousBlocks();
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
            return ExtendedDataHelpers.IsApplicable(overruledSubject, AxisFunction.MPCOEntName);
        }
    }
    /* Так как у линии обрыва все точки одинаковы, то достаточно создать одно переопределени
     * Если есть сильная разница, то можно создавать несколько GripData. Однако нужны тесты
     */
    /// <summary>Описание ручки линии обрыва</summary>
    public class AxisGrip : MPCOGrips.MPCOGripData //<-- Там будут определны типы точек и их ViewportDraw в зависимости от типа. Пока ничего этого нет
    {
        public AxisGrip()
        {
            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;
        }
        // Экземпляр класса breakline, связанный с этой ручкой
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
                        return "Растянуть";
                    }
                case AxisGripName.MiddleGrip: return "Переместить";
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
                //AcadHelpers.Editor.WriteMessage("\n OnGripStatusChanged in GripData entity id: " + entity.ObjectId);
                // При начале перемещения запоминаем первоначальное положение ручки
                // Запоминаем начальные значения
                if (newStatus == Status.GripStart)
                {
                    if (GripName == AxisGripName.StartGrip)
                        _startGripTmp = GripPoint;
                    if (GripName == AxisGripName.EndGrip)
                        _endGripTmp = GripPoint;
                    if (GripName == AxisGripName.BottomMarkerGrip)
                        _bottomMarkerGripTmp = GripPoint;
                    if (GripName == AxisGripName.TopMarkerGrip)
                        _topMarkerGripTmp = GripPoint;
                    if (GripName == AxisGripName.BottomOrientGrip)
                        _bottomOrientGripTmp = GripPoint;
                    if (GripName == AxisGripName.TopOrientGrip)
                        _topOrientGripTmp = GripPoint;
                    if (GripName == AxisGripName.MiddleGrip)
                    {
                        _startGripTmp = Axis.StartGrip;
                        _endGripTmp = Axis.EndGrip;
                    }
                }

                // При удачном перемещении ручки записываем новые значения в расширенные данные
                // По этим данным я потом получаю экземпляр класса breakline
                if (newStatus == Status.GripEnd)
                {
                    using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var blkRef = tr.GetObject(Axis.BlockId, OpenMode.ForWrite);
                        using (var resBuf = Axis.GetParametersForXData())
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
                    if (_startGripTmp != null && GripName == AxisGripName.StartGrip)
                        Axis.EndPoint = GripPoint;
                    if (_bottomMarkerGripTmp != null && GripName == AxisGripName.BottomMarkerGrip)
                        Axis.BottomMarkerPoint = GripPoint;
                    if (_topMarkerGripTmp != null && GripName == AxisGripName.TopMarkerGrip)
                        Axis.TopMarkerPoint = GripPoint;
                    if (_bottomOrientGripTmp != null && GripName == AxisGripName.BottomOrientGrip)
                        Axis.BottomOrientPoint = GripPoint;
                    if (_topOrientGripTmp != null && GripName == AxisGripName.TopOrientGrip)
                        Axis.TopOrientPoint = GripPoint;
                    if (GripName == AxisGripName.MiddleGrip && _startGripTmp != null && _endGripTmp != null)
                    {
                        Axis.InsertionPoint = _startGripTmp;
                        Axis.EndPoint = _endGripTmp;
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
    public enum AxisGripName
    {
        StartGrip,
        MiddleGrip,
        EndGrip,
        BottomMarkerGrip,
        TopMarkerGrip,
        BottomOrientGrip,
        TopOrientGrip
    }
}
