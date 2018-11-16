using System.Windows;
using mpESKD.Base.Helpers;
using mpESKD.Base.Styles;

namespace mpESKD.Functions.mpBreakLine.Styles
{
    public partial class BreakLineStyleProperties
    {
        public BreakLineStyleProperties(string layerNameFromStyle)
        {
            InitializeComponent();
            ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources);
            ModPlusAPI.Windows.Helpers.WindowHelpers.ChangeStyleForResourceDictionary(Resources);
            // get list of scales
            CbScale.ItemsSource = AcadHelpers.Scales;
            // layers
            var layers = AcadHelpers.Layers;
            layers.Insert(0, ModPlusAPI.Language.GetItem(MainFunction.LangItem, "defl")); // "По умолчанию"
            if (!layers.Contains(layerNameFromStyle))
                layers.Insert(1, layerNameFromStyle);
            CbLayerName.ItemsSource = layers;
        }
        private void FrameworkElement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void FrameworkElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            StyleEditorWork.ShowDescription(string.Empty);
        }
    }
}
