using System.Windows;
using mpESKD.Base.Helpers;
using mpESKD.Base.Styles;
using mpESKD.Functions.mpBreakLine.Properties;

namespace mpESKD.Functions.mpBreakLine.Styles
{
    public partial class BreakLineStyleProperties
    {
        public BreakLineStyleProperties(string layerNameFromStyle)
        {
            InitializeComponent();
            // get list of scales
            CbScale.ItemsSource = AcadHelpers.Scales;
            // layers
            var layers = AcadHelpers.Layers;
            layers.Insert(0, "По умолчанию");
            if(!layers.Contains(layerNameFromStyle))
                layers.Insert(1, layerNameFromStyle);
            CbLayerName.ItemsSource = layers;
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
            if (fe.Name.Equals("CbLayerName"))
                StyleEditorWork.ShowDescription(mpBreakLineProperties.LayerName.Description);
        }

        private void FrameworkElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            StyleEditorWork.ShowDescription(string.Empty);
        }
    }
}
