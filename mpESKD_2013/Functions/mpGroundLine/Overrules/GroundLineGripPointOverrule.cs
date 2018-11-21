// ReSharper disable InconsistentNaming
namespace mpESKD.Functions.mpGroundLine.Overrules
{
    using System.Diagnostics;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using Base.Overrules;
    using ModPlusAPI;
    using ModPlusAPI.Windows;

    public class GroundLineGripPointOverrule : GripOverrule
    {
        protected static GroundLineGripPointOverrule _groundLineGripPointOverrule;

        public static GroundLineGripPointOverrule Instance()
        {
            if (_groundLineGripPointOverrule != null) return _groundLineGripPointOverrule;
            _groundLineGripPointOverrule = new GroundLineGripPointOverrule();
            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _groundLineGripPointOverrule.SetXDataFilter(GroundLineDescriptor.Instance.Name);
            return _groundLineGripPointOverrule;
        }

        public override void GetGripPoints(Entity entity, GripDataCollection grips, double curViewUnitSize, int gripSize, Vector3d curViewDir,
            GetGripPointsFlags bitFlags)
        {
            Debug.Print("GroundLineGripPointOverrule");
            try
            {
                if (IsApplicable(entity))
                {
                    // Удаляю все ручки - это удалит ручку вставки блока
                    grips.Clear();

                    // Получаем экземпляр класса, который описывает как должен выглядеть примитив
                    // т.е. правила построения графики внутри блока
                    // Информация собирается по XData и свойствам самого блока
                    
                    var groundLine = EntityReaderFactory.Instance.GetFromEntity<GroundLine>(entity);
                    if (groundLine != null)
                    {
                        Vector3d val = new Vector3d(entity.GetPlane(), Vector2d.XAxis);
                        var scale = val.Length;

                        // Если средних точек нет, значит линия состоит всего из двух точек
                        // в этом случае не нужно добавлять точки удаления крайних вершин

                        // insertion (start) grip
                        var vertexGrip = new GroundLineVertexGrip(groundLine, 0)
                        {
                            GripPoint = groundLine.InsertionPoint
                        };
                        grips.Add(vertexGrip);

                        if (groundLine.MiddlePoints.Any())
                        {
                            var removeVertexGrip = new GroundLineRemoveVertexGrip(groundLine, 0)
                            {
                                GripPoint = groundLine.InsertionPoint - Vector3d.YAxis * 4 * scale
                            };
                            grips.Add(removeVertexGrip);
                        }

                        // middle points
                        for (var index = 0; index < groundLine.MiddlePoints.Count; index++)
                        {
                            vertexGrip = new GroundLineVertexGrip(groundLine, index + 1)
                            {
                                GripPoint = groundLine.MiddlePoints[index]
                            };
                            grips.Add(vertexGrip);

                            var removeVertexGrip = new GroundLineRemoveVertexGrip(groundLine, index + 1)
                            {
                                GripPoint = groundLine.MiddlePoints[index] - Vector3d.YAxis * 4 * scale
                            };
                            grips.Add(removeVertexGrip);
                        }

                        // end point
                        vertexGrip = new GroundLineVertexGrip(groundLine, groundLine.MiddlePoints.Count + 1)
                        {
                            GripPoint = groundLine.EndPoint
                        };
                        grips.Add(vertexGrip);

                        if (groundLine.MiddlePoints.Any())
                        {
                            var removeVertexGrip = new GroundLineRemoveVertexGrip(groundLine, groundLine.MiddlePoints.Count + 1)
                            {
                                GripPoint = groundLine.EndPoint - Vector3d.YAxis * 4 * scale
                            };
                            grips.Add(removeVertexGrip);
                        }

                        #region AddVertex grips

                        // add vertex grips
                        for (var i = 0; i < groundLine.MiddlePoints.Count; i++)
                        {
                            if (i == 0)
                            {
                                var addVertexGrip = new GroundLineAddVertexGrip(groundLine,
                                    groundLine.InsertionPoint, groundLine.MiddlePoints[i])
                                {
                                    GripPoint = GeometryHelpers.GetMiddlePoint3d(groundLine.InsertionPoint, groundLine.MiddlePoints[i])
                                };
                                grips.Add(addVertexGrip);
                            }
                            else
                            {
                                var addVertexGrip = new GroundLineAddVertexGrip(groundLine,
                                    groundLine.MiddlePoints[i - 1], groundLine.MiddlePoints[i])
                                {
                                    GripPoint = GeometryHelpers.GetMiddlePoint3d(groundLine.MiddlePoints[i - 1], groundLine.MiddlePoints[i])
                                };
                                grips.Add(addVertexGrip);
                            }
                            // last segment
                            if (i == groundLine.MiddlePoints.Count - 1)
                            {
                                var addVertexGrip = new GroundLineAddVertexGrip(groundLine,
                                    groundLine.MiddlePoints[i], groundLine.EndPoint)
                                {
                                    GripPoint = GeometryHelpers.GetMiddlePoint3d(groundLine.MiddlePoints[i], groundLine.EndPoint)
                                };
                                grips.Add(addVertexGrip);
                            }
                        }

                        {
                            if (groundLine.MiddlePoints.Any())
                            {
                                var addVertexGrip = new GroundLineAddVertexGrip(groundLine, groundLine.EndPoint, null)
                                {
                                    GripPoint = groundLine.EndPoint +
                                                (groundLine.EndPoint - groundLine.MiddlePoints.Last()).GetNormal() * 4 * scale
                                };
                                grips.Add(addVertexGrip);

                                addVertexGrip = new GroundLineAddVertexGrip(groundLine, null, groundLine.InsertionPoint)
                                {
                                    GripPoint = groundLine.InsertionPoint +
                                                (groundLine.InsertionPoint - groundLine.MiddlePoints.First()).GetNormal() * 4 * scale
                                };
                                grips.Add(addVertexGrip);
                            }
                            else
                            {
                                var addVertexGrip = new GroundLineAddVertexGrip(groundLine, groundLine.EndPoint, null)
                                {
                                    GripPoint = groundLine.EndPoint + 
                                                (groundLine.InsertionPoint - groundLine.EndPoint).GetNormal() * 4 * scale
                                };
                                grips.Add(addVertexGrip);

                                addVertexGrip = new GroundLineAddVertexGrip(groundLine, null, groundLine.EndPoint)
                                {
                                    GripPoint = groundLine.InsertionPoint +
                                                (groundLine.EndPoint - groundLine.InsertionPoint ).GetNormal() * 4 * scale
                                };
                                grips.Add(addVertexGrip);

                                addVertexGrip = new GroundLineAddVertexGrip(groundLine, groundLine.InsertionPoint, groundLine.EndPoint)
                                {
                                    GripPoint = GeometryHelpers.GetMiddlePoint3d(groundLine.InsertionPoint, groundLine.EndPoint)
                                };
                                grips.Add(addVertexGrip);
                            }
                        }

                        #endregion

                        var reverseGrip = new GroundLineReverseGrip(groundLine);
                        if (groundLine.MiddlePoints.Any())
                        {
                            Point2dCollection points = new Point2dCollection();
                            points.Add(ModPlus.Helpers.GeometryHelpers.ConvertPoint3dToPoint2d(groundLine.InsertionPoint));
                            groundLine.MiddlePoints.ForEach(p => points.Add(ModPlus.Helpers.GeometryHelpers.ConvertPoint3dToPoint2d(p)));
                            points.Add(ModPlus.Helpers.GeometryHelpers.ConvertPoint3dToPoint2d(groundLine.EndPoint));
                            Polyline polyline = new Polyline();
                            for (var i = 0; i < points.Count; i++)
                            {
                                polyline.AddVertexAt(i, points[i], 0.0, 0.0, 0.0);
                            }

                            reverseGrip.GripPoint = polyline.GetPointAtDist(polyline.Length / 2) +
                                                    Vector3d.YAxis * 4 * scale;
                        }
                        else
                        {
                            reverseGrip.GripPoint =
                                GeometryHelpers.GetMiddlePoint3d(groundLine.InsertionPoint, groundLine.EndPoint) +
                                Vector3d.YAxis * 4 * scale;
                        }
                        grips.Add(reverseGrip);
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
                        else if (gripData is GroundLineAddVertexGrip addVertexGrip)
                        {
                            addVertexGrip.NewPoint = addVertexGrip.GripPoint + offset;
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
            return ExtendedDataHelpers.IsApplicable(overruledSubject, GroundLineDescriptor.Instance.Name);
        }
    }

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

    public class GroundLineAddVertexGrip : IntellectualEntityGripData
    {
        public GroundLineAddVertexGrip(GroundLine groundLine, Point3d? leftPoint, Point3d? rightPoint)
        {
            GroundLine = groundLine;
            GripLeftPoint = leftPoint;
            GripRightPoint = rightPoint;
            GripType = GripType.Plus;
            RubberBandLineDisabled = true;

            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;
        }

        /// <summary>
        /// Экземпляр класса GroundLine
        /// </summary>
        public GroundLine GroundLine { get; }

        /// <summary>
        /// Левая точка
        /// </summary>
        public Point3d? GripLeftPoint { get; }

        /// <summary>
        /// Правая точка
        /// </summary>
        public Point3d? GripRightPoint { get; }

        public Point3d NewPoint { get; set; }

        public override string GetTooltip()
        {
            return Language.GetItem(MainFunction.LangItem, "gp4"); //  "Добавить вершину";
        }

        public override void OnGripStatusChanged(ObjectId entityId, Status newStatus)
        {
            if (newStatus == Status.GripStart)
            {
                AcadHelpers.Editor.TurnForcedPickOn();
                AcadHelpers.Editor.PointMonitor += AddNewVertex_EdOnPointMonitor;
            }

            if (newStatus == Status.GripEnd)
            {
                AcadHelpers.Editor.TurnForcedPickOff();
                AcadHelpers.Editor.PointMonitor -= AddNewVertex_EdOnPointMonitor;
                using (GroundLine)
                {
                    Point3d? newInsertionPoint = null;

                    if (GripLeftPoint == GroundLine.InsertionPoint)
                    {
                        GroundLine.MiddlePoints.Insert(0, NewPoint);
                    }
                    else if (GripLeftPoint == null)
                    {
                        GroundLine.MiddlePoints.Insert(0, GroundLine.InsertionPoint);
                        GroundLine.InsertionPoint = NewPoint;
                        newInsertionPoint = NewPoint;
                    }
                    else if (GripRightPoint == null)
                    {
                        GroundLine.MiddlePoints.Add(GroundLine.EndPoint);
                        GroundLine.EndPoint = NewPoint;
                    }
                    else
                    {
                        GroundLine.MiddlePoints.Insert(GroundLine.MiddlePoints.IndexOf(GripLeftPoint.Value) + 1, NewPoint);
                    }
                    GroundLine.UpdateEntities();
                    GroundLine.BlockRecord.UpdateAnonymousBlocks();
                    using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var blkRef = tr.GetObject(GroundLine.BlockId, OpenMode.ForWrite);
                        if (newInsertionPoint.HasValue)
                            ((BlockReference)blkRef).Position = newInsertionPoint.Value;
                        using (var resBuf = GroundLine.GetDataForXData())
                        {
                            blkRef.XData = resBuf;
                        }

                        tr.Commit();
                    }
                }
            }

            if (newStatus == Status.GripAbort)
            {
                AcadHelpers.Editor.TurnForcedPickOff();
                AcadHelpers.Editor.PointMonitor -= AddNewVertex_EdOnPointMonitor;
            }

            base.OnGripStatusChanged(entityId, newStatus);
        }

        private void AddNewVertex_EdOnPointMonitor(object sender, PointMonitorEventArgs pointMonitorEventArgs)
        {
            try
            {
                if (GripLeftPoint.HasValue)
                {
                    Line leftLine = new Line(GripLeftPoint.Value, pointMonitorEventArgs.Context.ComputedPoint)
                    {
                        ColorIndex = 150
                    };
                    pointMonitorEventArgs.Context.DrawContext.Geometry.Draw(leftLine);
                }

                if (GripRightPoint.HasValue)
                {
                    Line rightLine = new Line(pointMonitorEventArgs.Context.ComputedPoint, GripRightPoint.Value)
                    {
                        ColorIndex = 150
                    };
                    pointMonitorEventArgs.Context.DrawContext.Geometry.Draw(rightLine);
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    public class GroundLineRemoveVertexGrip : IntellectualEntityGripData
    {
        public GroundLineRemoveVertexGrip(GroundLine groundLine, int index)
        {
            GroundLine = groundLine;
            GripIndex = index;
            GripType = GripType.Minus;

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
            return Language.GetItem(MainFunction.LangItem, "gp3"); // "Удалить вершину";
        }

        public override ReturnValue OnHotGrip(ObjectId entityId, Context contextFlags)
        {
            using (GroundLine)
            {
                Point3d? newInsertionPoint = null;

                if (GripIndex == 0)
                {
                    GroundLine.InsertionPoint = GroundLine.MiddlePoints[0];
                    newInsertionPoint = GroundLine.MiddlePoints[0];
                    GroundLine.MiddlePoints.RemoveAt(0);
                }
                else if (GripIndex == GroundLine.MiddlePoints.Count + 1)
                {
                    GroundLine.EndPoint = GroundLine.MiddlePoints.Last();
                    GroundLine.MiddlePoints.RemoveAt(GroundLine.MiddlePoints.Count - 1);
                }
                else
                {
                    GroundLine.MiddlePoints.RemoveAt(GripIndex - 1);
                }

                GroundLine.UpdateEntities();
                GroundLine.BlockRecord.UpdateAnonymousBlocks();
                using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    var blkRef = tr.GetObject(GroundLine.BlockId, OpenMode.ForWrite);
                    if (newInsertionPoint.HasValue)
                        ((BlockReference)blkRef).Position = newInsertionPoint.Value;
                    using (var resBuf = GroundLine.GetDataForXData())
                    {
                        blkRef.XData = resBuf;
                    }

                    tr.Commit();
                }
            }

            return ReturnValue.GetNewGripPoints;
        }
    }

    public class GroundLineReverseGrip : IntellectualEntityGripData
    {
        public GroundLineReverseGrip(GroundLine groundLine)
        {
            GroundLine = groundLine;
            GripType = GripType.Mirror;

            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;
        }

        /// <summary>
        /// Экземпляр класса GroundLine
        /// </summary>
        public GroundLine GroundLine { get; }

        public override ReturnValue OnHotGrip(ObjectId entityId, Context contextFlags)
        {
            using (GroundLine)
            {
                Point3d newInsertionPoint = GroundLine.EndPoint;
                GroundLine.EndPoint = GroundLine.InsertionPoint;
                GroundLine.InsertionPoint = newInsertionPoint;
                GroundLine.MiddlePoints.Reverse();
                GroundLine.BlockTransform = GroundLine.BlockTransform.Inverse();
                
                GroundLine.UpdateEntities();
                GroundLine.BlockRecord.UpdateAnonymousBlocks();
                using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    var blkRef = tr.GetObject(GroundLine.BlockId, OpenMode.ForWrite);
                    ((BlockReference)blkRef).Position = newInsertionPoint;
                    using (var resBuf = GroundLine.GetDataForXData())
                    {
                        blkRef.XData = resBuf;
                    }

                    tr.Commit();
                }
            }

            return ReturnValue.GetNewGripPoints;
        }
    }
}
