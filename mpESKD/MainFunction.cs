namespace mpESKD
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Windows.Forms.Integration;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Runtime;
    using Autodesk.AutoCAD.Windows;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using Functions.mpAxis;
    using Functions.mpSection;
    using Functions.SearchEntities;
    using ModPlus;
    using ModPlusAPI;
    using mpESKD.Base.Properties;
    using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

    public class MainFunction : IExtensionApplication
    {
        private static ContextMenuExtension _intellectualEntityContextMenu;

        /// <summary>
        /// Путь к папке хранения пользовательских стилей
        /// </summary>
        public static string StylesPath { get; private set; } = string.Empty;

        /// <inheritdoc />
        public void Initialize()
        {
            StartUpInitialize();

            // Functions Init
            TypeFactory.Instance.GetEntityFunctionTypes().ForEach(f => f.Initialize());

            Overrule.Overruling = true;

            // ribbon build for
            Autodesk.Windows.ComponentManager.ItemInitialized += ComponentManager_ItemInitialized;

            // palette
            var loadPropertiesPalette = MainSettings.Instance.AutoLoad;
            var addPropertiesPaletteToMpPalette = MainSettings.Instance.AddToMpPalette;

            if (loadPropertiesPalette & !addPropertiesPaletteToMpPalette)
            {
                PropertiesPaletteFunction.Start();
            }
            else if (loadPropertiesPalette & addPropertiesPaletteToMpPalette)
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }

            // bedit watcher
            BeditCommandWatcher.Initialize();
            AcApp.BeginDoubleClick += AcApp_BeginDoubleClick;

            AcadHelpers.Documents.DocumentCreated += Documents_DocumentCreated;
            AcadHelpers.Documents.DocumentActivated += Documents_DocumentActivated;

            foreach (Document document in AcadHelpers.Documents)
            {
                document.ImpliedSelectionChanged += Document_ImpliedSelectionChanged;
            }
        }

        /// <inheritdoc />
        public void Terminate()
        {
            TypeFactory.Instance.GetEntityFunctionTypes().ForEach(f => f.Terminate());

            // remove context menu
            DetachCreateAnalogContextMenu();
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        public static void StartUpInitialize()
        {
            var curDir = Constants.CurrentDirectory;
            if (!string.IsNullOrEmpty(curDir))
            {
                var mpcoStylesPath = Path.Combine(Constants.UserDataDirectory, "Styles");
                if (!Directory.Exists(mpcoStylesPath))
                {
                    Directory.CreateDirectory(mpcoStylesPath);
                }

                // set public parameter
                StylesPath = mpcoStylesPath;
            }
            else
            {
                ModPlusAPI.Windows.MessageBox.Show(
                    Language.GetItem(Invariables.LangItem, "err5"),
                    ModPlusAPI.Windows.MessageBoxIcon.Close);
            }
        }

        /// <summary>
        /// Создание вкладки на ленте
        /// </summary>
        [CommandMethod("ModPlus", "mpESKDCreateRibbonTab", CommandFlags.Modal)]
        public void CreateRibbon()
        {
            if (Autodesk.Windows.ComponentManager.Ribbon == null)
            {
                return;
            }

            RibbonBuilder.BuildRibbon();
        }

        /// <summary>
        /// Команда "Создать аналог"
        /// </summary>
        [CommandMethod("ModPlus", "mpESKDCreateAnalog", CommandFlags.UsePickSet)]
        public void CreateAnalogCommand()
        {
            var psr = AcadHelpers.Editor.SelectImplied();
            if (psr.Value == null || psr.Value.Count != 1) 
                return;

            IntellectualEntity intellectualEntity = null;
            using (AcadHelpers.Document.LockDocument())
            {
                using (var tr = new OpenCloseTransaction())
                {
                    foreach (SelectedObject selectedObject in psr.Value)
                    {
                        if (selectedObject.ObjectId == ObjectId.Null)
                        {
                            continue;
                        }

                        var obj = tr.GetObject(selectedObject.ObjectId, OpenMode.ForRead);
                        if (obj is BlockReference blockReference)
                        {
                            intellectualEntity = EntityReaderFactory.Instance.GetFromEntity(blockReference);
                        }
                    }

                    tr.Commit();
                }
            }

            if (intellectualEntity == null)
                return;

            var copyLayer = true;
            var layerActionOnCreateAnalog = MainSettings.Instance.LayerActionOnCreateAnalog;
            if (layerActionOnCreateAnalog == LayerActionOnCreateAnalog.NotCopy)
            {
                copyLayer = false;
            }
            else if (layerActionOnCreateAnalog == LayerActionOnCreateAnalog.Ask)
            {
                var promptKeywordOptions =
                    new PromptKeywordOptions("\n" + Language.GetItem(Invariables.LangItem, "msg8"), "Yes No");
                var promptResult = AcadHelpers.Editor.GetKeywords(promptKeywordOptions);
                if (promptResult.Status == PromptStatus.OK)
                {
                    if (promptResult.StringResult == "No")
                    {
                        copyLayer = false;
                    }
                }
                else
                {
                    copyLayer = false;
                }
            }

            var function = TypeFactory.Instance.GetEntityFunctionTypes().FirstOrDefault(f =>
            {
                var functionName = $"{intellectualEntity.GetType().Name}Function";
                var fName = f.GetType().Name;
                return fName == functionName;
            });
            function?.CreateAnalog(intellectualEntity, copyLayer);
        }

        public static BlockReference CreateBlock(IntellectualEntity intellectualEntity)
        {
            BlockReference blockReference;
            using (AcadHelpers.Document.LockDocument())
            {
                ObjectId objectId;
                using (var transaction = AcadHelpers.Document.TransactionManager.StartTransaction())
                {
                    using (var blockTable = AcadHelpers.Database.BlockTableId.Write<BlockTable>())
                    {
                        var blockTableRecordObjectId = blockTable.Add(intellectualEntity.BlockRecord);
                        blockReference = new BlockReference(intellectualEntity.InsertionPoint, blockTableRecordObjectId);
                        using (var blockTableRecord = AcadHelpers.Database.CurrentSpaceId.Write<BlockTableRecord>())
                        {
                            blockTableRecord.BlockScaling = BlockScaling.Uniform;
                            objectId = blockTableRecord.AppendEntity(blockReference);
                        }

                        transaction.AddNewlyCreatedDBObject(blockReference, true);
                        transaction.AddNewlyCreatedDBObject(intellectualEntity.BlockRecord, true);
                    }

                    transaction.Commit();
                }

                intellectualEntity.BlockId = objectId;
            }

            return blockReference;
        }

        /// <summary>
        /// Подключение палитры свойств интеллектуальных примитивов к палитре ModPlus
        /// </summary>
        public static void AddToMpPalette()
        {
            var mpPaletteSet = MpPalette.MpPaletteSet;
            if (mpPaletteSet != null)
            {
                var flag = false;
                foreach (Palette palette in mpPaletteSet)
                {
                    if (palette.Name.Equals(Language.GetItem(Invariables.LangItem, "h11"))) //// Свойства примитивов ModPlus
                    {
                        flag = true;
                    }
                }

                if (!flag)
                {
                    var lmPalette = new PropertiesPalette();
                    mpPaletteSet.Add(Language.GetItem(Invariables.LangItem, "h11"), new ElementHost
                    {
                        AutoSize = true,
                        Dock = DockStyle.Fill,
                        Child = lmPalette
                    });
                    
                    mpPaletteSet.Visible = true;
                }
            }

            if (PropertiesPaletteFunction.PaletteSet != null)
            {
                PropertiesPaletteFunction.PaletteSet.Visible = false;
            }
        }

        /// <summary>
        /// Отключение палитры свойств интеллектуальных примитивов от палитры ModPlus
        /// </summary>
        /// <param name="fromSettings">True - метод запущен из окна настроек палитры</param>
        public static void RemoveFromMpPalette(bool fromSettings)
        {
            var mpPaletteSet = MpPalette.MpPaletteSet;
            if (mpPaletteSet != null)
            {
                var num = 0;
                while (num < mpPaletteSet.Count)
                {
                    if (!mpPaletteSet[num].Name.Equals(Language.GetItem(Invariables.LangItem, "h11")))
                    {
                        num++;
                    }
                    else
                    {
                        mpPaletteSet.Remove(num);
                        break;
                    }
                }
            }

            if (PropertiesPaletteFunction.PaletteSet != null)
            {
                PropertiesPaletteFunction.PaletteSet.Visible = true;
            }
            else if (fromSettings)
            {
                if (AcApp.DocumentManager.MdiActiveDocument != null)
                {
                    PropertiesPaletteFunction.Start();
                }
            }
        }

        private static void ComponentManager_ItemInitialized(object sender, Autodesk.Windows.RibbonItemEventArgs e)
        {
            // now one Ribbon item is initialized, but the Ribbon control
            // may not be available yet, so check if before
            if (Autodesk.Windows.ComponentManager.Ribbon == null)
            {
                return;
            }

            RibbonBuilder.BuildRibbon();

            // and remove the event handler
            Autodesk.Windows.ComponentManager.ItemInitialized -= ComponentManager_ItemInitialized;
        }

        /// <summary>Обработка двойного клика по блоку</summary>
        private static void AcApp_BeginDoubleClick(object sender, BeginDoubleClickEventArgs e)
        {
            var pt = e.Location;

            var psr = AcadHelpers.Editor.SelectImplied();
            if (psr.Status != PromptStatus.OK)
            {
                return;
            }

            var ids = psr.Value.GetObjectIds();

            if (ids.Length != 1) 
                return;

            using (AcadHelpers.Document.LockDocument())
            {
                using (var tr = AcadHelpers.Document.TransactionManager.StartTransaction())
                {
                    var obj = tr.GetObject(ids[0], OpenMode.ForWrite, true, true);
                    if (obj is BlockReference blockReference)
                    {
                        // axis
                        if (ExtendedDataHelpers.IsIntellectualEntity(blockReference, AxisDescriptor.Instance.Name))
                        {
                            AxisFunction.DoubleClickEdit(blockReference, pt, tr);
                        }

                        // section
                        else if (ExtendedDataHelpers.IsIntellectualEntity(blockReference, SectionDescriptor.Instance.Name))
                        {
                            SectionFunction.DoubleClickEdit(blockReference, pt, tr);
                        }
                        else
                        {
                            BeditCommandWatcher.UseBedit = true;
                        }
                    }
                    else
                    {
                        BeditCommandWatcher.UseBedit = true;
                    }

                    tr.Commit();
                }
            }
        }
        
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (!args.Name.Contains("ModPlus_"))
                return null;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            PropertiesPaletteFunction.Start();

            return null;
        }
        
        private static void Documents_DocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            if (e.Document == null)
                return;

            e.Document.ImpliedSelectionChanged -= Document_ImpliedSelectionChanged;
            e.Document.ImpliedSelectionChanged += Document_ImpliedSelectionChanged;

            // при открытии документа соберу все блоки в текущем пространстве (?) и вызову обновление их внутренних
            // примитивов. Нужно, так как в некоторых случаях (пока не ясно в каких) внутренние примитивы отсутствуют
            try
            {
                var timer = Stopwatch.StartNew();
                using (var tr = AcadHelpers.Document.TransactionManager.StartOpenCloseTransaction())
                {
                    foreach (var blockReference in SearchEntitiesCommand.GetBlockReferencesOfIntellectualEntities(
                        TypeFactory.Instance.GetEntityCommandNames(), tr))
                    {
                        var ie = EntityReaderFactory.Instance.GetFromEntity(blockReference);
                        if (ie != null)
                        {
                            blockReference.UpgradeOpen();
                            ie.UpdateEntities();
                            ie.GetBlockTableRecordForUndo(blockReference).UpdateAnonymousBlocks();
                        }
                    }

                    tr.Commit();
                }

                timer.Stop();
                Debug.Print($"Time for update entities: {timer.ElapsedMilliseconds} milliseconds");
            }
            catch
            {
                // ignore
            }
        }

        private static void Documents_DocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            if (e.Document == null) 
                return;

            e.Document.ImpliedSelectionChanged -= Document_ImpliedSelectionChanged;
            e.Document.ImpliedSelectionChanged += Document_ImpliedSelectionChanged;
        }

        private static void Document_ImpliedSelectionChanged(object sender, EventArgs e)
        {
            var psr = AcadHelpers.Editor.SelectImplied();
            var detach = true;
            if (psr.Value != null && psr.Value.Count == 1)
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var tr = new OpenCloseTransaction())
                    {
                        foreach (SelectedObject selectedObject in psr.Value)
                        {
                            if (selectedObject.ObjectId == ObjectId.Null)
                            {
                                continue;
                            }

                            var obj = tr.GetObject(selectedObject.ObjectId, OpenMode.ForRead);
                            if (obj is BlockReference blockReference &&
                                ExtendedDataHelpers.IsApplicable(blockReference))
                            {
                                AttachCreateAnalogContextMenu();
                                detach = false;
                            }
                        }

                        tr.Commit();
                    }
                }
            }

            if (detach)
            {
                DetachCreateAnalogContextMenu();
            }
        }
        
        private static void AttachCreateAnalogContextMenu()
        {
            if (_intellectualEntityContextMenu == null)
            {
                _intellectualEntityContextMenu = new ContextMenuExtension();
                var menuItem = new Autodesk.AutoCAD.Windows.MenuItem(Language.GetItem(Invariables.LangItem, "h95"));
                menuItem.Click += CreateAnalogMenuItem_Click;
                _intellectualEntityContextMenu.MenuItems.Add(menuItem);
            }

            var rxObject = RXObject.GetClass(typeof(BlockReference));
            Autodesk.AutoCAD.ApplicationServices.Application.AddObjectContextMenuExtension(
                rxObject, _intellectualEntityContextMenu);
        }

        private static void CreateAnalogMenuItem_Click(object sender, EventArgs e)
        {
            AcApp.DocumentManager.MdiActiveDocument.SendStringToExecute(
                "_.mpESKDCreateAnalog ", false, false, false);
        }

        private static void DetachCreateAnalogContextMenu()
        {
            if (_intellectualEntityContextMenu == null)
                return;

            var rxObject = RXObject.GetClass(typeof(BlockReference));
            Autodesk.AutoCAD.ApplicationServices.Application.RemoveObjectContextMenuExtension(
                rxObject, _intellectualEntityContextMenu);
        }
    }
}
