using System;
using mpESKD.Base.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
// ReSharper disable InconsistentNaming

namespace mpESKD.Functions.mpBreakLine.Properties
{
    public partial class mpBreakLinePropertiesPalette
    {
        public mpBreakLinePropertiesPalette()
        {
            InitializeComponent();
            CbBreakLineType.ItemsSource = mpBreakLinePropertiesHelpers.BreakLineTypeLocalNames;
            // get list of scales
            var scales = new List<string>();
            var ocm = AcadHelpers.Database.ObjectContextManager;
            if (ocm != null)
            {
                var occ = ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");
                foreach (ObjectContext objectContext in occ)
                {
                    scales.Add(((AnnotationScale)objectContext).Name);
                }
            }
            CbScale.ItemsSource = scales;
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
                Base.Properties.PropertiesFunction.ShowDescription(mpBreakLineProperties.OverhangPropertyDescriptive.Description);
            if (fe.Name.Equals("TbBreakHeight"))
                Base.Properties.PropertiesFunction.ShowDescription(mpBreakLineProperties.BreakHeightPropertyDescriptive.Description);
            if (fe.Name.Equals("TbBreakWidth"))
                Base.Properties.PropertiesFunction.ShowDescription(mpBreakLineProperties.BreakWidthPropertyDescriptive.Description);
            if (fe.Name.Equals("CbBreakLineType"))
                Base.Properties.PropertiesFunction.ShowDescription(mpBreakLineProperties.BreakLineTypePropertyDescriptive.Description);
            if (fe.Name.Equals("CbScale"))
                Base.Properties.PropertiesFunction.ShowDescription(mpBreakLineProperties.ScalePropertyDescriptive.Description);
            if (fe.Name.Equals("TbLineTypeScale"))
                Base.Properties.PropertiesFunction.ShowDescription(mpBreakLineProperties.LineTypeScalePropertyDescriptive.Description);
        }

        private void FrameworkElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            Base.Properties.PropertiesFunction.ShowDescription(String.Empty);
        }
    }
}
