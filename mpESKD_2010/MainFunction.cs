#if ac2010
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
#elif ac2013
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#endif
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using mpESKD.Base.Properties;
using ModPlus;
using ModPlusAPI;
using MessageBox = ModPlusAPI.Windows.MessageBox;
using MessageBoxIcon = ModPlusAPI.Windows.MessageBoxIcon;

namespace mpESKD
{
    public class MainFunction : IExtensionApplication
    {
        #region Properties palette
        
        public static void AddToMpPalette(bool show)
        {
            PaletteSet mpPaletteSet = MpPalette.MpPaletteSet;
            if (mpPaletteSet != null)
            {
                bool flag = false;
                foreach (Palette palette in mpPaletteSet)
                {
                    if (palette.Name.Equals("Свойства примитивов ModPlus"))
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    PropertiesPalette lmPalette = new PropertiesPalette();
                    mpPaletteSet.Add("Свойства примитивов ModPlus", new ElementHost
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
            if (PropertiesFunction._paletteSet != null)
            {
                PropertiesFunction._paletteSet.Visible = false;
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
                    if (!mpPaletteSet[num].Name.Equals("Свойства примитивов ModPlus"))
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
            if (PropertiesFunction._paletteSet != null)
            {
                PropertiesFunction._paletteSet.Visible = true;
            }
            else if (fromSettings)
            {
                if (AcApp.DocumentManager.MdiActiveDocument != null)
                {
                    AcApp.DocumentManager.MdiActiveDocument.SendStringToExecute("_MPPROPERTIESPALETTE ", true, false, false);
                }
            }
        }
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("ModPlus_"))
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                PropertiesFunction.Start();
            }
            return null;
        }
        #endregion
        public void Initialize()
        {
            StartUpInitialize();
            // Functions Init
            Functions.mpBreakLine.BreakLineFunction.Initialize();
            // ribbon build for
            Autodesk.Windows.ComponentManager.ItemInitialized += ComponentManager_ItemInitialized;
            // palette
            var loadPropertiesPalette = bool.TryParse(UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings,
                                        "mpESKD", "AutoLoad"), out bool b) & b;
            var addPropertiesPaletteToMpPalette = bool.TryParse(UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings,
                                                  "mpESKD", "AddToMpPalette"), out b) & b;
            if (loadPropertiesPalette & !addPropertiesPaletteToMpPalette)
            {
                PropertiesFunction.Start();
            }
            else if (loadPropertiesPalette & addPropertiesPaletteToMpPalette)
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }
        }

        public void Terminate()
        {
            Functions.mpBreakLine.BreakLineFunction.Terminate();
        }
        static void ComponentManager_ItemInitialized(object sender, Autodesk.Windows.RibbonItemEventArgs e)
        {
            //now one Ribbon item is initialized, but the Ribbon control
            //may not be available yet, so check if before
            if (Autodesk.Windows.ComponentManager.Ribbon == null) return;
            LoadHelpers.RibbonBuilder.BuildRibbon();
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
                var mpcoPath = Path.Combine(curDir, "UserData");
                if (!Directory.Exists(mpcoPath))
                    Directory.CreateDirectory(mpcoPath);
                var mpcoStylesPath = Path.Combine(mpcoPath, "Styles");
                if (!Directory.Exists(mpcoStylesPath))
                    Directory.CreateDirectory(mpcoStylesPath);
                // set public parameter
                StylesPath = mpcoStylesPath;
            }
            else
            {
                MessageBox.Show(
                    "Ошибка получения данных из реестра! Запустите Конфигуратор для обновления данных в реестре",
                    MessageBoxIcon.Close);
            }
        }
    }
}
