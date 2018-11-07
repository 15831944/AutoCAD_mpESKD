﻿using System.Windows;
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
            if (!(sender is FrameworkElement fe)) return;
            if (fe.Name.Equals("TbOverhang"))
                StyleEditorWork.ShowDescription(BreakLineProperties.Overhang.Description);
            if (fe.Name.Equals("TbBreakHeight"))
                StyleEditorWork.ShowDescription(BreakLineProperties.BreakHeight.Description);
            if (fe.Name.Equals("TbBreakWidth"))
                StyleEditorWork.ShowDescription(BreakLineProperties.BreakWidth.Description);
            if (fe.Name.Equals("CbBreakLineType"))
                StyleEditorWork.ShowDescription(BreakLineProperties.BreakLineType.Description);
            if (fe.Name.Equals("CbScale"))
                StyleEditorWork.ShowDescription(BreakLineProperties.Scale.Description);
            if (fe.Name.Equals("TbLineTypeScale"))
                StyleEditorWork.ShowDescription(BreakLineProperties.LineTypeScale.Description);
            if (fe.Name.Equals("CbLayerName"))
                StyleEditorWork.ShowDescription(BreakLineProperties.LayerName.Description);
        }

        private void FrameworkElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            StyleEditorWork.ShowDescription(string.Empty);
        }
    }
}