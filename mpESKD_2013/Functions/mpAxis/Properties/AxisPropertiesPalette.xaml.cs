using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using mpESKD.Base.Helpers;
using mpESKD.Base.Properties;
using mpESKD.Base.Properties.Controls;
using mpESKD.Functions.mpAxis.Styles;
using Visibility = System.Windows.Visibility;

namespace mpESKD.Functions.mpAxis.Properties
{
    public partial class AxisPropertiesPalette
    {
        private const string LangItem = "mpESKD";
        private readonly PropertiesPalette _parentPalette;

        public AxisPropertiesPalette(PropertiesPalette palette)
        {
            _parentPalette = palette;
            InitializeComponent();
            ModPlusAPI.Language.SetLanguageProviderForWindow(Resources);
            // styles
            var sNames = new List<string>();
            foreach (var style in AxisStyleManager.Styles)
            {
                sNames.Add(style.Name);
            }
            CbStyle.ItemsSource = sNames;
            // markers positions
            CbMarkersPosition.ItemsSource = AxisPropertiesHelpers.AxisMarkersTypeLocalNames;
            // get list of scales
            CbScale.ItemsSource = AcadHelpers.Scales;
            // fill layers
            CbLayerName.ItemsSource = AcadHelpers.Layers;
            // fill text styles
            CbTextStyle.ItemsSource = AcadHelpers.TextStyles;
            // marker types
            var markerTypes = new List<string>
            {
                ModPlusAPI.Language.GetItem(LangItem, "type1"), // "Тип 1",
                ModPlusAPI.Language.GetItem(LangItem, "type2") //"Тип 2"
            };
            CbFirstMarkerType.ItemsSource = markerTypes;
            CbSecondMarkerType.ItemsSource = markerTypes;
            CbThirdMarkerType.ItemsSource = markerTypes;
            CbOrientMarkerType.ItemsSource = markerTypes;
            // get data
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
        private AxisSummaryProperties _axisSummaryProperties;
        private void ShowProperties()
        {
            PromptSelectionResult psr = AcadHelpers.Editor.SelectImplied();

            if (psr.Status != PromptStatus.OK || psr.Value == null || psr.Value.Count == 0)
            {
                _axisSummaryProperties = null;
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
                            if (ExtendedDataHelpers.IsApplicable(obj, AxisFunction.MPCOEntName))
                            {
                                objectIds.Add(selectedObject.ObjectId);
                            }
                        }
                    }
                }
                if (objectIds.Any())
                {
                    Expander.Header = AxisFunction.MPCOEntDisplayName + " (" + objectIds.Count + ")";
                    _axisSummaryProperties = new AxisSummaryProperties(objectIds, out int maxCount);
                    ChangeVisibilityByMarkerCount(maxCount);
                    SetData(_axisSummaryProperties);
                }
            }
        }
        public void SetData(AxisSummaryProperties data)
        {
            DataContext = data;
        }

        private void FrameworkElement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement fe)) return;
            if (fe.Name.Equals("CbStyle"))
                _parentPalette.ShowDescription(ModPlusAPI.Language.GetItem(LangItem, "h52")); // "Стиль интеллектуального примитива"
            if (fe.Name.Equals("TbMarkersCount"))
                _parentPalette.ShowDescription(AxisProperties.MarkersCount.Description);
            if (fe.Name.Equals("TbMarkersDiameter"))
                _parentPalette.ShowDescription(AxisProperties.MarkersDiameter.Description);
            if (fe.Name.Equals("TbFracture"))
                _parentPalette.ShowDescription(AxisProperties.Fracture.Description);
            if (fe.Name.Equals("TbBottomFractureOffset"))
                _parentPalette.ShowDescription(AxisProperties.BottomFractureOffset.Description);
            if (fe.Name.Equals("TbTopFractureOffset"))
                _parentPalette.ShowDescription(AxisProperties.TopFractureOffset.Description);
            if (fe.Name.Equals("CbMarkersPosition"))
                _parentPalette.ShowDescription(AxisProperties.MarkersPosition.Description);
            if (fe.Name.Equals("CbFirstMarkerType"))
                _parentPalette.ShowDescription(AxisProperties.FirstMarkerType.Description);
            if (fe.Name.Equals("CbSecondMarkerType"))
                _parentPalette.ShowDescription(AxisProperties.SecondMarkerType.Description);
            if (fe.Name.Equals("CbThirdMarkerType"))
                _parentPalette.ShowDescription(AxisProperties.ThirdMarkerType.Description);
            if (fe.Name.Equals("TbArrowSize"))
                _parentPalette.ShowDescription(AxisProperties.ArrowsSize.Description);
            if (fe.Name.Equals("CbScale"))
                _parentPalette.ShowDescription(AxisProperties.Scale.Description);
            if (fe.Name.Equals("TbLineTypeScale"))
                _parentPalette.ShowDescription(AxisProperties.LineTypeScale.Description);
            if (fe.Name.Equals("TbLineType"))
                _parentPalette.ShowDescription(AxisProperties.LineType.Description);
            if (fe.Name.Equals("CbLayerName"))
                _parentPalette.ShowDescription(AxisProperties.LayerName.Description);
            if (fe.Name.Equals("CbTextStyle"))
                _parentPalette.ShowDescription(AxisProperties.TextStyle.Description);
            if (fe.Name.Equals("TbTextHeight"))
                _parentPalette.ShowDescription(AxisProperties.TextHeight.Description);
            if (fe.Name.Equals("TbFirstTextPrefix"))
                _parentPalette.ShowDescription(AxisProperties.FirstTextPrefix.Description);
            if (fe.Name.Equals("TbFirstText"))
                _parentPalette.ShowDescription(AxisProperties.FirstText.Description);
            if (fe.Name.Equals("TbFirstTextSuffix"))
                _parentPalette.ShowDescription(AxisProperties.FirstTextSuffix.Description);
            if (fe.Name.Equals("TbSecondTextPrefix"))
                _parentPalette.ShowDescription(AxisProperties.SecondTextPrefix.Description);
            if (fe.Name.Equals("TbSecondText"))
                _parentPalette.ShowDescription(AxisProperties.SecondText.Description);
            if (fe.Name.Equals("TbSecondTextSuffix"))
                _parentPalette.ShowDescription(AxisProperties.SecondTextSuffix.Description);
            if (fe.Name.Equals("TbThirdTextPrefix"))
                _parentPalette.ShowDescription(AxisProperties.ThirdTextPrefix.Description);
            if (fe.Name.Equals("TbThirdText"))
                _parentPalette.ShowDescription(AxisProperties.ThirdText.Description);
            if (fe.Name.Equals("TbThirdTextSuffix"))
                _parentPalette.ShowDescription(AxisProperties.ThirdTextSuffix.Description);
            if (fe.Name.Equals("TbArrowsSize"))
                _parentPalette.ShowDescription(AxisProperties.ArrowsSize.Description);
            if (fe.Name.Equals("ChkBottomOrientMarkerVisible"))
                _parentPalette.ShowDescription(AxisProperties.BottomOrientMarkerVisible.Description);
            if (fe.Name.Equals("ChkTopOrientMarkerVisible"))
                _parentPalette.ShowDescription(AxisProperties.TopOrientMarkerVisible.Description);
            if (fe.Name.Equals("CbOrientMarkerType"))
                _parentPalette.ShowDescription(AxisProperties.OrientMarkerType.Description);
            if (fe.Name.Equals("TbBottomOrientText"))
                _parentPalette.ShowDescription(AxisProperties.BottomOrientText.Description);
            if (fe.Name.Equals("TbTopOrientText"))
                _parentPalette.ShowDescription(AxisProperties.TopOrientText.Description);
        }

        private void FrameworkElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            _parentPalette.ShowDescription(String.Empty);
        }

        private void AxisPropertiesPalette_OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (AcadHelpers.Document != null)
                AcadHelpers.Document.ImpliedSelectionChanged -= Document_ImpliedSelectionChanged;
        }
        // set line type
        private void TbLineType_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            using (AcadHelpers.Document.LockDocument())
            {
                var ltd = new LinetypeDialog { IncludeByBlockByLayer = false };
                if (ltd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!ltd.Linetype.IsNull)
                        using (var tr = AcadHelpers.Document.TransactionManager.StartTransaction())
                        {
                            using (var ltr = tr.GetObject(ltd.Linetype, OpenMode.ForRead) as LinetypeTableRecord)
                            {
                                if (ltr != null)
                                {
                                    //TbLineType.Text = ltr.Name;
                                    ((AxisSummaryProperties) DataContext).LineType = ltr.Name;
                                }
                            }
                            tr.Commit();
                        }
                }
            }
        }

        private void ChangeVisibilityByMarkerCount(int markerCount)
        {
            switch (markerCount)
            {
                case 1:
                    TbSecondMarkerTypeHeader.Visibility = CbSecondMarkerType.Visibility = Visibility.Collapsed;
                    TbThirdMarkerTypeHeader.Visibility = CbThirdMarkerType.Visibility = Visibility.Collapsed;
                    // text
                    TbSecondText.Visibility = Visibility.Collapsed;
                    TbSecondTextPrefix.Visibility = Visibility.Collapsed;
                    TbSecondTextSuffix.Visibility = Visibility.Collapsed;
                    TbThirdText.Visibility = Visibility.Collapsed;
                    TbThirdTextPrefix.Visibility = Visibility.Collapsed;
                    TbThirdTextSuffix.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    TbSecondMarkerTypeHeader.Visibility = CbSecondMarkerType.Visibility = Visibility.Visible;
                    TbThirdMarkerTypeHeader.Visibility = CbThirdMarkerType.Visibility = Visibility.Collapsed;
                    // text
                    TbSecondText.Visibility = Visibility.Visible;
                    TbSecondTextPrefix.Visibility = Visibility.Visible;
                    TbSecondTextSuffix.Visibility = Visibility.Visible;
                    TbThirdText.Visibility = Visibility.Collapsed;
                    TbThirdTextPrefix.Visibility = Visibility.Collapsed;
                    TbThirdTextSuffix.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    TbSecondMarkerTypeHeader.Visibility = CbSecondMarkerType.Visibility = Visibility.Visible;
                    TbThirdMarkerTypeHeader.Visibility = CbThirdMarkerType.Visibility = Visibility.Visible;
                    // text
                    TbSecondText.Visibility = Visibility.Visible;
                    TbSecondTextPrefix.Visibility = Visibility.Visible;
                    TbSecondTextSuffix.Visibility = Visibility.Visible;
                    TbThirdText.Visibility = Visibility.Visible;
                    TbThirdTextPrefix.Visibility = Visibility.Visible;
                    TbThirdTextSuffix.Visibility = Visibility.Visible;
                    break;
            }
        }
        
        private void TbMarkersCount_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is IntTextBox itb)
            {
                if (int.TryParse(itb.Value.ToString(), out int i))
                    ChangeVisibilityByMarkerCount(i);
            }
        }
    }
}
