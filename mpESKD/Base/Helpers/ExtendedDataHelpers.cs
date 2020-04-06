﻿namespace mpESKD.Base.Helpers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Runtime;

    /// <summary>Вспомогательные методы работы с расширенными данными
    /// Есть аналогичные в MpCadHelpers. Некоторые будут совпадать
    /// но все-равно делаю отдельно</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class ExtendedDataHelpers
    {
        /// <summary>
        /// Добавление регистрации приложения в соответствующую таблицу чертежа
        /// </summary>
        public static void AddRegAppTableRecord(string appName)
        {
            using (var tr = AcadHelpers.Document.TransactionManager.StartTransaction())
            {
                var rat =
                    (RegAppTable)tr.GetObject(AcadHelpers.Database.RegAppTableId, OpenMode.ForRead, false);
                if (!rat.Has(appName))
                {
                    rat.UpgradeOpen();
                    var regAppTableRecord = new RegAppTableRecord
                    {
                        Name = appName
                    };
                    rat.Add(regAppTableRecord);
                    tr.AddNewlyCreatedDBObject(regAppTableRecord, true);
                }

                tr.Commit();
            }
        }

        /// <summary>
        /// Проверка поддерживаемости примитива для Overrule
        /// </summary>
        /// <param name="rxObject"></param>
        /// <param name="appName"></param>
        /// <param name="checkIsNullId">comment #16 - http://adn-cis.org/forum/index.php?topic=8910.15 </param>
        public static bool IsApplicable(RXObject rxObject, string appName, bool checkIsNullId = false)
        {
            var dbObject = rxObject as DBObject;
            if (dbObject == null)
            {
                return false;
            }

            if (checkIsNullId)
            {
                if (dbObject.ObjectId == ObjectId.Null)
                {
                    return false;
                }
            }

            if (dbObject is BlockReference)
            {
                // Всегда нужно проверять по наличию расширенных данных
                // иначе может привести к фаталам при работе с динамическими блоками
                return IsIntellectualEntity(dbObject, appName);
            }

            return false;
        }

        /// <summary>
        /// Проверка поддерживаемости вставки блока путем проверки наличия XData с поддерживаемым кодом 1001
        /// </summary>
        public static bool IsApplicable(BlockReference blockReference)
        {
            if (blockReference.XData == null)
            {
                return false;
            }

            var applicableCommands = TypeFactory.Instance.GetEntityCommandNames();
            var typedValue = blockReference.XData.AsArray()
                .FirstOrDefault(tv => tv.TypeCode == (int)DxfCode.ExtendedDataRegAppName && applicableCommands.Contains(tv.Value.ToString()));
            return typedValue.Value != null;
        }

        /// <summary>
        /// Проверка по XData вхождения блока, что он является любым ЕСКД примитивом
        /// </summary>
        /// <param name="blkRef">Вхождение блока</param>
        /// <param name="appName"></param>
        /// <returns></returns>
        public static bool IsIntellectualEntity(Entity blkRef, string appName)
        {
            var rb = blkRef.GetXDataForApplication(appName);
            return rb != null;
        }

        public static bool IsIntellectualEntity(DBObject dbObject, string appName)
        {
            var rb = dbObject.GetXDataForApplication(appName);
            return rb != null;
        }
    }
}