// ReSharper disable InconsistentNaming

namespace mpESKD.Functions.mpBreakLine.Overrules
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Base.Helpers;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using Base;
    using Base.Enums;
    using Base.Overrules;

    /// <summary>Класс, создающий и обрабатывающий переопределение ручек</summary>
    public class BreakLineGripPointsOverrule : GripOverrule
    {
        protected static BreakLineGripPointsOverrule _breakLineGripPointOverrule;
        public static BreakLineGripPointsOverrule Instance()
        {
            if (_breakLineGripPointOverrule != null) return _breakLineGripPointOverrule;
            _breakLineGripPointOverrule = new BreakLineGripPointsOverrule();
            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _breakLineGripPointOverrule.SetXDataFilter(BreakLineFunction.MPCOEntName);
            return _breakLineGripPointOverrule;
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
                    // Чтобы "отключить" точку вставки блока, нужно получить сначала блок
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
                    // Информация собирается по XData и свойствам самого блока
                    var breakLine = EntityReaderFactory.Instance.GetFromEntity<BreakLine>(entity);
                    // Паранойя программиста =)
                    if (breakLine != null)
                    {
                        // Получаем первую ручку (совпадает с точкой вставки блока)
                        var gp = new BreakLineGrip
                        {
                            GripType = GripType.Point,
                            BreakLine = breakLine,
                            GripName = BreakLineGripName.StartGrip,
                            GripPoint = breakLine.InsertionPoint // вот эта точка из экземпляра класса breakline
                        };
                        grips.Add(gp);
                        // получаем среднюю ручку
                        gp = new BreakLineGrip
                        {
                            GripType = GripType.Point,
                            BreakLine = breakLine,
                            GripName = BreakLineGripName.MiddleGrip,
                            GripPoint = breakLine.MiddlePoint
                        };
                        grips.Add(gp);
                        // получаем конечную ручку
                        gp = new BreakLineGrip
                        {
                            GripType = GripType.Point,
                            BreakLine = breakLine,
                            GripName = BreakLineGripName.EndGrip,
                            GripPoint = breakLine.EndPoint
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
                        if (gripData is BreakLineGrip gripPoint)
                        {
                            // Далее, в зависимости от имени ручки произвожу действия
                            if (gripPoint.GripName == BreakLineGripName.StartGrip)
                            {
                                // Переношу точку вставки блока, и точку, описывающую первую точку в примитиве
                                // Все точки всегда совпадают (+ ручка)
                                var newPt = gripPoint.GripPoint + offset;
                                var length = gripPoint.BreakLine.EndPoint.DistanceTo(newPt);
                                var scale = gripPoint.BreakLine.GetScale();
                                if (length < gripPoint.BreakLine.BreakLineMinLength * scale * gripPoint.BreakLine.BlockTransform.GetScale())
                                {
                                    /* Если новая точка получается на расстоянии меньше минимального, то
                                     * переносим ее в направлении между двумя точками на минимальное расстояние
                                     */
                                    var tmpInsertionPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(
                                        gripPoint.BreakLine.EndPoint, newPt, gripPoint.BreakLine.EndPoint,
                                        gripPoint.BreakLine.BreakLineMinLength * scale * gripPoint.BreakLine.BlockTransform.GetScale());

                                    if (gripPoint.BreakLine.EndPoint.Equals(newPt))
                                    {
                                        // Если точки совпали, то задаем минимальное значение
                                        tmpInsertionPoint = new Point3d(gripPoint.BreakLine.EndPoint.X + gripPoint.BreakLine.BreakLineMinLength * scale * gripPoint.BreakLine.BlockTransform.GetScale(),
                                            gripPoint.BreakLine.EndPoint.Y, gripPoint.BreakLine.EndPoint.Z);
                                    }

                                    ((BlockReference)entity).Position = tmpInsertionPoint;
                                    gripPoint.BreakLine.InsertionPoint = tmpInsertionPoint;
                                }
                                else
                                {
                                    ((BlockReference)entity).Position = gripPoint.GripPoint + offset;
                                    gripPoint.BreakLine.InsertionPoint = gripPoint.GripPoint + offset;
                                }
                            }

                            if (gripPoint.GripName == BreakLineGripName.MiddleGrip)
                            {
                                // Т.к. средняя точка нужна для переноса примитива, но не соответствует точки вставки блока
                                // и получается как средняя точка между InsertionPoint и EndPoint, то я переношу
                                // точку вставки
                                var lenghtVector = (gripPoint.BreakLine.InsertionPoint - gripPoint.BreakLine.EndPoint) / 2;
                                ((BlockReference)entity).Position = gripPoint.GripPoint + offset + lenghtVector;
                            }

                            if (gripPoint.GripName == BreakLineGripName.EndGrip)
                            {
                                var newPt = gripPoint.GripPoint + offset;
                                if (newPt.Equals(((BlockReference)entity).Position))
                                {
                                    var scale = gripPoint.BreakLine.GetScale();
                                    gripPoint.BreakLine.EndPoint = new Point3d(
                                        ((BlockReference)entity).Position.X + gripPoint.BreakLine.BreakLineMinLength * scale * gripPoint.BreakLine.BlockTransform.GetScale(),
                                        ((BlockReference)entity).Position.Y, ((BlockReference)entity).Position.Z);
                                }
                                // С конечной точкой все просто
                                else gripPoint.BreakLine.EndPoint = gripPoint.GripPoint + offset;
                            }

                            // Вот тут происходит перерисовка примитивов внутри блока
                            gripPoint.BreakLine.UpdateEntities();
                            gripPoint.BreakLine.BlockRecord.UpdateAnonymousBlocks();
                        }
                        else base.MoveGripPointsAt(entity, grips, offset, bitFlags);
                    }
                }
                else base.MoveGripPointsAt(entity, grips, offset, bitFlags);
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
    /* Так как у линии обрыва все точки одинаковы, то достаточно создать одно переопределение
     * Если есть сильная разница, то можно создавать несколько GripData. Однако нужны тесты
     */
    /// <summary>Описание ручки линии обрыва</summary>
    public class BreakLineGrip : IntellectualEntityGripData //<-- Там будут определены типы точек и их ViewportDraw в зависимости от типа. Пока ничего этого нет
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
                        return Language.GetItem(MainFunction.LangItem, "gp1"); // stretch
                    }
                case BreakLineGripName.MiddleGrip: return Language.GetItem(MainFunction.LangItem, "gp2"); // move
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
                // AcadHelpers.Editor.WriteMessage("\n OnGripStatusChanged in GripData entity id: " + entity.ObjectId);

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
                        var blkRef = tr.GetObject(BreakLine.BlockId, OpenMode.ForWrite);
                        using (var resBuf = BreakLine.GetParametersForXData())
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
                        BreakLine.InsertionPoint = GripPoint;
                    if (GripName == BreakLineGripName.MiddleGrip & _startGripTmp != null & _endGripTmp != null)
                    {
                        BreakLine.InsertionPoint = _startGripTmp;
                        BreakLine.EndPoint = _endGripTmp;
                    }
                    if (_endGripTmp != null & GripName == BreakLineGripName.EndGrip)
                        BreakLine.EndPoint = GripPoint;
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
