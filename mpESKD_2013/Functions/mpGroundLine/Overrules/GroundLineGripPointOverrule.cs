// ReSharper disable InconsistentNaming
namespace mpESKD.Functions.mpGroundLine.Overrules
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Base.Helpers;
    using Base.Overrules;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using Exception = System.Exception;

    public class GroundLineGripPointOverrule : GripOverrule
    {
        protected static GroundLineGripPointOverrule _groundLineGripPointOverrule;

        public static GroundLineGripPointOverrule Instance()
        {
            if (_groundLineGripPointOverrule != null) return _groundLineGripPointOverrule;
            _groundLineGripPointOverrule = new GroundLineGripPointOverrule();
            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _groundLineGripPointOverrule.SetXDataFilter(GroundLineFunction.MPCOEntName);
            return _groundLineGripPointOverrule;
        }

        public override void GetGripPoints(Entity entity, GripDataCollection grips, double curViewUnitSize, int gripSize, Vector3d curViewDir,
            GetGripPointsFlags bitFlags)
        {
            try
            {
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
                    var groundLine = GroundLine.GetGroundLineFromEntity(entity);
                    if (groundLine != null)
                    {
                        // insertion (start) grip
                        var gp = new GroundLineVertexGrip(groundLine, 0)
                        {
                            GripType = MPCOGrips.MPCOEntityGripType.Point,
                            GripPoint = groundLine.InsertionPoint
                        };
                        grips.Add(gp);

                        // middle points
                        for (var index = 0; index < groundLine.MiddlePoints.Count; index++)
                        {
                            gp = new GroundLineVertexGrip(groundLine, index + 1)
                            {
                                GripType = MPCOGrips.MPCOEntityGripType.Point,
                                GripPoint = groundLine.MiddlePoints[index]
                            };
                            grips.Add(gp);
                        }

                        // end point
                        gp = new GroundLineVertexGrip(groundLine, groundLine.MiddlePoints.Count + 1)
                        {
                            GripType = MPCOGrips.MPCOEntityGripType.Point,
                            GripPoint = groundLine.EndPoint
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

        public override void MoveGripPointsAt(Entity entity, GripDataCollection grips, Vector3d offset, MoveGripPointsFlags bitFlags)
        {
            try
            {
                if (IsApplicable(entity))
                {
                    foreach (GripData gripData in grips)
                    {
                        if (gripData is GroundLineVertexGrip vertexGrip)
                        {
                            /*
                             * Если индекс ручки
                             */
                            if (vertexGrip.GripIndex == 0)
                            {
                                ((BlockReference)entity).Position = vertexGrip.GripPoint + offset;
                                vertexGrip.GroundLine.InsertionPoint = vertexGrip.GripPoint + offset;
                            }
                            else if (vertexGrip.GripIndex == vertexGrip.GroundLine.MiddlePoints.Count + 1)
                            {
                                vertexGrip.GroundLine.EndPoint = vertexGrip.GripPoint + offset;
                            }
                            else
                            {
                                vertexGrip.GroundLine.MiddlePoints[vertexGrip.GripIndex - 1] =
                                    vertexGrip.GripPoint + offset;
                            }

                            // Вот тут происходит перерисовка примитивов внутри блока
                            vertexGrip.GroundLine.UpdateEntities();
                            vertexGrip.GroundLine.BlockRecord.UpdateAnonymousBlocks();
                        }
                        ////else if ()
                        ////{

                        ////}
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
            return ExtendedDataHelpers.IsApplicable(overruledSubject, GroundLineFunction.MPCOEntName);
        }
    }

    /// <summary>
    /// Ручка вершин
    /// </summary>
    public class GroundLineVertexGrip : MPCOGrips.MPCOGripData //<-- Там будут определены типы точек и их ViewportDraw в зависимости от типа. Пока ничего этого нет
    {
        public GroundLineVertexGrip(GroundLine groundLine, int index)
        {
            GroundLine = groundLine;
            GripIndex = index;

            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;
        }

        /// <summary>
        /// Экземпляр класса GroundLine
        /// </summary>
        public GroundLine GroundLine { get; }

        /// <summary>
        /// Индекс точки
        /// </summary>
        public int GripIndex { get; }

        // Подсказка в зависимости от имени ручки
        public override string GetTooltip()
        {
            return Language.GetItem(MainFunction.LangItem, "gp1"); // stretch
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
                        var blkRef = tr.GetObject(GroundLine.BlockId, OpenMode.ForWrite);
                        using (var resBuf = GroundLine.GetParametersForXData())
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
                            GroundLine.InsertionPoint = _gripTmp;
                        else if (GripIndex == GroundLine.MiddlePoints.Count + 1)
                            GroundLine.EndPoint = _gripTmp;
                        else GroundLine.MiddlePoints[GripIndex - 1] = _gripTmp;
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
