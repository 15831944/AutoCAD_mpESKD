#if ac2010
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
#elif ac2013
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#endif
using System;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using RibbonPanelSource = Autodesk.Windows.RibbonPanelSource;
using RibbonRowPanel = Autodesk.Windows.RibbonRowPanel;
using System.Windows.Controls;
using ModPlus.Helpers;
using ModPlusAPI.Windows;

namespace mpESKD.LoadHelpers
{
    public class RibbonBuilder
    {
        public static void BuildRibbon()
        {
            if (!IsLoaded())
            {
                CreateRibbon();
                AcApp.SystemVariableChanged += acadApp_SystemVariableChanged;
            }
        }
        private static bool IsLoaded()
        {
            var loaded = false;
            var ribCntrl = ComponentManager.Ribbon;
            foreach (var tab in ribCntrl.Tabs)
            {
                if (tab.Id.Equals("ModPlus_ESKD") & tab.Title.Equals("ModPlus ЕСКД"))
                    loaded = true;
                else loaded = false;
            }
            return loaded;
        }
        public static void RemoveRibbon()
        {
            try
            {
                if (IsLoaded())
                {
                    var ribCntrl = ComponentManager.Ribbon;
                    foreach (var tab in ribCntrl.Tabs.Where(
                        tab => tab.Id.Equals("ModPlus_ESKD") & tab.Title.Equals("ModPlus ЕСКД")))
                    {
                        ribCntrl.Tabs.Remove(tab);
                        AcApp.SystemVariableChanged -= acadApp_SystemVariableChanged;
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
        static void acadApp_SystemVariableChanged(object sender, SystemVariableChangedEventArgs e)
        {
            if (e.Name.Equals("WSCURRENT")) BuildRibbon();
        }
        private static void CreateRibbon()
        {
            try
            {
                var ribCntrl = ComponentManager.Ribbon;
                // add the tab
                var ribTab = new RibbonTab { Title = "ModPlus ЕСКД", Id = "ModPlus_ESKD" };
                ribCntrl.Tabs.Add(ribTab);
                // add content
                AddAxisPanel(ribTab);
                AddLinesPanel(ribTab);
                // add settings panel
                AddSettingsPanel(ribTab);
                ////////////////////////
                ribCntrl.UpdateLayout();
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
            var ribSourcePanel = new RibbonPanelSource { Title = "Оси" };
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
            ribRowPanel.Items.Add(ribBtn);
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
            var ribSourcePanel = new RibbonPanelSource { Title = "Линии" };
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
            risSplitBtn.Items.Add(ribBtn);
            risSplitBtn.Current = ribBtn;
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
                Title = "Настройки"
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
                    "Стили и настройки",
                    "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Resources/StyleEditor_32x32.png",
                    "Работа со стилями примитивов ModPlus", Orientation.Vertical, "", ""
                ));
            ribRowPanel.Items.Add(
                RibbonHelpers.AddBigButton(
                    "mpPropertiesPalette",
                    "Палитра\nсвойств",
                    "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Resources/Properties_32x32.png",
                    "Палитра свойств примитивов ModPlus", Orientation.Vertical, "", ""
                ));
            ribSourcePanel.Items.Add(ribRowPanel);
        }

        private static string GetBigIconForFunction(string functionName, string subFunctionName)
        {
            return "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Functions/" +
                   functionName + "/Icons/" + subFunctionName + "_32x32.png";
        }
        private static string GetSmallIconForFunction(string functionName, string subFunctionName)
        {
            return "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Functions/" +
                   functionName + "/Icons/" + subFunctionName + "_16x16.png";
        }

        private static string GetHelpImageForFunction(string functionName, string imgName)
        {
            return "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Functions/" +
                   functionName + "/Help/" + imgName;
        }
    }
}
