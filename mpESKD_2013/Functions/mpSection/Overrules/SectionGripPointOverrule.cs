namespace mpESKD.Functions.mpSection.Overrules
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.GraphicsInterface;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using Base.Overrules;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    // ReSharper disable once RedundantNameQualifier
    using Section = mpSection.Section;

    public class SectionGripPointOverrule : GripOverrule
    {
        private static SectionGripPointOverrule _instance;

        public static SectionGripPointOverrule Instance()
        {
            if (_instance != null) return _instance;
            _instance = new SectionGripPointOverrule();
            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _instance.SetXDataFilter(SectionDescriptor.Instance.Name);
            return _instance;
        }

        public override void GetGripPoints(Entity entity, GripDataCollection grips, double curViewUnitSize, int gripSize, Vector3d curViewDir, GetGripPointsFlags bitFlags)
        {
            try
            {
                if (IsApplicable(entity))
                {
                    // Удаляю все ручки - это удалит ручку вставки блока
                    grips.Clear();

                    var section = EntityReaderFactory.Instance.GetFromEntity<Section>(entity);
                    if (section != null)
                    {
                        // insertion (start) grip
                        var vertexGrip = new SectionVertexGrip(section, 0)
                        {
                            GripPoint = section.InsertionPoint
                        };
                        grips.Add(vertexGrip);

                        // middle points
                        for (var index = 0; index < section.MiddlePoints.Count; index++)
                        {
                            vertexGrip = new SectionVertexGrip(section, index + 1)
                            {
                                GripPoint = section.MiddlePoints[index]
                            };
                            grips.Add(vertexGrip);

                            var removeVertexGrip = new SectionRemoveVertexGrip(section, index + 1)
                            {
                                GripPoint = section.MiddlePoints[index] - Vector3d.YAxis * 20 * curViewUnitSize
                            };
                            grips.Add(removeVertexGrip);
                        }

                        // end point
                        vertexGrip = new SectionVertexGrip(section, section.MiddlePoints.Count + 1)
                        {
                            GripPoint = section.EndPoint
                        };
                        grips.Add(vertexGrip);

                        #region AddVertex grips

                        // add vertex grips
                        for (var i = 0; i < section.MiddlePoints.Count; i++)
                        {
                            if (i == 0)
                            {
                                var addVertexGrip = new SectionAddVertexGrip(section,
                                    section.InsertionPoint, section.MiddlePoints[i])
                                {
                                    GripPoint = GeometryHelpers.GetMiddlePoint3d(section.InsertionPoint, section.MiddlePoints[i])
                                };
                                grips.Add(addVertexGrip);
                            }
                            else
                            {
                                var addVertexGrip = new SectionAddVertexGrip(section,
                                    section.MiddlePoints[i - 1], section.MiddlePoints[i])
                                {
                                    GripPoint = GeometryHelpers.GetMiddlePoint3d(section.MiddlePoints[i - 1], section.MiddlePoints[i])
                                };
                                grips.Add(addVertexGrip);
                            }
                            // last segment
                            if (i == section.MiddlePoints.Count - 1)
                            {
                                var addVertexGrip = new SectionAddVertexGrip(section,
                                    section.MiddlePoints[i], section.EndPoint)
                                {
                                    GripPoint = GeometryHelpers.GetMiddlePoint3d(section.MiddlePoints[i], section.EndPoint)
                                };
                                grips.Add(addVertexGrip);
                            }
                        }

                        if (!section.MiddlePoints.Any())
                        {
                            var addVertexGrip = new SectionAddVertexGrip(section, section.InsertionPoint, section.EndPoint)
                            {
                                GripPoint = GeometryHelpers.GetMiddlePoint3d(section.InsertionPoint, section.EndPoint)
                            };
                            grips.Add(addVertexGrip);
                        }

                        #endregion

                        #region Reverse Grips


                        var reverseGrip = new SectionReverseGrip(section)
                        {
                            GripPoint = section.EntityDirection == EntityDirection.LeftToRight
                                ? section.TopShelfEndPoint - Vector3d.XAxis * 20 * curViewUnitSize
                                : section.TopShelfEndPoint + Vector3d.XAxis * 20 * curViewUnitSize
                        };
                        grips.Add(reverseGrip);
                        reverseGrip = new SectionReverseGrip(section)
                        {
                            GripPoint = section.EntityDirection == EntityDirection.LeftToRight
                                ? section.BottomShelfEndPoint - Vector3d.XAxis * 20 * curViewUnitSize
                                : section.BottomShelfEndPoint + Vector3d.XAxis * 20 * curViewUnitSize
                        };
                        grips.Add(reverseGrip);

                        #endregion

                        #region Text grips

                        if (section.TopDesignationPoint != Point3d.Origin && section.HasTextValue())
                        {
                            var textGrip = new SectionTextGrip(section)
                            {
                                GripPoint = section.TopDesignationPoint,
                                Name = TextGripName.TopText
                            };
                            grips.Add(textGrip);
                        }

                        if (section.BottomDesignationPoint != Point3d.Origin && section.HasTextValue())
                        {
                            var textGrip = new SectionTextGrip(section)
                            {
                                GripPoint = section.BottomDesignationPoint,
                                Name = TextGripName.BottomText
                            };
                            grips.Add(textGrip);
                        }

                        #endregion
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
                        if (gripData is SectionVertexGrip vertexGrip)
                        {
                            if (vertexGrip.GripIndex == 0)
                            {
                                ((BlockReference)entity).Position = vertexGrip.GripPoint + offset;
                                vertexGrip.Section.InsertionPoint = vertexGrip.GripPoint + offset;
                            }
                            else if (vertexGrip.GripIndex == vertexGrip.Section.MiddlePoints.Count + 1)
                            {
                                vertexGrip.Section.EndPoint = vertexGrip.GripPoint + offset;
                            }
                            else
                            {
                                vertexGrip.Section.MiddlePoints[vertexGrip.GripIndex - 1] =
                                    vertexGrip.GripPoint + offset;
                            }

                            // Вот тут происходит перерисовка примитивов внутри блока
                            vertexGrip.Section.UpdateEntities();
                            vertexGrip.Section.BlockRecord.UpdateAnonymousBlocks();
                        }
                        else if (gripData is SectionTextGrip textGrip)
                        {
                            var section = textGrip.Section;
                            if (textGrip.Name == TextGripName.TopText)
                            {
                                var topStrokeVector = section.MiddlePoints.Any()
                                    ? (section.InsertionPoint - section.MiddlePoints.First()).GetNormal()
                                    : (section.InsertionPoint - section.EndPoint).GetNormal();
                                var topShelfVector = topStrokeVector.GetPerpendicularVector().Negate();
                                var deltaY = topStrokeVector.DotProduct(offset) / section.BlockTransform.GetScale();
                                var deltaX = topShelfVector.DotProduct(offset) / section.BlockTransform.GetScale();
                                if (double.IsNaN(textGrip.CachedAlongTopShelfTextOffset))
                                    section.AlongTopShelfTextOffset = deltaX;
                                else
                                    section.AlongTopShelfTextOffset = textGrip.CachedAlongTopShelfTextOffset + deltaX;

                                if (double.IsNaN(textGrip.CachedAcrossTopShelfTextOffset))
                                    section.AcrossTopShelfTextOffset = deltaY;
                                else
                                    section.AcrossTopShelfTextOffset = textGrip.CachedAcrossTopShelfTextOffset + deltaY;

                                if (MainStaticSettings.Settings.SectionDependentTextMovement)
                                {
                                    if (double.IsNaN(textGrip.CachedAlongBottomShelfTextOffset))
                                        section.AlongBottomShelfTextOffset = deltaX;
                                    else section.AlongBottomShelfTextOffset = textGrip.CachedAlongBottomShelfTextOffset + deltaX;

                                    if (double.IsNaN(textGrip.CachedAcrossBottomShelfTextOffset))
                                        section.AcrossBottomShelfTextOffset = deltaY;
                                    else section.AcrossBottomShelfTextOffset = textGrip.CachedAcrossBottomShelfTextOffset + deltaY;
                                }
                            }

                            if (textGrip.Name == TextGripName.BottomText)
                            {
                                var bottomStrokeVector = section.MiddlePoints.Any()
                                    ? (section.EndPoint - section.MiddlePoints.Last()).GetNormal()
                                    : (section.EndPoint - section.InsertionPoint).GetNormal();
                                var bottomShelfVector = bottomStrokeVector.GetPerpendicularVector();
                                var deltaY = bottomStrokeVector.DotProduct(offset) / section.BlockTransform.GetScale();
                                var deltaX = bottomShelfVector.DotProduct(offset) / section.BlockTransform.GetScale();

                                if (double.IsNaN(textGrip.CachedAlongBottomShelfTextOffset))
                                    section.AlongBottomShelfTextOffset = deltaX;
                                else section.AlongBottomShelfTextOffset = textGrip.CachedAlongBottomShelfTextOffset + deltaX;

                                if (double.IsNaN(textGrip.CachedAcrossBottomShelfTextOffset))
                                    section.AcrossBottomShelfTextOffset = deltaY;
                                else section.AcrossBottomShelfTextOffset = textGrip.CachedAcrossBottomShelfTextOffset + deltaY;

                                if (MainStaticSettings.Settings.SectionDependentTextMovement)
                                {
                                    if (double.IsNaN(textGrip.CachedAlongTopShelfTextOffset))
                                        section.AlongTopShelfTextOffset = deltaX;
                                    else
                                        section.AlongTopShelfTextOffset = textGrip.CachedAlongTopShelfTextOffset + deltaX;

                                    if (double.IsNaN(textGrip.CachedAcrossTopShelfTextOffset))
                                        section.AcrossTopShelfTextOffset = deltaY;
                                    else
                                        section.AcrossTopShelfTextOffset = textGrip.CachedAcrossTopShelfTextOffset + deltaY;
                                }
                            }

                            section.UpdateEntities();
                            section.BlockRecord.UpdateAnonymousBlocks();
                        }
                        else if (gripData is SectionAddVertexGrip addVertexGrip)
                        {
                            addVertexGrip.NewPoint = addVertexGrip.GripPoint + offset;
                        }
                        else base.MoveGripPointsAt(entity, grips, offset, bitFlags);
                    }
                }
                else
                {
                    base.MoveGripPointsAt(entity, grips, offset, bitFlags);
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
            return ExtendedDataHelpers.IsApplicable(overruledSubject, SectionDescriptor.Instance.Name);
        }
    }

    /// <summary>
    /// Ручка вершин
    /// </summary>
    public class SectionVertexGrip : IntellectualEntityGripData
    {
        public SectionVertexGrip(Section section, int index)
        {
            Section = section;
            GripIndex = index;
            GripType = GripType.Point;

            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;

            /* При инициализации ручки нужно собрать все точки разреза и поместить их в поле _points.
             * Это создаст кэш точек разреза. Если в методе WorldDraw брать точки из самого разреза (Section),
             * то вспомогательные линии будут меняться при зуммировании. Это связано с тем, что в методе
             * MoveGripPointsAt происходит вызов метода UpdateEntities */
            _points = new List<Point3d> { Section.InsertionPoint };
            _points.AddRange(Section.MiddlePoints);
            _points.Add(Section.EndPoint);

        }

        private readonly List<Point3d> _points;

        /// <summary>
        /// Экземпляр класса Section
        /// </summary>
        public Section Section { get; }

        /// <summary>
        /// Индекс точки
        /// </summary>
        public int GripIndex { get; }

        public override string GetTooltip()
        {
            return Language.GetItem(Invariables.LangItem, "gp1"); // stretch
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
                // По этим данным я потом получаю экземпляр класса section
                if (newStatus == Status.GripEnd)
                {
                    using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var blkRef = tr.GetObject(Section.BlockId, OpenMode.ForWrite, true, true);
                        using (var resBuf = Section.GetDataForXData())
                        {
                            blkRef.XData = resBuf;
                        }
                        tr.Commit();
                    }
                    Section.Dispose();
                }

                // При отмене перемещения возвращаем временные значения
                if (newStatus == Status.GripAbort)
                {
                    if (_gripTmp != null)
                    {
                        if (GripIndex == 0)
                            Section.InsertionPoint = _gripTmp;
                        else if (GripIndex == Section.MiddlePoints.Count + 1)
                            Section.EndPoint = _gripTmp;
                        else Section.MiddlePoints[GripIndex - 1] = _gripTmp;
                    }
                }

                base.OnGripStatusChanged(entityId, newStatus);
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        public override bool WorldDraw(WorldDraw worldDraw, ObjectId entityId, DrawType type, Point3d? imageGripPoint, double dGripSize)
        {
            if (GripIndex > 0 && MainStaticSettings.Settings.SectionShowHelpLineOnSelection)
            {
                short backupColor = worldDraw.SubEntityTraits.Color;
                FillType backupFillType = worldDraw.SubEntityTraits.FillType;

                worldDraw.SubEntityTraits.FillType = FillType.FillAlways;
                worldDraw.SubEntityTraits.Color = 40;
                worldDraw.Geometry.WorldLine(_points[GripIndex - 1], _points[GripIndex]);
                // restore
                worldDraw.SubEntityTraits.Color = backupColor;
                worldDraw.SubEntityTraits.FillType = backupFillType;
            }
            return base.WorldDraw(worldDraw, entityId, type, imageGripPoint, dGripSize);
        }
    }

    /// <summary>
    /// Ручка позиции текста
    /// </summary>
    public class SectionTextGrip : IntellectualEntityGripData
    {
        public SectionTextGrip(Section section)
        {
            Section = section;
            GripType = GripType.Point;
            CachedAlongTopShelfTextOffset = section.AlongTopShelfTextOffset;
            CachedAcrossTopShelfTextOffset = section.AcrossTopShelfTextOffset;
            CachedAlongBottomShelfTextOffset = section.AlongBottomShelfTextOffset;
            CachedAcrossBottomShelfTextOffset = section.AcrossBottomShelfTextOffset;

            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;
        }

        /// <summary>
        /// Экземпляр класса Section
        /// </summary>
        public Section Section { get; }

        /// <summary>
        /// Имя ручки, чтобы определить от какого она текста
        /// </summary>
        public TextGripName Name { get; set; }

        public double CachedAlongTopShelfTextOffset { get; }

        public double CachedAcrossTopShelfTextOffset { get; }

        public double CachedAlongBottomShelfTextOffset { get; }

        public double CachedAcrossBottomShelfTextOffset { get; }

        public override string GetTooltip()
        {
            return Language.GetItem(Invariables.LangItem, "gp1"); // stretch
        }

        public override void OnGripStatusChanged(ObjectId entityId, Status newStatus)
        {
            try
            {
                // При удачном перемещении ручки записываем новые значения в расширенные данные
                // По этим данным я потом получаю экземпляр класса section
                if (newStatus == Status.GripEnd)
                {
                    using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var blkRef = tr.GetObject(Section.BlockId, OpenMode.ForWrite, true, true);
                        using (var resBuf = Section.GetDataForXData())
                        {
                            blkRef.XData = resBuf;
                        }
                        tr.Commit();
                    }
                    Section.Dispose();
                }

                // При отмене перемещения возвращаем временные значения
                if (newStatus == Status.GripAbort)
                {
                    if (Name == TextGripName.TopText)
                    {
                        Section.AlongTopShelfTextOffset = CachedAlongTopShelfTextOffset;
                        Section.AcrossTopShelfTextOffset = CachedAcrossTopShelfTextOffset;
                    }

                    if (Name == TextGripName.BottomText)
                    {
                        Section.AlongBottomShelfTextOffset = CachedAlongBottomShelfTextOffset;
                        Section.AcrossBottomShelfTextOffset = CachedAcrossBottomShelfTextOffset;
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

    public enum TextGripName
    {
        TopText,
        BottomText
    }

    /// <summary>
    /// Ручка добавления вершины
    /// </summary>
    public class SectionAddVertexGrip : IntellectualEntityGripData
    {
        public SectionAddVertexGrip(Section section, Point3d? leftPoint, Point3d? rightPoint)
        {
            Section = section;
            GripLeftPoint = leftPoint;
            GripRightPoint = rightPoint;
            GripType = GripType.Plus;
            RubberBandLineDisabled = true;

            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;
        }

        /// <summary>
        /// Экземпляр класса Section
        /// </summary>
        public Section Section { get; }

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
            return Language.GetItem(Invariables.LangItem, "gp4"); //  "Добавить вершину";
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
                using (Section)
                {
                    Point3d? newInsertionPoint = null;

                    if (GripLeftPoint == Section.InsertionPoint)
                    {
                        Section.MiddlePoints.Insert(0, NewPoint);
                    }
                    else if (GripLeftPoint == null)
                    {
                        Section.MiddlePoints.Insert(0, Section.InsertionPoint);
                        Section.InsertionPoint = NewPoint;
                        newInsertionPoint = NewPoint;
                    }
                    else if (GripRightPoint == null)
                    {
                        Section.MiddlePoints.Add(Section.EndPoint);
                        Section.EndPoint = NewPoint;
                    }
                    else
                    {
                        Section.MiddlePoints.Insert(Section.MiddlePoints.IndexOf(GripLeftPoint.Value) + 1, NewPoint);
                    }
                    Section.UpdateEntities();
                    Section.BlockRecord.UpdateAnonymousBlocks();
                    using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var blkRef = tr.GetObject(Section.BlockId, OpenMode.ForWrite, true, true);
                        if (newInsertionPoint.HasValue)
                            ((BlockReference)blkRef).Position = newInsertionPoint.Value;
                        using (var resBuf = Section.GetDataForXData())
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

    /// <summary>
    /// Ручка удаления вершины
    /// </summary>
    public class SectionRemoveVertexGrip : IntellectualEntityGripData
    {
        public SectionRemoveVertexGrip(Section section, int index)
        {
            Section = section;
            GripIndex = index;
            GripType = GripType.Minus;

            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;
        }

        /// <summary>
        /// Экземпляр класса Section
        /// </summary>
        public Section Section { get; }

        /// <summary>
        /// Индекс точки
        /// </summary>
        public int GripIndex { get; }

        // Подсказка в зависимости от имени ручки
        public override string GetTooltip()
        {
            return Language.GetItem(Invariables.LangItem, "gp3"); // "Удалить вершину";
        }

        public override ReturnValue OnHotGrip(ObjectId entityId, Context contextFlags)
        {
            using (Section)
            {
                Point3d? newInsertionPoint = null;

                if (GripIndex == 0)
                {
                    Section.InsertionPoint = Section.MiddlePoints[0];
                    newInsertionPoint = Section.MiddlePoints[0];
                    Section.MiddlePoints.RemoveAt(0);
                }
                else if (GripIndex == Section.MiddlePoints.Count + 1)
                {
                    Section.EndPoint = Section.MiddlePoints.Last();
                    Section.MiddlePoints.RemoveAt(Section.MiddlePoints.Count - 1);
                }
                else
                {
                    Section.MiddlePoints.RemoveAt(GripIndex - 1);
                }

                Section.UpdateEntities();
                Section.BlockRecord.UpdateAnonymousBlocks();
                using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    var blkRef = tr.GetObject(Section.BlockId, OpenMode.ForWrite, true, true);
                    if (newInsertionPoint.HasValue)
                        ((BlockReference)blkRef).Position = newInsertionPoint.Value;
                    using (var resBuf = Section.GetDataForXData())
                    {
                        blkRef.XData = resBuf;
                    }

                    tr.Commit();
                }
            }

            return ReturnValue.GetNewGripPoints;
        }
    }

    /// <summary>
    /// Ручка реверса разреза
    /// </summary>
    public class SectionReverseGrip : IntellectualEntityGripData
    {
        public SectionReverseGrip(Section section)
        {
            Section = section;
            GripType = GripType.Mirror;

            // отключение контекстного меню и возможности менять команду
            // http://help.autodesk.com/view/OARX/2018/ENU/?guid=OREF-AcDbGripData__disableModeKeywords_bool
            ModeKeywordsDisabled = true;
        }

        /// <summary>
        /// Экземпляр класса Section
        /// </summary>
        public Section Section { get; }

        public override ReturnValue OnHotGrip(ObjectId entityId, Context contextFlags)
        {
            using (Section)
            {
                Point3d newInsertionPoint = Section.EndPoint;
                Section.EndPoint = Section.InsertionPoint;
                Section.InsertionPoint = newInsertionPoint;
                Section.MiddlePoints.Reverse();

                // swap direction
                Section.EntityDirection = Section.EntityDirection == EntityDirection.LeftToRight
                    ? EntityDirection.RightToLeft
                    : EntityDirection.LeftToRight;
                Section.BlockTransform = Section.BlockTransform.Inverse();

                // swap text offsets
                var tmp = Section.AcrossBottomShelfTextOffset;
                Section.AcrossBottomShelfTextOffset = Section.AcrossTopShelfTextOffset;
                Section.AcrossTopShelfTextOffset = tmp;
                tmp = Section.AlongBottomShelfTextOffset;
                Section.AlongBottomShelfTextOffset = Section.AlongTopShelfTextOffset;
                Section.AlongTopShelfTextOffset = tmp;

                Section.UpdateEntities();
                Section.BlockRecord.UpdateAnonymousBlocks();
                using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    var blkRef = tr.GetObject(Section.BlockId, OpenMode.ForWrite, true, true);
                    ((BlockReference)blkRef).Position = newInsertionPoint;
                    using (var resBuf = Section.GetDataForXData())
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
