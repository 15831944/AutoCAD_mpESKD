// ReSharper disable InconsistentNaming

namespace mpESKD.Functions.mpAxis.Overrules
{
    using System;
    using System.Diagnostics;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base.Helpers;
    using mpESKD.Base.Overrules;
    using ModPlusAPI.Windows;
    using Autodesk.AutoCAD.Runtime;
    using Properties;
    using ModPlusAPI;
    using Base;
    using Base.Enums;

    public class AxisGripPointsOverrule : GripOverrule
    {
        protected static AxisGripPointsOverrule _axisGripPointOverrule;
        public static AxisGripPointsOverrule Instance()
        {
            if (_axisGripPointOverrule != null) return _axisGripPointOverrule;
            _axisGripPointOverrule = new AxisGripPointsOverrule();
            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _axisGripPointOverrule.SetXDataFilter(AxisDescriptor.Instance.Name);
            return _axisGripPointOverrule;
        }
        private Point3d InitEndPoint;
        private Point3d InitInsertionPoint;
        private Point3d InitBottomOrientPoint;
        private Point3d InitTopOrientPoint;
        /// <summary>Получение ручек для примитива</summary>
        public override void GetGripPoints(Entity entity, GripDataCollection grips, double curViewUnitSize, int gripSize, Vector3d curViewDir,
            GetGripPointsFlags bitFlags)
        {
            try
            {
                Debug.Print("AxisGripPointsOverrule");
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
                    var axis = EntityReaderFactory.Instance.GetFromEntity<Axis>(entity);
                    // Параноя программиста =)
                    if (axis != null)
                    {
                        // Получаем первую ручку (совпадает с точкой вставки блока)
                        var gp = new AxisGrip
                        {
                            GripType = GripType.Point,
                            Axis = axis,
                            GripName = AxisGripName.StartGrip,
                            GripPoint = axis.InsertionPoint // вот эта точка из экземпляра класса axis
                        };
                        grips.Add(gp);
                        InitInsertionPoint = axis.InsertionPoint;
                        // получаем среднюю ручку
                        gp = new AxisGrip
                        {
                            GripType = GripType.Point,
                            Axis = axis,
                            GripName = AxisGripName.MiddleGrip,
                            GripPoint = axis.MiddlePoint
                        };
                        grips.Add(gp);
                        // получаем конечную ручку
                        gp = new AxisGrip
                        {
                            GripType = GripType.Point,
                            Axis = axis,
                            GripName = AxisGripName.EndGrip,
                            GripPoint = axis.EndPoint
                        };
                        grips.Add(gp);
                        InitEndPoint = axis.EndPoint;
                        if (axis.MarkersPosition == AxisMarkersPosition.Both ||
                            axis.MarkersPosition == AxisMarkersPosition.Bottom)
                        {
                            // other points
                            gp = new AxisGrip
                            {
                                GripType = GripType.Point,
                                Axis = axis,
                                GripName = AxisGripName.BottomMarkerGrip,
                                GripPoint = axis.BottomMarkerPoint
                            };
                            grips.Add(gp);
                        }
                        if (axis.MarkersPosition == AxisMarkersPosition.Both ||
                            axis.MarkersPosition == AxisMarkersPosition.Top)
                        {
                            gp = new AxisGrip
                            {
                                GripType = GripType.Point,
                                Axis = axis,
                                GripName = AxisGripName.TopMarkerGrip,
                                GripPoint = axis.TopMarkerPoint
                            };
                            grips.Add(gp);
                        }
                        // orient
                        if (axis.MarkersPosition == AxisMarkersPosition.Both || axis.MarkersPosition == AxisMarkersPosition.Bottom)
                            if (axis.BottomOrientMarkerVisible)
                            {
                                gp = new AxisGrip
                                {
                                    GripType = GripType.Point,
                                    Axis = axis,
                                    GripName = AxisGripName.BottomOrientGrip,
                                    GripPoint = axis.BottomOrientPoint
                                };
                                grips.Add(gp);
                                InitBottomOrientPoint = axis.BottomOrientPoint;
                            }
                        if (axis.MarkersPosition == AxisMarkersPosition.Both || axis.MarkersPosition == AxisMarkersPosition.Top)
                            if (axis.TopOrientMarkerVisible)
                            {
                                gp = new AxisGrip
                                {
                                    GripType = GripType.Point,
                                    Axis = axis,
                                    GripName = AxisGripName.TopOrientGrip,
                                    GripPoint = axis.TopOrientPoint
                                };
                                grips.Add(gp);
                                InitTopOrientPoint = axis.TopOrientPoint;
                            }
                    }
                }
            }
            catch (System.Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
        
        /// <summary>Перемещение ручек</summary>
        public override void MoveGripPointsAt(Entity entity, GripDataCollection grips, Vector3d offset, MoveGripPointsFlags bitFlags)
        {
            try
            {
                if (IsApplicable(entity))
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
                                if (length < gripPoint.Axis.AxisMinLength * GetFullScale(gripPoint))
                                {
                                    /* Если новая точка получается на расстоянии меньше минимального, то
                                     * переносим ее в направлении между двумя точками на минимальное расстояние
                                     */
                                    var tmpInsertionPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(
                                        gripPoint.Axis.EndPoint, newPt, gripPoint.Axis.EndPoint,
                                        gripPoint.Axis.AxisMinLength * GetFullScale(gripPoint));

                                    if (gripPoint.Axis.EndPoint.Equals(newPt))
                                    {
                                        // Если точки совпали, то задаем минимальное значение
                                        tmpInsertionPoint = new Point3d(
                                            gripPoint.Axis.EndPoint.X,
                                            gripPoint.Axis.EndPoint.Y - gripPoint.Axis.AxisMinLength * GetFullScale(gripPoint),
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
                                // Т.к. средняя точка нужна для переноса примитива, но не соответствует точки вставки блока
                                // и получается как средняя точка между InsertionPoint и EndPoint, то я переношу
                                // точку вставки
                                var lenghtVector = (gripPoint.Axis.InsertionPoint - gripPoint.Axis.EndPoint) / 2;
                                ((BlockReference)entity).Position = gripPoint.GripPoint + offset + lenghtVector;
                            }

                            if (gripPoint.GripName == AxisGripName.EndGrip)
                            {
                                var newPt = gripPoint.GripPoint + offset;
                                Point3d newEnedPoint;
                                if (newPt.Equals(((BlockReference)entity).Position))
                                {
                                    var scale = gripPoint.Axis.GetScale();
                                    newEnedPoint = new Point3d(
                                        ((BlockReference)entity).Position.X,
                                        ((BlockReference)entity).Position.Y - gripPoint.Axis.AxisMinLength * scale *
                                        gripPoint.Axis.BlockTransform.GetScale(),
                                        ((BlockReference)entity).Position.Z);
                                    gripPoint.Axis.EndPoint = newEnedPoint;
                                }
                                else
                                {
                                    gripPoint.Axis.EndPoint = gripPoint.GripPoint + offset;
                                    newEnedPoint = gripPoint.GripPoint + offset;
                                }

                                // change bottom orient point
                                gripPoint.Axis.BottomOrientPoint = GetSavePositionPoint(
                                    InitInsertionPoint,
                                    gripPoint.GripPoint,
                                    newEnedPoint,
                                    InitBottomOrientPoint
                                );
                            }

                            if (gripPoint.GripName == AxisGripName.BottomMarkerGrip)
                            {
                                var mainVector = gripPoint.Axis.EndPoint - gripPoint.Axis.InsertionPoint;
                                var v = mainVector.CrossProduct(Vector3d.ZAxis).GetNormal();
                                gripPoint.Axis.BottomMarkerPoint = gripPoint.GripPoint + offset.DotProduct(v) * v;
                                // Меняю также точку маркера-ориентира
                                if (InitBottomOrientPoint != Point3d.Origin)
                                    gripPoint.Axis.BottomOrientPoint = InitBottomOrientPoint + offset.DotProduct(v) * v;
                            }

                            if (gripPoint.GripName == AxisGripName.TopMarkerGrip)
                            {
                                var mainVector = gripPoint.Axis.InsertionPoint - gripPoint.Axis.EndPoint;
                                var v = mainVector.CrossProduct(Vector3d.ZAxis).GetNormal();
                                gripPoint.Axis.TopMarkerPoint = gripPoint.GripPoint + offset.DotProduct(v) * v;
                                // Меняю также точку маркера-ориентира
                                if (InitTopOrientPoint != Point3d.Origin)
                                    gripPoint.Axis.TopOrientPoint = InitTopOrientPoint + offset.DotProduct(v) * v;
                            }

                            if (gripPoint.GripName == AxisGripName.BottomOrientGrip)
                            {
                                var mainVector = gripPoint.Axis.EndPoint - gripPoint.Axis.InsertionPoint;
                                Vector3d v = mainVector.CrossProduct(Vector3d.ZAxis).GetNormal();
                                var newPoint = gripPoint.GripPoint + offset.DotProduct(v) * v;

                                if (Math.Abs((newPoint - gripPoint.Axis.BottomMarkerPoint).Length) >
                                    gripPoint.Axis.MarkersDiameter * GetFullScale(gripPoint))
                                    gripPoint.Axis.BottomOrientPoint = newPoint;
                                else
                                {
                                    if (newPoint.X >= gripPoint.Axis.BottomMarkerPoint.X)
                                        gripPoint.Axis.BottomOrientPoint =
                                            gripPoint.Axis.BottomMarkerPoint + v * -1 * gripPoint.Axis.MarkersDiameter *
                                            GetFullScale(gripPoint);
                                    else
                                        gripPoint.Axis.BottomOrientPoint =
                                            gripPoint.Axis.BottomMarkerPoint + v * gripPoint.Axis.MarkersDiameter *
                                            GetFullScale(gripPoint);
                                }
                            }

                            if (gripPoint.GripName == AxisGripName.TopOrientGrip)
                            {
                                var mainVector = gripPoint.Axis.InsertionPoint - gripPoint.Axis.EndPoint;
                                Vector3d v = mainVector.CrossProduct(Vector3d.ZAxis).GetNormal();
                                var newPoint = gripPoint.GripPoint + offset.DotProduct(v) * v;

                                if (Math.Abs((newPoint - gripPoint.Axis.TopMarkerPoint).Length) >
                                    gripPoint.Axis.MarkersDiameter * GetFullScale(gripPoint))
                                    gripPoint.Axis.TopOrientPoint = newPoint;
                                else
                                {
                                    if (newPoint.X >= gripPoint.Axis.TopMarkerPoint.X)
                                        gripPoint.Axis.TopOrientPoint =
                                            gripPoint.Axis.TopMarkerPoint + v * gripPoint.Axis.MarkersDiameter *
                                            GetFullScale(gripPoint);
                                    else
                                        gripPoint.Axis.TopOrientPoint =
                                            gripPoint.Axis.TopMarkerPoint + v * -1 * gripPoint.Axis.MarkersDiameter *
                                            GetFullScale(gripPoint);
                                }
                            }

                            // Вот тут происходит перерисовка примитивов внутри блока
                            gripPoint.Axis.UpdateEntities();
                            gripPoint.Axis.BlockRecord.UpdateAnonymousBlocks();
                        }
                        else base.MoveGripPointsAt(entity, grips, offset, bitFlags);
                    }
                }
                else base.MoveGripPointsAt(entity, grips, offset, bitFlags);
            }
            catch (System.Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
        // Проверка поддерживаемости примитива
        // Проверка происходит по наличию XData с определенным AppName
        public override bool IsApplicable(RXObject overruledSubject)
        {
            return ExtendedDataHelpers.IsApplicable(overruledSubject, AxisDescriptor.Instance.Name);
        }

        #region Helpers
        /// <summary>Получение нового значения точки, которая должна сохранить свое положение
        /// относительно передвигаемой точки</summary>
        /// <param name="basePoint">Базовая точка - точка, относительно которой все двигается</param>
        /// <param name="oldMovedpoint">Начальное значение передвигаемой точки</param>
        /// <param name="newMovedPoint">Новое значение передвигаемой точки</param>
        /// <param name="oldSavePositionPoint">Начальное значение точки, положение которой должно сохранится
        /// относительно передвигаемой точки</param>
        /// <returns></returns>
        private static Point3d GetSavePositionPoint(Point3d basePoint, Point3d oldMovedpoint, Point3d newMovedPoint, Point3d oldSavePositionPoint)
        {
            var vectorOld = oldMovedpoint - basePoint;
            var vectorNew = newMovedPoint - basePoint;
            var l = vectorNew.Length - vectorOld.Length;
            var h = l * vectorOld.GetNormal();
            var angle = vectorOld.GetAngleTo(vectorNew, Vector3d.ZAxis);

            var tmpP = new Point3d(
                oldSavePositionPoint.X - basePoint.X,
                oldSavePositionPoint.Y - basePoint.Y,
                oldSavePositionPoint.Z) + h;
            return new Point3d(
                tmpP.X * Math.Cos(angle) - tmpP.Y * Math.Sin(angle) + basePoint.X,
                tmpP.X * Math.Sin(angle) + tmpP.Y * Math.Cos(angle) + basePoint.Y,
                oldSavePositionPoint.Z
            );
        }
        /// <summary>Получение "полного" масштаба примитива (масштаб в свойствах умноженный на масштаб блока)</summary>
        /// <param name="gripPoint"></param>
        /// <returns></returns>
        private static double GetFullScale(AxisGrip gripPoint)
        {
            return gripPoint.Axis.GetScale() * gripPoint.Axis.BlockTransform.GetScale();
        }

        #endregion
    }
    /* Так как у линии обрыва все точки одинаковы, то достаточно создать одно переопределени
     * Если есть сильная разница, то можно создавать несколько GripData. Однако нужны тесты
     */
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
