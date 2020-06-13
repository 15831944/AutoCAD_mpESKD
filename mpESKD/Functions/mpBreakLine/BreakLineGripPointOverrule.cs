﻿namespace mpESKD.Functions.mpBreakLine
{
    using System.Diagnostics;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Utils;
    using Grips;
    using ModPlusAPI.Windows;

    /// <inheritdoc />
    public class BreakLineGripPointOverrule : GripOverrule
    {
        private static BreakLineGripPointOverrule _breakLineGripPointOverrule;

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static BreakLineGripPointOverrule Instance()
        {
            if (_breakLineGripPointOverrule != null)
            {
                return _breakLineGripPointOverrule;
            }

            _breakLineGripPointOverrule = new BreakLineGripPointOverrule();

            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _breakLineGripPointOverrule.SetXDataFilter(BreakLineDescriptor.Instance.Name);
            return _breakLineGripPointOverrule;
        }

        /// <inheritdoc />
        public override void GetGripPoints(
            Entity entity, GripDataCollection grips, double curViewUnitSize, int gripSize, Vector3d curViewDir, GetGripPointsFlags bitFlags)
        {
            Debug.Print("BreakLineGripPointOverrule");
            try
            {
                // Проверка дополнительных условий
                if (IsApplicable(entity))
                {
                    // Чтобы "отключить" точку вставки блока, нужно получить сначала блок
                    // Т.к. мы точно знаем для какого примитива переопределение, то получаем блок:
                    var blkRef = (BlockReference)entity;

                    // Удаляем стандартную ручку позиции блока (точки вставки)
                    GripData toRemove = null;
                    foreach (var gd in grips)
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
                    var breakLine = EntityReaderService.Instance.GetFromEntity<BreakLine>(entity);

                    // Паранойя программиста =)
                    if (breakLine != null)
                    {
                        // Получаем первую ручку (совпадает с точкой вставки блока)
                        var gp = new BreakLineGrip(breakLine, BreakLineGripName.StartGrip)
                        {
                            GripPoint = breakLine.InsertionPoint
                        };
                        grips.Add(gp);

                        // получаем среднюю ручку
                        gp = new BreakLineGrip(breakLine, BreakLineGripName.MiddleGrip)
                        {
                            GripPoint = breakLine.MiddlePoint
                        };
                        grips.Add(gp);

                        // получаем конечную ручку
                        gp = new BreakLineGrip(breakLine, BreakLineGripName.EndGrip)
                        {
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

        /// <inheritdoc/>
        public override void MoveGripPointsAt(
            Entity entity, GripDataCollection grips, Vector3d offset, MoveGripPointsFlags bitFlags)
        {
            try
            {
                if (IsApplicable(entity))
                {
                    // Проходим по коллекции ручек
                    foreach (var gripData in grips)
                    {
                        if (gripData is BreakLineGrip gripPoint)
                        {
                            var breakLine = gripPoint.BreakLine;
                            var scale = breakLine.GetFullScale();

                            // Далее, в зависимости от имени ручки произвожу действия
                            if (gripPoint.GripName == BreakLineGripName.StartGrip)
                            {
                                // Переношу точку вставки блока, и точку, описывающую первую точку в примитиве
                                // Все точки всегда совпадают (+ ручка)
                                var newPt = gripPoint.GripPoint + offset;
                                var length = breakLine.EndPoint.DistanceTo(newPt);
                                
                                if (length < breakLine.MinDistanceBetweenPoints * scale)
                                {
                                    /* Если новая точка получается на расстоянии меньше минимального, то
                                     * переносим ее в направлении между двумя точками на минимальное расстояние
                                     */
                                    var tmpInsertionPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(
                                        breakLine.EndPoint, newPt, breakLine.EndPoint,
                                        breakLine.MinDistanceBetweenPoints * scale);

                                    if (breakLine.EndPoint.Equals(newPt))
                                    {
                                        // Если точки совпали, то задаем минимальное значение
                                        tmpInsertionPoint = new Point3d(
                                            breakLine.EndPoint.X + (breakLine.MinDistanceBetweenPoints * scale),
                                            breakLine.EndPoint.Y, breakLine.EndPoint.Z);
                                    }

                                    ((BlockReference)entity).Position = tmpInsertionPoint;
                                    breakLine.InsertionPoint = tmpInsertionPoint;
                                }
                                else
                                {
                                    ((BlockReference)entity).Position = gripPoint.GripPoint + offset;
                                    breakLine.InsertionPoint = gripPoint.GripPoint + offset;
                                }
                            }

                            if (gripPoint.GripName == BreakLineGripName.MiddleGrip)
                            {
                                // Т.к. средняя точка нужна для переноса примитива, но не соответствует точки вставки блока
                                // и получается как средняя точка между InsertionPoint и EndPoint, то я переношу
                                // точку вставки
                                var lengthVector = (breakLine.InsertionPoint - breakLine.EndPoint) / 2;
                                ((BlockReference)entity).Position = gripPoint.GripPoint + offset + lengthVector;
                            }

                            if (gripPoint.GripName == BreakLineGripName.EndGrip)
                            {
                                var newPt = gripPoint.GripPoint + offset;
                                if (newPt.Equals(((BlockReference)entity).Position))
                                {
                                    breakLine.EndPoint = new Point3d(
                                        ((BlockReference)entity).Position.X + (breakLine.MinDistanceBetweenPoints * scale),
                                        ((BlockReference)entity).Position.Y, ((BlockReference)entity).Position.Z);
                                }

                                // С конечной точкой все просто
                                else
                                {
                                    breakLine.EndPoint = gripPoint.GripPoint + offset;
                                }
                            }

                            // Вот тут происходит перерисовка примитивов внутри блока
                            breakLine.UpdateEntities();
                            breakLine.BlockRecord.UpdateAnonymousBlocks();
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

        /// <inheritdoc />
        public override bool IsApplicable(RXObject overruledSubject)
        {
            return ExtendedDataUtils.IsApplicable(overruledSubject, BreakLineDescriptor.Instance.Name);
        }
    }
}