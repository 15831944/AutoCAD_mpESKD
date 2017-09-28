using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using mpESKD.Base.Helpers;
using ModPlusAPI;
using ModPlusAPI.Windows;

namespace mpESKD.Base.Properties
{
    public partial class PropertiesPalette
    {
        public PropertiesPalette()
        {
            InitializeComponent();
            AcadHelpers.Documents.DocumentCreated += Documents_DocumentCreated;
            AcadHelpers.Documents.DocumentActivated += Documents_DocumentActivated;
            foreach (Document document in AcadHelpers.Documents)
            {
                //document.ImpliedSelectionChanged -= Document_ImpliedSelectionChanged;
                document.ImpliedSelectionChanged += Document_ImpliedSelectionChanged;
            }
            if (AcadHelpers.Document != null)
                ShowPropertiesControlsBySelection();
        }

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
            ShowPropertiesControlsBySelection();
        }
        /// <summary>Добавление пользовательских элементов в палитру в зависимости от выбранных объектов</summary>
        private void ShowPropertiesControlsBySelection()
        {
            PromptSelectionResult psr = AcadHelpers.Editor.SelectImplied();
            if (psr.Value == null || psr.Value.Count == 0)
            {
                // Удаляем контролы свойств
                if (StackPanelProperties.Children.Count > 0)
                    StackPanelProperties.Children.Clear();
                // Очищаем панель описания
                PropertiesFunction.ShowDescription(String.Empty);
            }
            else
            {
                foreach (SelectedObject selectedObject in psr.Value)
                {
                    using (OpenCloseTransaction tr = new OpenCloseTransaction())
                    {
                        var obj = tr.GetObject(selectedObject.ObjectId, OpenMode.ForRead);
                        if (obj is BlockReference)
                        {
                            if (ExtendedDataHelpers.IsApplicable(obj, Functions.mpBreakLine.BreakLineFunction.MPCOEntName))
                            {
                                // mpBreakLine
                                if (!HasPropertyControl(Functions.mpBreakLine.BreakLineFunction.MPCOEntName))
                                {
                                    var mpBreakLineProperties = new mpESKD.Functions.mpBreakLine.Properties.mpBreakLinePropertiesPalette
                                    {
                                        Name = Functions.mpBreakLine.BreakLineFunction.MPCOEntName
                                    };
                                    StackPanelProperties.Children.Add(mpBreakLineProperties);
                                }
                            }
                        }
                    }
                }
            }
        }
        private bool HasPropertyControl(string name)
        {
            foreach (object child in StackPanelProperties.Children)
            {
                if (child is FrameworkElement)
                    if ((child as FrameworkElement).Name.Equals(name))
                        return true;
            }
            return false;
        }

        private void LmSettings_OnClick(object sender, RoutedEventArgs e)
        {
            PaletteSettings lmSetting = new PaletteSettings()
            {
                Topmost = true
            };
            lmSetting.ShowDialog();
            
            if (!lmSetting.ChkAddToMpPalette.IsChecked ?? true)
            {
                MainFunction.RemoveFromMpPalette(true);
            }
            else
            {
                MainFunction.AddToMpPalette(true);
            }
        }
    }

    public static class PropertiesFunction
    {
        public static PaletteSet _paletteSet;
        private static PropertiesPalette propertiesPalette;
        [CommandMethod("ModPlus", "mpPropertiesPalette", CommandFlags.Modal)]
        public static void Start()
        {
            try
            {
                if (!(!bool.TryParse(
                          UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", "AddToMpPalette"),
                          out bool b) | b))
                {
                    MainFunction.RemoveFromMpPalette(false);
                    if (_paletteSet != null)
                    {
                        _paletteSet.Visible = true;
                    }
                    else
                    {
                        _paletteSet = new PaletteSet("Свойства примитивов ModPlus", "mpPropertiesPalette",
                            new Guid("1c0dc0f7-0d06-49df-a2d3-bcea4241e036"));
                        _paletteSet.Load += _paletteSet_Load;
                        _paletteSet.Save += _paletteSet_Save;
                        propertiesPalette = new PropertiesPalette();
                        ElementHost elementHost = new ElementHost()
                        {
                            AutoSize = true,
                            Dock = DockStyle.Fill,
                            Child = propertiesPalette
                        };
                        _paletteSet.Add("Свойства примитивов ModPlus", elementHost);
                        _paletteSet.Style = PaletteSetStyles.ShowCloseButton | PaletteSetStyles.ShowPropertiesMenu |
                                            PaletteSetStyles.ShowAutoHideButton;
                        _paletteSet.MinimumSize = new System.Drawing.Size(100, 300);
                        _paletteSet.DockEnabled = DockSides.Right | DockSides.Left;
                        _paletteSet.Visible = true;
                    }
                }
                else
                {
                    if (_paletteSet != null)
                    {
                        _paletteSet.Visible = false;
                    }
                    MainFunction.AddToMpPalette(true);
                }
            }
            catch (System.Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
        private static void _paletteSet_Load(object sender, PalettePersistEventArgs e)
        {
            double num = (double)e.ConfigurationSection.ReadProperty("mpPropertiesPalette", 22.3);
        }

        private static void _paletteSet_Save(object sender, PalettePersistEventArgs e)
        {
            e.ConfigurationSection.WriteProperty("mpPropertiesPalette", 32.3);
        }

        public static void ShowDescription(string description)
        {
            if (propertiesPalette != null)
            {
                propertiesPalette.TbDescription.Text = description;
            }
        }
    }
}
