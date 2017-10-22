using System;
using mpESKD.Base.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using mpESKD.Base.Properties;

// ReSharper disable InconsistentNaming

namespace mpESKD.Functions.mpBreakLine.Properties
{
    public partial class BreakLinePropertiesPalette
    {
        private readonly PropertiesPalette _parentPalette;

        public BreakLinePropertiesPalette(PropertiesPalette palette)
        {
            _parentPalette = palette;
            InitializeComponent();
            CbBreakLineType.ItemsSource = BreakLinePropertiesHelpers.BreakLineTypeLocalNames;
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
                            if (ExtendedDataHelpers.IsApplicable(obj, BreakLineFunction.MPCOEntName, true))
                            {
                                objectIds.Add(selectedObject.ObjectId);
                            }
                        }
                    }
                }
                if (objectIds.Any())
                {
                    Expander.Header = BreakLineFunction.MPCOEntDisplayName + " (" + objectIds.Count + ")";
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
            if (fe.Name.Equals("TbOverhang"))
                _parentPalette.ShowDescription(BreakLineProperties.OverhangPropertyDescriptive.Description);
            if (fe.Name.Equals("TbBreakHeight"))
                _parentPalette.ShowDescription(BreakLineProperties.BreakHeightPropertyDescriptive.Description);
            if (fe.Name.Equals("TbBreakWidth"))
                _parentPalette.ShowDescription(BreakLineProperties.BreakWidthPropertyDescriptive.Description);
            if (fe.Name.Equals("CbBreakLineType"))
                _parentPalette.ShowDescription(BreakLineProperties.BreakLineTypePropertyDescriptive.Description);
            if (fe.Name.Equals("CbScale"))
                _parentPalette.ShowDescription(BreakLineProperties.ScalePropertyDescriptive.Description);
            if (fe.Name.Equals("TbLineTypeScale"))
                _parentPalette.ShowDescription(BreakLineProperties.LineTypeScalePropertyDescriptive.Description);
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
