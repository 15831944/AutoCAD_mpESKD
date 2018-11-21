namespace mpESKD
{
    using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Windows.Forms.Integration;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Autodesk.AutoCAD.Windows;
    using Base;
    using Base.Helpers;
    using Functions.mpAxis;
    using mpESKD.Base.Properties;
    using ModPlus;
    using ModPlusAPI;

    public class MainFunction : IExtensionApplication
    {
        public static string LangItem = "mpESKD";
        
        #region Properties palette

        public static void AddToMpPalette(bool show)
        {
            PaletteSet mpPaletteSet = MpPalette.MpPaletteSet;
            if (mpPaletteSet != null)
            {
                bool flag = false;
                foreach (Palette palette in mpPaletteSet)
                {
                    if (palette.Name.Equals(Language.GetItem(LangItem, "h11"))) // Свойства примитивов ModPlus
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    PropertiesPalette lmPalette = new PropertiesPalette();
                    mpPaletteSet.Add(Language.GetItem(LangItem, "h11"), new ElementHost
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
                    if (!mpPaletteSet[num].Name.Equals(Language.GetItem(LangItem, "h11")))
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
        }

        public void Terminate()
        {
            TypeFactory.Instance.GetEntityFunctionTypes().ForEach(f => f.Terminate());
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
                    Language.GetItem(LangItem, "err5"),
                    ModPlusAPI.Windows.MessageBoxIcon.Close);
            }
        }

        /// <summary>Обработка двойного клика по блоку</summary>
        private static void AcApp_BeginDoubleClick(object sender, Autodesk.AutoCAD.ApplicationServices.BeginDoubleClickEventArgs e)
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
                        var obj = tr.GetObject(ids[0], OpenMode.ForWrite);
                        // if axis
                        if (obj is BlockReference blockReference && 
                            ExtendedDataHelpers.IsIntellectualEntity(blockReference, AxisDescriptor.Instance.Name))
                        {
                            BeditCommandWatcher.UseBedit = false;
                            AxisFunction.DoubleClickEdit(blockReference, location, tr);
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
