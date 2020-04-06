namespace mpESKD.Functions.SearchEntities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using ModPlusAPI;
    using ModPlusAPI.Windows;

    public static class SearchEntitiesCommand
    {
        [CommandMethod("ModPlus", "mpESKDSearch", CommandFlags.Modal)]
        public static void Start()
        {
            try
            {
                List<Type> types = TypeFactory.Instance.GetEntityTypes();
                SearchEntitiesSettings settings = new SearchEntitiesSettings();

                foreach (Type entityType in types)
                {
                    CheckBox checkBox = new CheckBox
                    {
                        Content = TypeFactory.Instance.GetDescriptor(entityType).LName,
                        Tag = entityType
                    };
                    ListBoxItem listBoxItem = new ListBoxItem
                    {
                        Content = checkBox
                    };
                    settings.LbEntities.Items.Add(listBoxItem);
                }

                if (settings.ShowDialog() == false)
                {
                    return;
                }

                SearchProceedOption searchProceedOption = (SearchProceedOption)settings.CbSearchProceedOption.SelectedIndex;

                List<string> entitiesToProceed = new List<string>();
                foreach (var item in settings.LbEntities.Items)
                {
                    if (item is ListBoxItem listBoxItem &&
                        listBoxItem.Content is CheckBox checkBox &&
                        checkBox.IsChecked == true)
                    {
                        entitiesToProceed.Add($"mp{((Type)checkBox.Tag).Name}");
                    }
                }

                if (!entitiesToProceed.Any())
                {
                    return;
                }

                using (Transaction tr = AcadHelpers.Document.TransactionManager.StartTransaction())
                {
                    var btr = (BlockTableRecord)tr.GetObject(AcadHelpers.Database.CurrentSpaceId, OpenMode.ForRead);

                    var blockReferences = GetBlockReferencesOfIntellectualEntities(entitiesToProceed, tr).ToList();

                    if (blockReferences.Any())
                    {
                        switch (searchProceedOption)
                        {
                            case SearchProceedOption.Select:
                                AcadHelpers.Editor.SetImpliedSelection(blockReferences.Select(b => b.ObjectId).ToArray());
                                break;
                            case SearchProceedOption.RemoveData:
                                foreach (var blockReference in blockReferences)
                                {
                                    blockReference.UpgradeOpen();
                                    var typedValue = blockReference.XData.AsArray()
                                        .FirstOrDefault(tv => tv.TypeCode == (int)DxfCode.ExtendedDataRegAppName);
                                    blockReference.XData = new ResultBuffer(
                                        new TypedValue((int)DxfCode.ExtendedDataRegAppName, typedValue.Value.ToString()));
                                }

                                MessageBox.Show($"{Language.GetItem(Invariables.LangItem, "msg9")}: {blockReferences.Count}");
                                break;
                            case SearchProceedOption.Explode:
                                btr.UpgradeOpen();
                                foreach (var blockReference in blockReferences)
                                {
                                    blockReference.UpgradeOpen();
                                    using (DBObjectCollection dbObjCol = new DBObjectCollection())
                                    {
                                        blockReference.Explode(dbObjCol);
                                        foreach (DBObject dbObj in dbObjCol)
                                        {
                                            Entity acEnt = dbObj as Entity;

                                            btr.AppendEntity(acEnt);
                                            tr.AddNewlyCreatedDBObject(dbObj, true);
                                        }
                                    }

                                    blockReference.Erase(true);
                                }

                                MessageBox.Show($"{Language.GetItem(Invariables.LangItem, "msg9")}: {blockReferences.Count}");
                                break;
                            case SearchProceedOption.Delete:
                                foreach (var blockReference in blockReferences)
                                {
                                    blockReference.UpgradeOpen();
                                    blockReference.Erase(true);
                                }

                                MessageBox.Show($"{Language.GetItem(Invariables.LangItem, "msg9")}: {blockReferences.Count}");
                                break;
                        }
                    }
                    else
                    {
                        MessageBox.Show(Language.GetItem(Invariables.LangItem, "msg10"));
                    }

                    tr.Commit();
                }
            }
            catch (System.Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        /// <summary>
        /// Найти блоки в текущем пространстве, являющиеся интеллектуальными объектами
        /// </summary>
        /// <param name="typeNames">Список имен типов, интеллектуальные объекты которых нужно найти</param>
        /// <param name="tr">Открытая транзакция</param>
        /// <returns>Коллекция блоков</returns>
        public static IEnumerable<BlockReference> GetBlockReferencesOfIntellectualEntities(
            ICollection<string> typeNames, Transaction tr)
        {
            var btr = (BlockTableRecord)tr.GetObject(AcadHelpers.Database.CurrentSpaceId, OpenMode.ForRead);
            foreach (var objectId in btr)
            {
                if (tr.GetObject(objectId, OpenMode.ForRead) is BlockReference blockReference &&
                    blockReference.XData != null)
                {
                    var typedValue = blockReference.XData.AsArray()
                        .FirstOrDefault(tv => tv.TypeCode == (int)DxfCode.ExtendedDataRegAppName);
                    if (typeNames.Contains(typedValue.Value as string))
                    {
                        yield return blockReference;
                    }
                }
            }
        }
    }
}
