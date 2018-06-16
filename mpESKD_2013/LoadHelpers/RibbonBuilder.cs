using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using System;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using RibbonPanelSource = Autodesk.Windows.RibbonPanelSource;
using RibbonRowPanel = Autodesk.Windows.RibbonRowPanel;
using System.Windows.Controls;
using ModPlus.Helpers;
using ModPlusAPI;
using ModPlusAPI.Windows;

namespace mpESKD.LoadHelpers
{
    public class RibbonBuilder
    {
        private const string LangItem = "mpESKD";
        public static void BuildRibbon()
        {
            if (!IsLoaded())
            {
                GetColorTheme();
                CreateRibbon();
                AcApp.SystemVariableChanged -= AcadApp_SystemVariableChanged;
                AcApp.SystemVariableChanged += AcadApp_SystemVariableChanged;
            }
        }
        private static bool IsLoaded()
        {
            var loaded = false;
            var ribCntrl = ComponentManager.Ribbon;
            var tabName = Language.TryGetCuiLocalGroupName("ModPlus ЕСКД");
            foreach (var tab in ribCntrl.Tabs)
            {
                if (tab.Id.Equals("ModPlus_ESKD") && tab.Title.Equals(tabName))
                {
                    loaded = true;
                    break;
                }
            }
            return loaded;
        }
        private static bool IsActive()
        {
            var ribCntrl = ComponentManager.Ribbon;
            var tabName = Language.TryGetCuiLocalGroupName("ModPlus ЕСКД");
            foreach (var tab in ribCntrl.Tabs)
            {
                if (tab.Id.Equals("ModPlus_ESKD") && tab.Title.Equals(tabName))
                    return tab.IsActive;
            }
            return false;
        }
        public static void RemoveRibbon()
        {
            try
            {
                if (IsLoaded())
                {
                    var ribCntrl = ComponentManager.Ribbon;
                    var tabName = Language.TryGetCuiLocalGroupName("ModPlus ЕСКД");
                    foreach (var tab in ribCntrl.Tabs.Where(
                        tab => tab.Id.Equals("ModPlus_ESKD") && tab.Title.Equals(tabName)))
                    {
                        ribCntrl.Tabs.Remove(tab);
                        AcApp.SystemVariableChanged -= AcadApp_SystemVariableChanged;
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
        private static bool _wasActive = false;

        private static void AcadApp_SystemVariableChanged(object sender, SystemVariableChangedEventArgs e)
        {
            if (e.Name.Equals("WSCURRENT")) BuildRibbon();
            if (e.Name.Equals("COLORTHEME"))
            {
                _wasActive = IsActive();
                RemoveRibbon();
                BuildRibbon();
            }
        }

        private static int _colorTheme = 1;

        private static void GetColorTheme()
        {
            var sv = AcApp.GetSystemVariable("COLORTHEME").ToString();
            if (int.TryParse(sv, out int i))
                _colorTheme = i;
            else _colorTheme = 1; // light
        }

        private static void CreateRibbon()
        {
            try
            {
                var ribCntrl = ComponentManager.Ribbon;
                // add the tab
                var tabName = Language.TryGetCuiLocalGroupName("ModPlus ЕСКД");
                var ribTab = new RibbonTab { Title = tabName, Id = "ModPlus_ESKD" };
                ribCntrl.Tabs.Add(ribTab);
                // add content
                AddAxisPanel(ribTab);
                AddLinesPanel(ribTab);
                // add settings panel
                AddSettingsPanel(ribTab);
                ////////////////////////
                ribCntrl.UpdateLayout();
                if (_wasActive)
                    ribTab.IsActive = true;
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
        private static void AddAxisPanel(RibbonTab ribTab)
        {
            // Линии
            // create the panel source
            var ribSourcePanel = new RibbonPanelSource { Title = Language.GetItem(LangItem, "tab3") };
            // now the panel
            var ribPanel = new RibbonPanel { Source = ribSourcePanel };
            ribTab.Panels.Add(ribPanel);

            var ribRowPanel = new RibbonRowPanel();
            #region mpAxis
            // Добавляем в него первую функцию, которую делаем основной
            var ribBtn = RibbonHelpers.AddBigButton(
                Functions.mpAxis.AxisInterface.Name,
                Functions.mpAxis.AxisInterface.LName,
                GetBigIconForFunction(Functions.mpAxis.AxisInterface.Name, Functions.mpAxis.AxisInterface.Name),
                Functions.mpAxis.AxisInterface.Description,
                Orientation.Vertical,
                Functions.mpAxis.AxisInterface.FullDescription,
                GetHelpImageForFunction(Functions.mpAxis.AxisInterface.Name, Functions.mpAxis.AxisInterface.ToolTipHelpImage)
                );
            if (ribBtn != null) ribRowPanel.Items.Add(ribBtn);

            #endregion
            if (ribRowPanel.Items.Any())
            {
                ribSourcePanel.Items.Add(ribRowPanel);
            }
        }
        private static void AddLinesPanel(RibbonTab ribTab)
        {
            // Линии
            // create the panel source
            var ribSourcePanel = new RibbonPanelSource { Title = Language.GetItem(LangItem, "tab1") };
            // now the panel
            var ribPanel = new RibbonPanel { Source = ribSourcePanel };
            ribTab.Panels.Add(ribPanel);

            var ribRowPanel = new RibbonRowPanel();
            #region mpBreakLine
            // Создаем SplitButton
            var risSplitBtn = new RibbonSplitButton
            {
                Text = "RibbonSplitButton",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                ShowText = true,
                ListButtonStyle = Autodesk.Private.Windows.RibbonListButtonStyle.SplitButton,
                ResizeStyle = RibbonItemResizeStyles.NoResize,
                ListStyle = RibbonSplitButtonListStyle.List
            };
            // Добавляем в него первую функцию, которую делаем основной
            var ribBtn = RibbonHelpers.AddBigButton(
                Functions.mpBreakLine.BreakLineInterface.Name,
                Functions.mpBreakLine.BreakLineInterface.LName,
                GetBigIconForFunction(Functions.mpBreakLine.BreakLineInterface.Name, Functions.mpBreakLine.BreakLineInterface.Name),
                Functions.mpBreakLine.BreakLineInterface.Description,
                Orientation.Vertical,
                Functions.mpBreakLine.BreakLineInterface.FullDescription,
                GetHelpImageForFunction(Functions.mpBreakLine.BreakLineInterface.Name, Functions.mpBreakLine.BreakLineInterface.ToolTipHelpImage)
                );
            if (ribBtn != null)
            {
                risSplitBtn.Items.Add(ribBtn);
                risSplitBtn.Current = ribBtn;
            }
            // Затем добавляем подфункции
            for (int i = 0; i < Functions.mpBreakLine.BreakLineInterface.SubFunctionsNames.Count; i++)
            {
                risSplitBtn.Items.Add(RibbonHelpers.AddBigButton(
                    Functions.mpBreakLine.BreakLineInterface.SubFunctionsNames[i],
                    Functions.mpBreakLine.BreakLineInterface.SubFunctionsLNames[i],
                    GetBigIconForFunction(Functions.mpBreakLine.BreakLineInterface.Name, Functions.mpBreakLine.BreakLineInterface.SubFunctionsNames[i]),
                    Functions.mpBreakLine.BreakLineInterface.SubDescriptions[i], Orientation.Vertical,
                    Functions.mpBreakLine.BreakLineInterface.SubFullDescriptions[i],
                    GetHelpImageForFunction(Functions.mpBreakLine.BreakLineInterface.Name, Functions.mpBreakLine.BreakLineInterface.SubHelpImages[i])
                ));
            }
            ribRowPanel.Items.Add(risSplitBtn);
            #endregion
            if (ribRowPanel.Items.Any())
            {
                ribSourcePanel.Items.Add(ribRowPanel);
            }
        }
        private static void AddSettingsPanel(RibbonTab ribTab)
        {
            //create the panel source
            var ribSourcePanel = new RibbonPanelSource
            {
                Title = Language.GetItem(LangItem, "tab2")
            };
            // now the panel
            var ribPanel = new RibbonPanel
            {
                Source = ribSourcePanel
            };
            ribTab.Panels.Add(ribPanel);

            var ribRowPanel = new RibbonRowPanel();
            ribRowPanel.Items.Add(
                RibbonHelpers.AddBigButton(
                    "mpStyleEditor",
                    Language.GetItem(LangItem, "tab4"),
                    _colorTheme == 1 // 1 - light
                    ? "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Resources/StyleEditor_32x32.png"
                    : "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Resources/StyleEditor_32x32_dark.png",
                    Language.GetItem(LangItem, "tab5"), Orientation.Vertical, "", ""
                ));
            ribRowPanel.Items.Add(
                RibbonHelpers.AddBigButton(
                    "mpPropertiesPalette",
                    ConvertLName(Language.GetItem(LangItem, "tab6")),
                    _colorTheme == 1 // 1 - light
                    ? "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Resources/Properties_32x32.png"
                    : "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Resources/Properties_32x32_dark.png",
                    Language.GetItem(LangItem, "tab7"), Orientation.Vertical, "", ""
                ));
            ribSourcePanel.Items.Add(ribRowPanel);
        }

        private static string GetBigIconForFunction(string functionName, string subFunctionName)
        {
            return _colorTheme == 1
                ? "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Functions/" +
                  functionName + "/Icons/" + subFunctionName + "_32x32.png"
                : "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Functions/" +
                  functionName + "/Icons/" + subFunctionName + "_32x32_dark.png";
        }
        private static string GetSmallIconForFunction(string functionName, string subFunctionName)
        {
            return _colorTheme == 1
                ? "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Functions/" +
                   functionName + "/Icons/" + subFunctionName + "_16x16.png"
                : "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Functions/" +
                  functionName + "/Icons/" + subFunctionName + "_16x16_dark.png";
        }

        private static string GetHelpImageForFunction(string functionName, string imgName)
        {
            return "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Functions/" +
                   functionName + "/Help/" + imgName;
        }
        /// <summary>Вспомогательный метод для добавления символа перехода на новую строку в именах функций на палитре</summary>
        private static string ConvertLName(string lName)
        {
            if (!lName.Contains(" ")) return lName;
            if (lName.Length <= 8) return lName;
            if (lName.Count(x => x == ' ') == 1)
            {
                return lName.Split(' ')[0] + Environment.NewLine + lName.Split(' ')[1];
            }
            var center = lName.Length * 0.5;
            var nearestDelta = lName.Select((c, i) => new { index = i, value = c }).Where(w => w.value == ' ')
                .OrderBy(x => Math.Abs(x.index - center)).First().index;
            return lName.Substring(0, nearestDelta) + Environment.NewLine + lName.Substring(nearestDelta + 1);
        }
    }
}
