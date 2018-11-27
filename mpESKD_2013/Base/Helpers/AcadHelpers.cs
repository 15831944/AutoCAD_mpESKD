namespace mpESKD.Base.Helpers
{
    using System;
    using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml.Linq;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using ModPlusAPI;
    using ModPlusAPI.Windows;

    public static class AcadHelpers
    {
        /// <summary>БД активного документа</summary>
        public static Database Database => HostApplicationServices.WorkingDatabase;

        /// <summary>Коллекция документов</summary>
        public static DocumentCollection Documents => AcApp.DocumentManager;

        /// <summary>Активный документ</summary>
        public static Document Document => AcApp.DocumentManager.MdiActiveDocument;

        /// <summary>Редактор активного документа</summary>
        public static Editor Editor => AcApp.DocumentManager.MdiActiveDocument.Editor;

        public static ObjectContextCollection ObjectContextCollection
        {
            get
            {
                ObjectContextManager ocm = Database.ObjectContextManager;
                return ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");
            }
        }

        /// <summary>Список слоев текущей базы данных</summary>
        public static List<string> Layers
        {
            get
            {
                var layers = new List<string>();
                using (Document.LockDocument())
                {
                    using (OpenCloseTransaction tr = Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        LayerTable lt = tr.GetObject(Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                        if (lt != null)
                            foreach (ObjectId layerId in lt)
                            {
                                var layer = tr.GetObject(layerId, OpenMode.ForWrite) as LayerTableRecord;
                                if (layer != null)
                                    layers.Add(layer.Name);
                            }
                    }
                }
                return layers;
            }
        }

        /// <summary>Список масштабов текущего чертежа</summary>
        public static List<string> Scales
        {
            get
            {
                var scales = new List<string>();
                var ocm = Database.ObjectContextManager;
                if (ocm != null)
                {
                    var occ = ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");
                    foreach (ObjectContext objectContext in occ)
                    {
                        scales.Add(((AnnotationScale)objectContext).Name);
                    }
                }
                return scales;
            }
        }

        /// <summary>Текстовые стили текущего чертежа</summary>
        public static List<string> TextStyles
        {
            get
            {
                List<string> textStyles = new List<string>();
                using (Document.LockDocument())
                {
                    using (OpenCloseTransaction tr = Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var txtstbl = (TextStyleTable)tr.GetObject(Database.TextStyleTableId, OpenMode.ForRead);
                        foreach (ObjectId objectId in txtstbl)
                        {
                            var txtStl = (TextStyleTableRecord)tr.GetObject(objectId, OpenMode.ForRead);
                            if (!textStyles.Contains(txtStl.Name))
                                textStyles.Add(txtStl.Name);
                        }
                    }
                }
                return textStyles;
            }
        }

        /// <summary>
        /// Открыть объект для чтения
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectId"></param>
        /// <param name="openErased"></param>
        /// <param name="forceOpenOnLockedLayer"></param>
        /// <returns></returns>
        public static T Read<T>(this ObjectId objectId, bool openErased = false, bool forceOpenOnLockedLayer = true)
            where T : DBObject
        {
            return (T)(objectId.GetObject(0, openErased, forceOpenOnLockedLayer) as T);
        }

        /// <summary>
        /// Открыть объект для записи
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectId"></param>
        /// <param name="openErased"></param>
        /// <param name="forceOpenOnLockedLayer"></param>
        /// <returns></returns>
        public static T Write<T>(this ObjectId objectId, bool openErased = false, bool forceOpenOnLockedLayer = true)
            where T : DBObject
        {
            return (T)(objectId.GetObject(OpenMode.ForWrite, openErased, forceOpenOnLockedLayer) as T);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static BlockReference GetBlockReference(Point3d point, IEnumerable<Entity> entities)
        {
            BlockTableRecord blockTableRecord = new BlockTableRecord
            {
                Name = "*U"
            };
            foreach (Entity enity in entities)
            {
                blockTableRecord.AppendEntity(enity);
            }
            return new BlockReference(point, blockTableRecord.ObjectId);
        }

        /// <summary>Получение аннотативного масштаба по имени из текущего чертежа</summary>
        /// <param name="name">Имя масштаба</param>
        /// <returns>Аннотативный масштаб с таким именем или текущий масштаб в БД</returns>
        public static AnnotationScale GetAnnotationScaleByName(string name)
        {
            var ocm = Database.ObjectContextManager;
            if (ocm != null)
            {
                var occ = ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");
                if (occ != null)
                    foreach (var objectContext in occ)
                    {
                        var asc = objectContext as AnnotationScale;
                        if (asc?.Name == name) return asc;
                    }
            }
            return Database.Cannoscale;
        }

        /// <summary>
        /// Возвращает численный масштаб (отношение единиц чертежа к единицам листа)
        /// </summary>
        public static double GetNumericScale(this AnnotationScale scale)
        {
            return scale.DrawingUnits / scale.PaperUnits;
        }

        /// <summary>Применение стиля к блоку согласно стиля</summary>
        /// <param name="blkRefObjectId">ObjectId of block reference</param>
        /// <param name="layerName">Имя слоя</param>
        /// <param name="layerXmlData">Данные слоя</param>
        public static void SetLayerByName(ObjectId blkRefObjectId, string layerName, XElement layerXmlData)
        {
            if (blkRefObjectId == ObjectId.Null)
                return;
            if (!layerName.Equals(Language.GetItem(Invariables.LangItem, "defl"))) // "По умолчанию"
            {
                if (LayerHelper.HasLayer(layerName))
                {
                    using (Document.LockDocument())
                    {
                        using (Transaction tr = Database.TransactionManager.StartTransaction())
                        {
                            var blockReference = tr.GetObject(blkRefObjectId, OpenMode.ForWrite) as BlockReference;
                            if (blockReference != null) blockReference.Layer = layerName;
                            tr.Commit();
                        }
                    }
                }
                else
                {
                    if (MainStaticSettings.Settings.IfNoLayer == 1)
                    {
                        if (LayerHelper.AddLayerFromXelement(layerXmlData))
                            using (Document.LockDocument())
                            {
                                using (Transaction tr = Database.TransactionManager.StartTransaction())
                                {
                                    var blockReference =
                                        tr.GetObject(blkRefObjectId, OpenMode.ForWrite) as BlockReference;
                                    if (blockReference != null) blockReference.Layer = layerName;
                                    tr.Commit();
                                }
                            }
                    }
                }
            }
            else
            {
                if (Database.Clayer != ObjectId.Null && blkRefObjectId != ObjectId.Null)
                    using (Transaction tr = Database.TransactionManager.StartTransaction())
                    {
                        var blockReference = tr.GetObject(blkRefObjectId, OpenMode.ForWrite) as BlockReference;
                        var layer = tr.GetObject(Database.Clayer, OpenMode.ForRead) as LayerTableRecord;
                        if (blockReference != null) blockReference.Layer = layer?.Name;
                        tr.Commit();
                    }
            }
        }

        public static void WriteMessageInDebug(string message)
        {
#if DEBUG
            Editor?.WriteMessage("\n" + message);
#endif
        }

        /// <summary>Получить имя типа линии по ObjectId</summary>
        /// <param name="ltid">ObjectId</param>
        /// <returns></returns>
        public static string GetLineTypeName(ObjectId ltid)
        {

            var lt = "Continuous";
            if (ltid == ObjectId.Null) return lt;

            using (Document.LockDocument())
            {
                using (var tr = Database.TransactionManager.StartTransaction())
                {
                    var linetype = tr.GetObject(ltid, OpenMode.ForRead) as LinetypeTableRecord;
                    lt = linetype?.Name;
                    tr.Commit();
                }
            }
            return lt;
        }

        /// <summary>Получить ObjectId типа линии по имени в текущем документе</summary>
        /// <param name="ltname">Имя типа линии</param>
        /// <returns></returns>
        public static ObjectId GetLineTypeObjectId(string ltname)
        {
            var ltid = ObjectId.Null;
            if (string.IsNullOrEmpty(ltname)) return ObjectId.Null;

            using (Document.LockDocument())
            {
                using (var tr = Database.TransactionManager.StartTransaction())
                {
                    var lttbl = tr.GetObject(Database.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
                    if (lttbl != null)
                        if (lttbl.Has(ltname))
                        {
                            ltid = lttbl[ltname];
                        }
                        else
                        {
                            const string filename = "acad.lin";
                            try
                            {
                                var path = HostApplicationServices.Current.FindFile(
                                    filename, Database, FindFileHint.Default);
                                Database.LoadLineTypeFile(ltname, path);
                                ltid = lttbl[ltname];
                            }
                            catch (Autodesk.AutoCAD.Runtime.Exception exception)
                            {
                                if (exception.ErrorStatus == ErrorStatus.FilerError)
                                    MessageBox.Show(Language.GetItem(Invariables.LangItem, "err1") + ": " + filename, MessageBoxIcon.Close); // Не удалось найти файл
                                else if (exception.ErrorStatus == ErrorStatus.DuplicateRecordName)
                                {
                                    // ignore
                                }
                                else MessageBox.Show(Language.GetItem(Invariables.LangItem, "err2") + ": " + ltname, MessageBoxIcon.Close); //Не удалось загрузить тип линий
                            }
                        }
                    tr.Commit();
                }
            }
            return ltid;
        }

        /// <summary>Установка типа линии для блока согласно стиля</summary>
        public static void SetLineType(ObjectId blkRefObjectId, string lineTypeName)
        {
            if (blkRefObjectId == ObjectId.Null) return;
            using (Document.LockDocument())
            {
                using (var tr = Document.TransactionManager.StartTransaction())
                {
                    var blockReference = tr.GetObject(blkRefObjectId, OpenMode.ForWrite) as BlockReference;
                    if (blockReference != null)
                    {
                        if (HasLineType(lineTypeName, tr))
                            blockReference.Linetype = lineTypeName;
                        else
                        {
                            if (LoadLineType(lineTypeName))
                                blockReference.Linetype = lineTypeName;
                        }
                    }
                    tr.Commit();
                }
            }
        }

        /// <summary>Проверка наличия типа линии в документе</summary>
        private static bool HasLineType(string lineTypeName, Transaction tr)
        {
            var lttbl = tr.GetObject(Database.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
            if (lttbl != null)
                return (lttbl.Has(lineTypeName));
            return false;
        }

        /// <summary>Загрузка типа линии в файл</summary>
        /// <param name="lineTypeName">Имя типа линии</param>
        /// <param name="fileNames">Файлы, в которых искать тип линии. Если не указаны, то из стандартного</param>
        public static bool LoadLineType(string lineTypeName, List<string> fileNames = null)
        {
            var loaded = false;
            if (fileNames != null)
            {
                foreach (var fileName in fileNames)
                {
                    try
                    {
                        Database.LoadLineTypeFile(lineTypeName, fileName);
                        loaded = true;
                        break;
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
            if (!loaded)
            {
                try
                {
                    Database.LoadLineTypeFile(lineTypeName, "acad.lin");
                    loaded = true;
                }
                catch
                {
                    // ignore
                }
            }
            return loaded;
        }

        public static ObjectId GetTextStyleIdByName(string textStyleName)
        {
            using (Document.LockDocument())
            {
                using (OpenCloseTransaction tr = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    var txtstbl = (TextStyleTable)tr.GetObject(Database.TextStyleTableId, OpenMode.ForRead);
                    foreach (ObjectId objectId in txtstbl)
                    {
                        var txtStl = (TextStyleTableRecord)tr.GetObject(objectId, OpenMode.ForRead);
                        if (txtStl.Name.Equals(textStyleName))
                            return objectId;
                    }
                }
            }
            return ObjectId.Null;
        }

        /// <summary>
        /// Найти все блоки в текущем пространстве (модель/лист), представляющие интеллектуальный примитив указанного типа
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static List<T> GetAllIntellectualEntitiesInCurrentSpace<T>(Type entityType) where T : IntellectualEntity
        {
            List<T> list = new List<T>();
            var appName = $"mp{entityType.Name}";
            using (var tr = Database.TransactionManager.StartOpenCloseTransaction())
            {
                BlockTableRecord blockTableRecord = (BlockTableRecord)tr.GetObject(Database.CurrentSpaceId, OpenMode.ForRead);
                foreach (ObjectId objectId in blockTableRecord)
                {
                    if(objectId == ObjectId.Null)
                        continue;
                    if (tr.GetObject(objectId, OpenMode.ForRead) is BlockReference blockReference)
                    {
                        var xData = blockReference.GetXDataForApplication(appName);
                        if (xData != null)
                            list.Add(EntityReaderFactory.Instance.GetFromEntity<T>(blockReference));
                    }
                }
                tr.Commit();
            }

            return list;
        }
    }

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
                RegAppTable rat =
                    (RegAppTable)tr.GetObject(AcadHelpers.Database.RegAppTableId, OpenMode.ForRead, false);
                if (!rat.Has(appName))
                {
                    rat.UpgradeOpen();
                    RegAppTableRecord ratr = new RegAppTableRecord
                    {
                        Name = appName
                    };
                    rat.Add(ratr);
                    tr.AddNewlyCreatedDBObject(ratr, true);
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
            DBObject dbObject = rxObject as DBObject;
            if (dbObject == null)
                return false;
            if (checkIsNullId)
                if (dbObject.ObjectId == ObjectId.Null)
                    return false;
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
                return false;
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
            ResultBuffer rb = blkRef.GetXDataForApplication(appName);
            return rb != null;
        }

        public static bool IsIntellectualEntity(DBObject dbObject, string appName)
        {
            ResultBuffer rb = dbObject.GetXDataForApplication(appName);
            return rb != null;
        }
    }
}
