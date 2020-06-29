﻿namespace mpESKD.Base.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using ModPlusAPI.Windows;
    using Overrules;
    using Overrules.Grips;

    /// <summary>
    /// Утилиты для объектов
    /// </summary>
    public static class EntityUtils
    {
        /// <summary>
        /// Установка свойств для однострочного текста
        /// </summary>
        /// <param name="dbText">Однострочный текст</param>
        /// <param name="textStyle">имя текстового стиля</param>
        /// <param name="height">Высота текста (с учетом масштаба блока)</param>
        /// <param name="horizontalMode">Выравнивание по горизонтали</param>
        /// <param name="verticalMode">Выравнивание по вертикали</param>
        /// <param name="attachmentPoint">Привязка к точке вставки</param>
        public static void SetPropertiesToDbText(
            this DBText dbText,
            string textStyle,
            double height,
            TextHorizontalMode? horizontalMode = null,
            TextVerticalMode? verticalMode = null,
            AttachmentPoint? attachmentPoint = null)
        {
            dbText.Height = height;
            if (horizontalMode.HasValue)
                dbText.HorizontalMode = horizontalMode.Value;
            if (verticalMode.HasValue)
                dbText.VerticalMode = verticalMode.Value;
            if (attachmentPoint.HasValue)
                dbText.Justify = attachmentPoint.Value;
            dbText.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
            dbText.Linetype = "ByBlock";
            dbText.LineWeight = LineWeight.ByBlock;
            dbText.TextStyleId = AcadUtils.GetTextStyleIdByName(textStyle);
        }

        /// <summary>
        /// Обработка объекта в методе Close класса <see cref="ObjectOverrule"/>
        /// </summary>
        /// <param name="dbObject">Instance of <see cref="DBObject"/></param>
        /// <param name="intellectualEntity">Метод получения объекта из блока</param>
        public static void ObjectOverruleProcess(DBObject dbObject, Func<IntellectualEntity> intellectualEntity)
        {
            try
            {
                if (AcadUtils.Document == null)
                    return;

                if (dbObject != null && dbObject.IsNewObject & dbObject.Database == AcadUtils.Database ||
                    dbObject != null && dbObject.IsUndoing & dbObject.IsModifiedXData)
                {
                    var entity = intellectualEntity.Invoke();
                    if (entity == null)
                        return;

                    entity.UpdateEntities();
                    entity.GetBlockTableRecordForUndo((BlockReference)dbObject).UpdateAnonymousBlocks();
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        /// <summary>
        /// Обработка объекта в методе GetObjectSnapPoints класса <see cref="OsnapOverrule"/>
        /// </summary>
        /// <param name="entity">Instance of <see cref="Entity"/></param>
        /// <param name="snapPoints">Коллекция точек для привязки</param>
        public static void OsnapOverruleProcess(Entity entity, Point3dCollection snapPoints)
        {
            try
            {
                var intellectualEntity = EntityReaderService.Instance.GetFromEntity(entity);
                if (intellectualEntity != null)
                {
                    foreach (var point3d in intellectualEntity.GetPointsForOsnap())
                    {
                        snapPoints.Add(point3d);
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        /// <summary>
        /// Редактирование свойств для интеллектуального объекта в специальном окне. Применяется для интеллектуальных
        /// объектов, содержащих текстовые значения
        /// </summary>
        /// <param name="blockReference">Блок, представляющий интеллектуальный объект</param>
        /// <param name="getEditor">Метод получения редактора свойств для интеллектуального объекта</param>
        public static void DoubleClickEdit(
            BlockReference blockReference,
            Func<IntellectualEntity, Window> getEditor)
        {
            BeditCommandWatcher.UseBedit = false;

            var intellectualEntity = EntityReaderService.Instance.GetFromEntity(blockReference);
            if (intellectualEntity != null)
            {
                intellectualEntity.UpdateEntities();
                var saveBack = false;

                var sectionValueEditor = getEditor(intellectualEntity);
                if (sectionValueEditor.ShowDialog() == true)
                {
                    saveBack = true;
                }

                if (saveBack)
                {
                    intellectualEntity.UpdateEntities();
                    intellectualEntity.BlockRecord.UpdateAnonymousBlocks();
                    using (var resBuf = intellectualEntity.GetDataForXData())
                    {
                        blockReference.XData = resBuf;
                    }
                }

                intellectualEntity.Dispose();
            }
        }

        /// <summary>
        /// Возвращает стандартные ручки для линейного интеллектуального объекта:
        /// ручки вершин, добавить вершину, удалить вершину, реверс объекта
        /// </summary>
        /// <param name="linearEntity">Линейный интеллектуальный объекты</param>
        /// <param name="curViewUnitSize">Размер единиц текущего вида</param>
        public static IEnumerable<IntellectualEntityGripData> GetLinearEntityGeneralGrips(
            ILinearEntity linearEntity, double curViewUnitSize)
        {
            var intellectualEntity = (IntellectualEntity)linearEntity;

            // Если средних точек нет, значит линия состоит всего из двух точек
            // в этом случае не нужно добавлять точки удаления крайних вершин

            // insertion (start) grip
            var vertexGrip = new LinearEntityVertexGrip(intellectualEntity, 0)
            {
                GripPoint = linearEntity.InsertionPoint
            };
            yield return vertexGrip;

            if (linearEntity.MiddlePoints.Any())
            {
                var removeVertexGrip = new LinearEntityRemoveVertexGrip(intellectualEntity, 0)
                {
                    GripPoint = linearEntity.InsertionPoint - (Vector3d.YAxis * 20 * curViewUnitSize)
                };
                yield return removeVertexGrip;
            }

            // middle points
            for (var index = 0; index < linearEntity.MiddlePoints.Count; index++)
            {
                vertexGrip = new LinearEntityVertexGrip(intellectualEntity, index + 1)
                {
                    GripPoint = linearEntity.MiddlePoints[index]
                };
                yield return vertexGrip;

                var removeVertexGrip = new LinearEntityRemoveVertexGrip(intellectualEntity, index + 1)
                {
                    GripPoint = linearEntity.MiddlePoints[index] - (Vector3d.YAxis * 20 * curViewUnitSize)
                };
                yield return removeVertexGrip;
            }

            // end point
            vertexGrip = new LinearEntityVertexGrip(intellectualEntity, linearEntity.MiddlePoints.Count + 1)
            {
                GripPoint = linearEntity.EndPoint
            };
            yield return vertexGrip;

            if (linearEntity.MiddlePoints.Any())
            {
                var removeVertexGrip = new LinearEntityRemoveVertexGrip(intellectualEntity, linearEntity.MiddlePoints.Count + 1)
                {
                    GripPoint = linearEntity.EndPoint - (Vector3d.YAxis * 20 * curViewUnitSize)
                };
                yield return removeVertexGrip;
            }

            #region AddVertex grips

            // add vertex grips
            for (var i = 0; i < linearEntity.MiddlePoints.Count; i++)
            {
                if (i == 0)
                {
                    var addVertexGrip = new LinearEntityAddVertexGrip(
                        intellectualEntity,
                        linearEntity.InsertionPoint, linearEntity.MiddlePoints[i])
                    {
                        GripPoint = GeometryUtils.GetMiddlePoint3d(linearEntity.InsertionPoint, linearEntity.MiddlePoints[i])
                    };
                    yield return addVertexGrip;
                }
                else
                {
                    var addVertexGrip = new LinearEntityAddVertexGrip(
                        intellectualEntity,
                        linearEntity.MiddlePoints[i - 1], linearEntity.MiddlePoints[i])
                    {
                        GripPoint = GeometryUtils.GetMiddlePoint3d(linearEntity.MiddlePoints[i - 1], linearEntity.MiddlePoints[i])
                    };
                    yield return addVertexGrip;
                }

                // last segment
                if (i == linearEntity.MiddlePoints.Count - 1)
                {
                    var addVertexGrip = new LinearEntityAddVertexGrip(
                        intellectualEntity,
                        linearEntity.MiddlePoints[i], linearEntity.EndPoint)
                    {
                        GripPoint = GeometryUtils.GetMiddlePoint3d(linearEntity.MiddlePoints[i], linearEntity.EndPoint)
                    };
                    yield return addVertexGrip;
                }
            }

            {
                if (linearEntity.MiddlePoints.Any())
                {
                    var addVertexGrip = new LinearEntityAddVertexGrip(intellectualEntity, linearEntity.EndPoint, null)
                    {
                        GripPoint = linearEntity.EndPoint +
                                    ((linearEntity.EndPoint - linearEntity.MiddlePoints.Last()).GetNormal() * 20 * curViewUnitSize)
                    };
                    yield return addVertexGrip;

                    addVertexGrip = new LinearEntityAddVertexGrip(intellectualEntity, null, linearEntity.InsertionPoint)
                    {
                        GripPoint = linearEntity.InsertionPoint +
                                    ((linearEntity.InsertionPoint - linearEntity.MiddlePoints.First()).GetNormal() * 20 * curViewUnitSize)
                    };
                    yield return addVertexGrip;
                }
                else
                {
                    var addVertexGrip = new LinearEntityAddVertexGrip(intellectualEntity, linearEntity.EndPoint, null)
                    {
                        GripPoint = linearEntity.EndPoint +
                                    ((linearEntity.InsertionPoint - linearEntity.EndPoint).GetNormal() * 20 * curViewUnitSize)
                    };
                    yield return addVertexGrip;

                    addVertexGrip = new LinearEntityAddVertexGrip(intellectualEntity, null, linearEntity.EndPoint)
                    {
                        GripPoint = linearEntity.InsertionPoint +
                                    ((linearEntity.EndPoint - linearEntity.InsertionPoint).GetNormal() * 20 * curViewUnitSize)
                    };
                    yield return addVertexGrip;

                    addVertexGrip = new LinearEntityAddVertexGrip(intellectualEntity, linearEntity.InsertionPoint, linearEntity.EndPoint)
                    {
                        GripPoint = GeometryUtils.GetMiddlePoint3d(linearEntity.InsertionPoint, linearEntity.EndPoint)
                    };
                    yield return addVertexGrip;
                }
            }

            #endregion

            var reverseGrip = new LinearEntityReverseGrip(intellectualEntity)
            {
                GripPoint = linearEntity.InsertionPoint + (Vector3d.YAxis * 20 * curViewUnitSize)
            };
            yield return reverseGrip;

            reverseGrip = new LinearEntityReverseGrip(intellectualEntity)
            {
                GripPoint = linearEntity.EndPoint + (Vector3d.YAxis * 20 * curViewUnitSize)
            };
            yield return reverseGrip;
        }

        /// <summary>
        /// Обработка ручек в методе MoveGripPointsAt класса <see cref="GripOverrule"/> для линейных интеллектуальных объектов
        /// </summary>
        /// <param name="entity">Примитив AutoCAD</param>
        /// <param name="grips">Коллекция ручек</param>
        /// <param name="offset">Смещение ручки</param>
        /// <param name="baseAction">Базовое действие метода MoveGripPointsAt для ручки</param>
        public static void LinearEntityGripPointMoveProcess(
            Entity entity, GripDataCollection grips, Vector3d offset, Action baseAction)
        {
            foreach (var gripData in grips)
            {
                if (gripData is LinearEntityVertexGrip vertexGrip)
                {
                    var intellectualEntity = vertexGrip.IntellectualEntity;

                    if (vertexGrip.GripIndex == 0)
                    {
                        ((BlockReference)entity).Position = vertexGrip.GripPoint + offset;
                        intellectualEntity.InsertionPoint = vertexGrip.GripPoint + offset;
                    }
                    else if (vertexGrip.GripIndex == ((ILinearEntity)intellectualEntity).MiddlePoints.Count + 1)
                    {
                        intellectualEntity.EndPoint = vertexGrip.GripPoint + offset;
                    }
                    else
                    {
                        ((ILinearEntity)intellectualEntity).MiddlePoints[vertexGrip.GripIndex - 1] =
                            vertexGrip.GripPoint + offset;
                    }

                    // Вот тут происходит перерисовка примитивов внутри блока
                    intellectualEntity.UpdateEntities();
                    intellectualEntity.BlockRecord.UpdateAnonymousBlocks();
                }
                else if (gripData is LinearEntityAddVertexGrip addVertexGrip)
                {
                    addVertexGrip.NewPoint = addVertexGrip.GripPoint + offset;
                }
                else
                {
                    baseAction.Invoke();
                }
            }
        }
    }
}
