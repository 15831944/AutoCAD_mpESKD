namespace mpESKD.Base.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.Windows;
    using Functions.mpAxis;
    using Functions.mpBreakLine;
    using Functions.mpGroundLine;
    using Functions.mpSection;
    using ModPlus.Helpers;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

    public class RibbonBuilder
    {
        public static void BuildRibbon()
        {
            if (!IsLoaded())
            {
                GetColorTheme();
                CreateRibbon();
                Application.SystemVariableChanged -= AcadApp_SystemVariableChanged;
                Application.SystemVariableChanged += AcadApp_SystemVariableChanged;
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
                        Application.SystemVariableChanged -= AcadApp_SystemVariableChanged;
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
            try
            {
                var sv = Application.GetSystemVariable("COLORTHEME").ToString();
                if (int.TryParse(sv, out int i))
                    _colorTheme = i;
                else _colorTheme = 1; // light
            }
            catch
            {
                _colorTheme = 1;
            }
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
                AddViewsPanel(ribTab);
                
                // tools 
                AddToolsPanel(ribTab);
                
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

        /// <summary>
        /// Панель "Оси"
        /// </summary>
        private static void AddAxisPanel(RibbonTab ribTab)
        {
            // create the panel source
            var ribSourcePanel = new RibbonPanelSource { Title = Language.GetItem(Invariables.LangItem, "tab3") };
            // now the panel
            var ribPanel = new RibbonPanel { Source = ribSourcePanel };
            ribTab.Panels.Add(ribPanel);

            var ribRowPanel = new RibbonRowPanel();

            // mpAxis
            var ribBtn = GetBigButton(AxisDescriptor.Instance);
            if (ribBtn != null)
                ribRowPanel.Items.Add(ribBtn);

            if (ribRowPanel.Items.Any())
            {
                ribSourcePanel.Items.Add(ribRowPanel);
            }
        }

        /// <summary>
        /// Панель "Линии"
        /// </summary>
        private static void AddLinesPanel(RibbonTab ribTab)
        {
            // create the panel source
            var ribSourcePanel = new RibbonPanelSource { Title = Language.GetItem(Invariables.LangItem, "tab1") };
            // now the panel
            var ribPanel = new RibbonPanel { Source = ribSourcePanel };
            ribTab.Panels.Add(ribPanel);

            var ribRowPanel = new RibbonRowPanel();

            // mpBreakLine
            ribRowPanel.Items.Add(GetSplitButton(BreakLineDescriptor.Instance));

            // mpGroundLine
            ribRowPanel.Items.Add(GetSplitButton(GroundLineDescriptor.Instance));

            if (ribRowPanel.Items.Any())
                ribSourcePanel.Items.Add(ribRowPanel);
        }

        /// <summary>
        /// Панель "Виды, разрезы"
        /// </summary>
        private static void AddViewsPanel(RibbonTab ribTab)
        {
            // create the panel source
            var ribSourcePanel = new RibbonPanelSource { Title = Language.GetItem(Invariables.LangItem, "tab8") };
            var ribPanel = new RibbonPanel { Source = ribSourcePanel };
            ribTab.Panels.Add(ribPanel);

            var ribRowPanel = new RibbonRowPanel();

            // mpSection
            ribRowPanel.Items.Add(GetSplitButton(SectionDescriptor.Instance));

            if (ribRowPanel.Items.Any())
                ribSourcePanel.Items.Add(ribRowPanel);
        }

        /// <summary>
        /// Добавить панель "Утилиты"
        /// </summary>
        private static void AddToolsPanel(RibbonTab ribTab)
        {
            //create the panel source
            var ribSourcePanel = new RibbonPanelSource
            {
                Title = Language.GetItem(Invariables.LangItem, "tab9")
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
                    "mpESKDSearch",
                    Language.GetItem(Invariables.LangItem, "tab10"),
                    _colorTheme == 1 // 1 - light
                        ? "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Resources/SearchEntities_32x32.png"
                        : "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Resources/SearchEntities_32x32_dark.png",
                    Language.GetItem(Invariables.LangItem, "tab5"), Orientation.Vertical, "", "", "help/mpeskd"
                ));
            ribSourcePanel.Items.Add(ribRowPanel);
        }

        /// <summary>
        /// Добавить панель "Настройки"
        /// </summary>
        private static void AddSettingsPanel(RibbonTab ribTab)
        {
            //create the panel source
            var ribSourcePanel = new RibbonPanelSource
            {
                Title = Language.GetItem(Invariables.LangItem, "tab2")
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
                    Language.GetItem(Invariables.LangItem, "tab4"),
                    _colorTheme == 1 // 1 - light
                    ? "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Resources/StyleEditor_32x32.png"
                    : "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Resources/StyleEditor_32x32_dark.png",
                    Language.GetItem(Invariables.LangItem, "tab5"), Orientation.Vertical, "", "", "help/mpeskd"
                ));
            ribRowPanel.Items.Add(
                RibbonHelpers.AddBigButton(
                    "mpPropertiesPalette",
                    ConvertLName(Language.GetItem(Invariables.LangItem, "tab6")),
                    _colorTheme == 1 // 1 - light
                    ? "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Resources/Properties_32x32.png"
                    : "pack://application:,,,/mpESKD_" + MpVersionData.CurCadVers + ";component/Resources/Properties_32x32_dark.png",
                    Language.GetItem(Invariables.LangItem, "tab7"), Orientation.Vertical, "", "", "help/mpeskd"
                ));
            ribSourcePanel.Items.Add(ribRowPanel);
        }

        /// <summary>
        /// Получить SplitButton (основная функция + все подфункции) для дескриптора функции
        /// </summary>
        /// <param name="descriptor">Дескриптор функции - класс, реализующий интерфейс <see cref="IIntellectualEntityDescriptor"/></param>
        /// <param name="orientation">Ориентация кнопки</param>
        /// <param name="size">Размер кнопки</param>
        private static RibbonSplitButton GetSplitButton(
            IIntellectualEntityDescriptor descriptor,
            Orientation orientation = Orientation.Vertical,
            RibbonItemSize size = RibbonItemSize.Large)
        {
            // Создаем SplitButton
            var risSplitBtn = new RibbonSplitButton
            {
                Text = "RibbonSplitButton",
                Orientation = orientation,
                Size = size,
                ShowImage = true,
                ShowText = true,
                ListButtonStyle = Autodesk.Private.Windows.RibbonListButtonStyle.SplitButton,
                ResizeStyle = RibbonItemResizeStyles.NoResize,
                ListStyle = RibbonSplitButtonListStyle.List
            };

            // Добавляем в него первую функцию, которую делаем основной
            var ribBtn = GetBigButton(descriptor, orientation);
            if (ribBtn != null)
            {
                risSplitBtn.Items.Add(ribBtn);
                risSplitBtn.Current = ribBtn;
            }

            // Затем добавляем подфункции
            GetBigButtonsForSubFunctions(descriptor, orientation).ForEach(b => risSplitBtn.Items.Add(b));

            return risSplitBtn;
        }

        /// <summary>
        /// Получить большую кнопку по дескриптору функции. Возвращает кнопку для основной функции в дескрипторе
        /// </summary>
        /// <param name="descriptor">Дескриптор функции - класс, реализующий интерфейс <see cref="IIntellectualEntityDescriptor"/></param>
        /// <param name="orientation">Ориентация кнопки</param>
        private static RibbonButton GetBigButton(IIntellectualEntityDescriptor descriptor, Orientation orientation = Orientation.Vertical)
        {
            return RibbonHelpers.AddBigButton(
                descriptor.Name,
                descriptor.LName,
                GetBigIconForFunction(descriptor.Name, descriptor.Name),
                descriptor.Description,
                orientation,
                descriptor.FullDescription,
                GetHelpImageForFunction(descriptor.Name, descriptor.ToolTipHelpImage),
                "help/mpeskd"
            );
        }

        /// <summary>
        /// Получить список больших кнопок для подфункций функции по дескриптору
        /// </summary>
        /// <param name="descriptor">Дескриптор функции - класс, реализующий интерфейс <see cref="IIntellectualEntityDescriptor"/></param>
        /// <param name="orientation">Ориентация кнопки</param>
        private static List<RibbonButton> GetBigButtonsForSubFunctions(IIntellectualEntityDescriptor descriptor, Orientation orientation = Orientation.Vertical)
        {
            List<RibbonButton> buttons = new List<RibbonButton>();

            for (int i = 0; i < descriptor.SubFunctionsNames.Count; i++)
            {
                buttons.Add(RibbonHelpers.AddBigButton(
                    descriptor.SubFunctionsNames[i],
                    descriptor.SubFunctionsLNames[i],
                    GetBigIconForFunction(descriptor.Name, descriptor.SubFunctionsNames[i]),
                    descriptor.SubDescriptions[i],
                    orientation,
                    descriptor.SubFullDescriptions[i],
                    GetHelpImageForFunction(descriptor.Name, descriptor.SubHelpImages[i]),
                    "help/mpeskd"
                ));
            }

            return buttons;
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
