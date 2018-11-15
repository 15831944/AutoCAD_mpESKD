using System;
using mpESKD.Base.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using mpESKD.Base.Properties;
using mpESKD.Functions.mpBreakLine.Styles;

// ReSharper disable InconsistentNaming

namespace mpESKD.Functions.mpBreakLine.Properties
{
    using Base.Enums;
    using Base.Styles;

    public partial class BreakLinePropertiesPalette
    {
        private readonly PropertiesPalette _parentPalette;

        public BreakLinePropertiesPalette(PropertiesPalette palette)
        {
            _parentPalette = palette;
            InitializeComponent();
            ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources);
            // styles
            CbStyle.ItemsSource = StyleManager.GetStyles<BreakLineStyle>().Select(s => s.Name);

            CbBreakLineType.ItemsSource = BreakLineTypeHelper.LocalNames;
            // get list of scales
            CbScale.ItemsSource = AcadHelpers.Scales;
            // fill layers
            CbLayerName.ItemsSource = AcadHelpers.Layers;
            if (AcadHelpers.Document != null)
            {
                ShowProperties();
                AcadHelpers.Document.ImpliedSelectionChanged += Document_ImpliedSelectionChanged;
            }
        }

        private void Document_ImpliedSelectionChanged(object sender, EventArgs e)
        {
           ShowProperties();
        }

        private BreakLineSummaryProperties _breakLineSummaryProperties;
        private void ShowProperties()
        {
            PromptSelectionResult psr = AcadHelpers.Editor.SelectImplied();

            if (psr.Status != PromptStatus.OK || psr.Value == null || psr.Value.Count == 0)
            {
                _breakLineSummaryProperties = null;
            }
            else
            {
                List<ObjectId> objectIds = new List<ObjectId>();
                foreach (SelectedObject selectedObject in psr.Value)
                {
                    using (OpenCloseTransaction tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var obj = tr.GetObject(selectedObject.ObjectId, OpenMode.ForRead);
                        if (obj is BlockReference)
                        {
                            if (ExtendedDataHelpers.IsApplicable(obj, BreakLineInterface.Name))
                            {
                                objectIds.Add(selectedObject.ObjectId);
                            }
                        }
                    }
                }
                if (objectIds.Any())
                {
                    Expander.Header = BreakLineInterface.LName + " (" + objectIds.Count + ")";
                    _breakLineSummaryProperties = new BreakLineSummaryProperties(objectIds);
                    SetData(_breakLineSummaryProperties);
                }
            }
        }
        public void SetData(BreakLineSummaryProperties data)
        {
            DataContext = data;
        }

        private void FrameworkElement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if(!(sender is FrameworkElement fe)) return;
            if (fe.Name.Equals("CbStyle"))
                _parentPalette.ShowDescription(ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h52"));
            if (fe.Name.Equals("TbOverhang"))
                _parentPalette.ShowDescription(BreakLineProperties.Overhang.Description);
            if (fe.Name.Equals("TbBreakHeight"))
                _parentPalette.ShowDescription(BreakLineProperties.BreakHeight.Description);
            if (fe.Name.Equals("TbBreakWidth"))
                _parentPalette.ShowDescription(BreakLineProperties.BreakWidth.Description);
            if (fe.Name.Equals("CbBreakLineType"))
                _parentPalette.ShowDescription(BreakLineProperties.BreakLineType.Description);
            if (fe.Name.Equals("CbScale"))
                _parentPalette.ShowDescription(BreakLineProperties.Scale.Description);
            if (fe.Name.Equals("TbLineTypeScale"))
                _parentPalette.ShowDescription(BreakLineProperties.LineTypeScale.Description);
            if (fe.Name.Equals("CbLayerName"))
                _parentPalette.ShowDescription(BreakLineProperties.LayerName.Description);
        }

        private void FrameworkElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            _parentPalette.ShowDescription(String.Empty);
        }

        private void BreakLinePropertiesPalette_OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (AcadHelpers.Document != null)
                AcadHelpers.Document.ImpliedSelectionChanged -= Document_ImpliedSelectionChanged;
        }
    }
}
