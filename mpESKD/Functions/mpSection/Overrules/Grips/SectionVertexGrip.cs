namespace mpESKD.Functions.mpSection.Overrules.Grips
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.GraphicsInterface;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using Base.Overrules;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using Section = mpSection.Section;

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
                        {
                            Section.InsertionPoint = _gripTmp;
                        }
                        else if (GripIndex == Section.MiddlePoints.Count + 1)
                        {
                            Section.EndPoint = _gripTmp;
                        }
                        else
                        {
                            Section.MiddlePoints[GripIndex - 1] = _gripTmp;
                        }
                    }
                }

                base.OnGripStatusChanged(entityId, newStatus);
            }
            catch (Exception exception)
            {
                if (exception.ErrorStatus != ErrorStatus.NotAllowedForThisProxy)
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
}