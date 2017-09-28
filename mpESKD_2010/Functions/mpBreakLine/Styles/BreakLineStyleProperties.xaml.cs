using System.Collections.Generic;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using mpESKD.Base.Helpers;
using mpESKD.Base.Styles;
using mpESKD.Functions.mpBreakLine.Properties;

namespace mpESKD.Functions.mpBreakLine.Styles
{
    public partial class BreakLineStyleProperties
    {
        public BreakLineStyleProperties()
        {
            InitializeComponent();
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
        }
        private void FrameworkElement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement fe)) return;
            if (fe.Name.Equals("TbOverhang"))
                StyleEditorWork.ShowDescription(mpBreakLineProperties.OverhangPropertyDescriptive.Description);
            if (fe.Name.Equals("TbBreakHeight"))
                StyleEditorWork.ShowDescription(mpBreakLineProperties.BreakHeightPropertyDescriptive.Description);
            if (fe.Name.Equals("TbBreakWidth"))
                StyleEditorWork.ShowDescription(mpBreakLineProperties.BreakWidthPropertyDescriptive.Description);
            if (fe.Name.Equals("CbBreakLineType"))
                StyleEditorWork.ShowDescription(mpBreakLineProperties.BreakLineTypePropertyDescriptive.Description);
            if (fe.Name.Equals("CbScale"))
                StyleEditorWork.ShowDescription(mpBreakLineProperties.ScalePropertyDescriptive.Description);
            if (fe.Name.Equals("TbLineTypeScale"))
                StyleEditorWork.ShowDescription(mpBreakLineProperties.LineTypeScalePropertyDescriptive.Description);
        }

        private void FrameworkElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            StyleEditorWork.ShowDescription(string.Empty);
        }
    }
}
