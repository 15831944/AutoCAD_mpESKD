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
    public partial class mpBreakLinePropertiesPalette
    {
        private PropertiesPalette _parentPalette;

        public mpBreakLinePropertiesPalette(PropertiesPalette palette)
        {
            _parentPalette = palette;
            InitializeComponent();
            CbBreakLineType.ItemsSource = mpBreakLinePropertiesHelpers.BreakLineTypeLocalNames;
            // get list of scales
            CbScale.ItemsSource = AcadHelpers.Scales;
            // fill layers
            CbLayerName.ItemsSource = AcadHelpers.Layers;
            if (AcadHelpers.Document != null)
                ShowProperties();
        }

        private mpBreakLineSummaryProperties mpBreakLineSummaryProperties;
        private void ShowProperties()
        {
            PromptSelectionResult psr = AcadHelpers.Editor.SelectImplied();

            if (psr.Status != PromptStatus.OK || psr.Value == null || psr.Value.Count == 0)
            {
                mpBreakLineSummaryProperties = null;
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
                            if (ExtendedDataHelpers.IsApplicable(obj, BreakLineFunction.MPCOEntName))
                            {
                                objectIds.Add(selectedObject.ObjectId);
                            }
                        }
                    }
                }
                if (objectIds.Any())
                {
                    Expander.Header = BreakLineFunction.MPCOEntDisplayName + " (" + objectIds.Count + ")";
                    mpBreakLineSummaryProperties = new mpBreakLineSummaryProperties(objectIds);
                    SetData(mpBreakLineSummaryProperties);
                }
            }
        }
        public void SetData(mpBreakLineSummaryProperties data)
        {
            DataContext = data;
        }

        private void FrameworkElement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if(!(sender is FrameworkElement fe)) return;
            if (fe.Name.Equals("TbOverhang"))
                _parentPalette.ShowDescription(mpBreakLineProperties.OverhangPropertyDescriptive.Description);
            if (fe.Name.Equals("TbBreakHeight"))
                _parentPalette.ShowDescription(mpBreakLineProperties.BreakHeightPropertyDescriptive.Description);
            if (fe.Name.Equals("TbBreakWidth"))
                _parentPalette.ShowDescription(mpBreakLineProperties.BreakWidthPropertyDescriptive.Description);
            if (fe.Name.Equals("CbBreakLineType"))
                _parentPalette.ShowDescription(mpBreakLineProperties.BreakLineTypePropertyDescriptive.Description);
            if (fe.Name.Equals("CbScale"))
                _parentPalette.ShowDescription(mpBreakLineProperties.ScalePropertyDescriptive.Description);
            if (fe.Name.Equals("TbLineTypeScale"))
                _parentPalette.ShowDescription(mpBreakLineProperties.LineTypeScalePropertyDescriptive.Description);
            if (fe.Name.Equals("CbLayerName"))
                _parentPalette.ShowDescription(mpBreakLineProperties.LayerName.Description);
        }

        private void FrameworkElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            _parentPalette.ShowDescription(String.Empty);
        }
    }
}
