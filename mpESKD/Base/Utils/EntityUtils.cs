namespace mpESKD.Base.Utils
{
    using System;
    using System.Windows;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using ModPlusAPI.Windows;

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

                if ((dbObject != null && dbObject.IsNewObject & dbObject.Database == AcadUtils.Database) ||
                    (dbObject != null && dbObject.IsUndoing & dbObject.IsModifiedXData))
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
    }
}
