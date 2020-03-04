namespace mpESKD.Functions.mpBreakLine.Overrules
{
    using System.Diagnostics;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using Grips;
    using ModPlusAPI.Windows;

    /// <summary>Класс, создающий и обрабатывающий переопределение ручек</summary>
    public class BreakLineGripPointsOverrule : GripOverrule
    {
        protected static BreakLineGripPointsOverrule _breakLineGripPointOverrule;

        public static BreakLineGripPointsOverrule Instance()
        {
            if (_breakLineGripPointOverrule != null)
            {
                return _breakLineGripPointOverrule;
            }

            _breakLineGripPointOverrule = new BreakLineGripPointsOverrule();

            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _breakLineGripPointOverrule.SetXDataFilter(BreakLineDescriptor.Instance.Name);
            return _breakLineGripPointOverrule;
        }

        /// <summary>Получение ручек для примитива</summary>
        public override void GetGripPoints(
            Entity entity, GripDataCollection grips, double curViewUnitSize, int gripSize, Vector3d curViewDir, GetGripPointsFlags bitFlags)
        {
            Debug.Print("BreakLineGripPointsOverrule");
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

                    if (toRemove != null)
                    {
                        grips.Remove(toRemove);
                    }

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
                if (exception.ErrorStatus != ErrorStatus.NotAllowedForThisProxy)
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
                                        tmpInsertionPoint = new Point3d(
                                            gripPoint.BreakLine.EndPoint.X + (gripPoint.BreakLine.BreakLineMinLength * scale * gripPoint.BreakLine.BlockTransform.GetScale()),
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
                                        ((BlockReference)entity).Position.X + (gripPoint.BreakLine.BreakLineMinLength * scale * gripPoint.BreakLine.BlockTransform.GetScale()),
                                        ((BlockReference)entity).Position.Y, ((BlockReference)entity).Position.Z);
                                }

                                // С конечной точкой все просто
                                else
                                {
                                    gripPoint.BreakLine.EndPoint = gripPoint.GripPoint + offset;
                                }
                            }

                            // Вот тут происходит перерисовка примитивов внутри блока
                            gripPoint.BreakLine.UpdateEntities();
                            gripPoint.BreakLine.BlockRecord.UpdateAnonymousBlocks();
                        }
                        else
                        {
                            base.MoveGripPointsAt(entity, grips, offset, bitFlags);
                        }
                    }
                }
                else
                {
                    base.MoveGripPointsAt(entity, grips, offset, bitFlags);
                }
            }
            catch (Exception exception)
            {
                if (exception.ErrorStatus != ErrorStatus.NotAllowedForThisProxy)
                    ExceptionBox.Show(exception);
            }
        }

        // Проверка поддерживаемости примитива
        // Проверка происходит по наличию XData с определенным AppName
        public override bool IsApplicable(RXObject overruledSubject)
        {
            return ExtendedDataHelpers.IsApplicable(overruledSubject, BreakLineDescriptor.Instance.Name);
        }
    }
}