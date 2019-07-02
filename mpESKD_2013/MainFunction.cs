namespace mpESKD
{
    using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Windows.Forms.Integration;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Autodesk.AutoCAD.Windows;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using Functions.mpAxis;
    using Functions.mpSection;
    using mpESKD.Base.Properties;
    using ModPlus;
    using ModPlusAPI;

    public class MainFunction : IExtensionApplication
    {
        public void Initialize()
        {
            StartUpInitialize();

            // Functions Init
            TypeFactory.Instance.GetEntityFunctionTypes().ForEach(f => f.Initialize());

            Overrule.Overruling = true;

            // ribbon build for
            Autodesk.Windows.ComponentManager.ItemInitialized += ComponentManager_ItemInitialized;

            // palette
            var loadPropertiesPalette =
                bool.TryParse(UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", "AutoLoad"), out var b) & b;
            var addPropertiesPaletteToMpPalette =
                bool.TryParse(UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", "AddToMpPalette"), out b) & b;
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

        public void Terminate()
        {
            TypeFactory.Instance.GetEntityFunctionTypes().ForEach(f => f.Terminate());

            // remove context menu
            DetachCreateAnalogContextMenu();
        }

        /// <summary>Инициализация</summary>
        public static void StartUpInitialize()
        {
            var curDir = Constants.CurrentDirectory;
            if (!string.IsNullOrEmpty(curDir))
            {
                var mpcoStylesPath = Path.Combine(Constants.UserDataDirectory, "Styles");
                if (!Directory.Exists(mpcoStylesPath))
                    Directory.CreateDirectory(mpcoStylesPath);
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

        [CommandMethod("ModPlus", "mpESKDCreateRibbonTab", CommandFlags.Modal)]
        public void CreateRibbon()
        {
            if (Autodesk.Windows.ComponentManager.Ribbon == null)
                return;
            RibbonBuilder.BuildRibbon();
        }

        private static void ComponentManager_ItemInitialized(object sender, Autodesk.Windows.RibbonItemEventArgs e)
        {
            //now one Ribbon item is initialized, but the Ribbon control
            //may not be available yet, so check if before
            if (Autodesk.Windows.ComponentManager.Ribbon == null)
                return;
            RibbonBuilder.BuildRibbon();

            //and remove the event handler
            Autodesk.Windows.ComponentManager.ItemInitialized -= ComponentManager_ItemInitialized;
        }

        public static string StylesPath = string.Empty;

        /// <summary>Обработка двойного клика по блоку</summary>
        private static void AcApp_BeginDoubleClick(object sender, BeginDoubleClickEventArgs e)
        {
            var pt = e.Location;

            PromptSelectionResult psr = AcadHelpers.Editor.SelectImplied();
            if (psr.Status != PromptStatus.OK) return;
            ObjectId[] ids = psr.Value.GetObjectIds();

            if (ids.Length == 1)
            {
                Point3d location = pt;
                using (AcadHelpers.Document.LockDocument())
                {
                    using (Transaction tr = AcadHelpers.Document.TransactionManager.StartTransaction())
                    {
                        var obj = tr.GetObject(ids[0], OpenMode.ForWrite, true, true);
                        if (obj is BlockReference blockReference)
                        {
                            // axis
                            if (ExtendedDataHelpers.IsIntellectualEntity(blockReference, AxisDescriptor.Instance.Name))
                            {
                                AxisFunction.DoubleClickEdit(blockReference, location, tr);
                            }
                            // section
                            else if (ExtendedDataHelpers.IsIntellectualEntity(blockReference, SectionDescriptor.Instance.Name))
                            {
                                SectionFunction.DoubleClickEdit(blockReference, location, tr);
                            }
                            else BeditCommandWatcher.UseBedit = true;
                        }
                        else BeditCommandWatcher.UseBedit = true;
                        tr.Commit();
                    }
                }
            }
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

        #region Properties palette

        public static void AddToMpPalette(bool show)
        {
            PaletteSet mpPaletteSet = MpPalette.MpPaletteSet;
            if (mpPaletteSet != null)
            {
                bool flag = false;
                foreach (Palette palette in mpPaletteSet)
                {
                    if (palette.Name.Equals(Language.GetItem(Invariables.LangItem, "h11"))) // Свойства примитивов ModPlus
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    PropertiesPalette lmPalette = new PropertiesPalette();
                    mpPaletteSet.Add(Language.GetItem(Invariables.LangItem, "h11"), new ElementHost
                    {
                        AutoSize = true,
                        Dock = DockStyle.Fill,
                        Child = lmPalette
                    });
                    if (show)
                    {
                        mpPaletteSet.Visible = true;
                    }
                }
            }
            if (PropertiesPaletteFunction.PaletteSet != null)
            {
                PropertiesPaletteFunction.PaletteSet.Visible = false;
            }
        }
        public static void RemoveFromMpPalette(bool fromSettings)
        {
            PaletteSet mpPaletteSet = MpPalette.MpPaletteSet;
            if (mpPaletteSet != null)
            {
                int num = 0;
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
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("ModPlus_"))
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                PropertiesPaletteFunction.Start();
            }
            return null;
        }
        #endregion

        #region Create Analog Command (and context menu)

        private void Documents_DocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            if (e.Document != null)
            {
                e.Document.ImpliedSelectionChanged -= Document_ImpliedSelectionChanged;
                e.Document.ImpliedSelectionChanged += Document_ImpliedSelectionChanged;
            }
        }

        private void Documents_DocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            if (e.Document != null)
            {
                e.Document.ImpliedSelectionChanged -= Document_ImpliedSelectionChanged;
                e.Document.ImpliedSelectionChanged += Document_ImpliedSelectionChanged;
            }
        }

        private void Document_ImpliedSelectionChanged(object sender, EventArgs e)
        {
            PromptSelectionResult psr = AcadHelpers.Editor.SelectImplied();
            bool detach = true;
            if (psr.Value != null && psr.Value.Count == 1)
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (OpenCloseTransaction tr = new OpenCloseTransaction())
                    {
                        foreach (SelectedObject selectedObject in psr.Value)
                        {
                            if (selectedObject.ObjectId == ObjectId.Null)
                                continue;
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
                DetachCreateAnalogContextMenu();
        }

        [CommandMethod("ModPlus", "mpESKDCreateAnalog", CommandFlags.UsePickSet)]
        public void CreateAnalogCommand()
        {
            PromptSelectionResult psr = AcadHelpers.Editor.SelectImplied();
            if (psr.Value != null && psr.Value.Count == 1)
            {
                IntellectualEntity intellectualEntity = null;
                using (AcadHelpers.Document.LockDocument())
                {
                    using (OpenCloseTransaction tr = new OpenCloseTransaction())
                    {
                        foreach (SelectedObject selectedObject in psr.Value)
                        {
                            if (selectedObject.ObjectId == ObjectId.Null)
                                continue;
                            var obj = tr.GetObject(selectedObject.ObjectId, OpenMode.ForRead);
                            if (obj is BlockReference blockReference)
                            {
                                intellectualEntity = EntityReaderFactory.Instance.GetFromEntity(blockReference);
                            }
                        }

                        tr.Commit();
                    }
                }

                if (intellectualEntity != null)
                {
                    var copyLayer = true;
                    if (MainStaticSettings.Settings.LayerActionOnCreateAnalog == LayerActionOnCreateAnalog.NotCopy)
                        copyLayer = false;
                    else if (MainStaticSettings.Settings.LayerActionOnCreateAnalog == LayerActionOnCreateAnalog.Ask)
                    {
                        PromptKeywordOptions promptKeywordOptions = 
                            new PromptKeywordOptions("\n" + Language.GetItem(Invariables.LangItem, "msg8"), "Yes No");
                        var promptResult = AcadHelpers.Editor.GetKeywords(promptKeywordOptions);
                        if (promptResult.Status == PromptStatus.OK)
                        {
                            if (promptResult.StringResult == "No")
                                copyLayer = false;
                        }
                        else copyLayer = false;
                    }
                    var function = TypeFactory.Instance.GetEntityFunctionTypes().FirstOrDefault(f =>
                    {
                        var functionName = $"{intellectualEntity.GetType().Name}Function";
                        var fName = f.GetType().Name;
                        return fName == functionName;
                    });
                    function?.CreateAnalog(intellectualEntity, copyLayer);
                }
            }
        }

        private static ContextMenuExtension _intellectualEntityContextMenu;

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
            Autodesk.AutoCAD.ApplicationServices.Application.AddObjectContextMenuExtension(rxObject, _intellectualEntityContextMenu);
        }

        private static void CreateAnalogMenuItem_Click(object sender, EventArgs e)
        {
            AcApp.DocumentManager.MdiActiveDocument.SendStringToExecute(
                "_.mpESKDCreateAnalog ", false, false, false);

        }

        private static void DetachCreateAnalogContextMenu()
        {
            if (_intellectualEntityContextMenu != null)
            {
                var rxObject = RXObject.GetClass(typeof(BlockReference));
                Autodesk.AutoCAD.ApplicationServices.Application.RemoveObjectContextMenuExtension(rxObject, _intellectualEntityContextMenu);
            }
        }

        #endregion
    }

    /// <summary>Слежение за командой "редактор блоков" автокада</summary>
    public class BeditCommandWatcher
    {
        /// <summary>True - использовать редактор блоков. False - не использовать</summary>
        public static bool UseBedit;

        public static void Initialize()
        {
            AcApp.DocumentManager.DocumentLockModeChanged += DocumentManager_DocumentLockModeChanged;
        }
        private static void DocumentManager_DocumentLockModeChanged(object sender, Autodesk.AutoCAD.ApplicationServices.DocumentLockModeChangedEventArgs e)
        {
            try
            {
                if (!UseBedit)
                    if (e.GlobalCommandName == "BEDIT")
                    {
                        e.Veto();
                    }
            }
            catch (System.Exception exception)
            {
                AcadHelpers.WriteMessageInDebug($"\nException {exception.Message}");
            }
        }
    }
}
