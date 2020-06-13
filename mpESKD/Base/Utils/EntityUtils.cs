namespace mpESKD.Base.Utils
{
    using System;
    using System.Windows;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;

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
