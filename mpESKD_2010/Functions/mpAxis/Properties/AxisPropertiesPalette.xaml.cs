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
using Visibility = System.Windows.Visibility;

namespace mpESKD.Functions.mpAxis.Properties
{
    public partial class AxisPropertiesPalette
    {
        private readonly PropertiesPalette _parentPalette;

        public AxisPropertiesPalette(PropertiesPalette palette)
        {
            _parentPalette = palette;
            InitializeComponent();
            CbMarkersPosition.ItemsSource = AxisPropertiesHelpers.BreakLineTypeLocalNames;
            // get list of scales
            CbScale.ItemsSource = AcadHelpers.Scales;
            // fill layers
            CbLayerName.ItemsSource = AcadHelpers.Layers;
            // fill text styles
            CbTextStyle.ItemsSource = AcadHelpers.TextStyles;
            // marker types
            var markerTypes = new List<string> { "Тип 1", "Тип 2" };
            CbFirstMarkerType.ItemsSource = markerTypes;
            CbSecondMarkerType.ItemsSource = markerTypes;
            CbThirdMarkerType.ItemsSource = markerTypes;
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
                            if (ExtendedDataHelpers.IsApplicable(obj, AxisFunction.MPCOEntName, true))
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
                    ChangeMarkersTypesVisibility(maxCount);
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
                _parentPalette.ShowDescription("Стиль интеллектуального примитива");
            if (fe.Name.Equals("TbMarkersCount"))
                _parentPalette.ShowDescription(AxisProperties.MarkersCountPropertyDescriptive.Description);
            if (fe.Name.Equals("TbMarkersDiameter"))
                _parentPalette.ShowDescription(AxisProperties.MarkersDiameterPropertyDescriptive.Description);
            if (fe.Name.Equals("TbFracture"))
                _parentPalette.ShowDescription(AxisProperties.FracturePropertyDescriptive.Description);
            if (fe.Name.Equals("TbBottomFractureOffset"))
                _parentPalette.ShowDescription(AxisProperties.BottomFractureOffsetPropertyDescriptive.Description);
            if (fe.Name.Equals("TbTopFractureOffset"))
                _parentPalette.ShowDescription(AxisProperties.TopFractureOffsetPropertyDescriptive.Description);
            if (fe.Name.Equals("CbMarkersPosition"))
                _parentPalette.ShowDescription(AxisProperties.MarkersPositionPropertyDescriptive.Description);
            if (fe.Name.Equals("CbFirstMarkerType"))
                _parentPalette.ShowDescription(AxisProperties.FirstMarkerTypePropertyDescriptive.Description);
            if (fe.Name.Equals("CbSecondMarkerType"))
                _parentPalette.ShowDescription(AxisProperties.SecondMarkerTypePropertyDescriptive.Description);
            if (fe.Name.Equals("CbThirdMarkerType"))
                _parentPalette.ShowDescription(AxisProperties.ThirdMarkerTypePropertyDescriptive.Description);
            if (fe.Name.Equals("CbScale"))
                _parentPalette.ShowDescription(AxisProperties.ScalePropertyDescriptive.Description);
            if (fe.Name.Equals("TbLineTypeScale"))
                _parentPalette.ShowDescription(AxisProperties.LineTypeScalePropertyDescriptive.Description);
            if (fe.Name.Equals("TbLineType"))
                _parentPalette.ShowDescription(AxisProperties.LineTypePropertyDescriptive.Description);
            if (fe.Name.Equals("CbLayerName"))
                _parentPalette.ShowDescription(AxisProperties.LayerName.Description);
            if (fe.Name.Equals("CbTextStyle"))
                _parentPalette.ShowDescription(AxisProperties.TextStylePropertyDescriptive.Description);
            if (fe.Name.Equals("TbTextHeight"))
                _parentPalette.ShowDescription(AxisProperties.TextHeightPropertyDescriptive.Description);
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

        private void ChangeMarkersTypesVisibility(int markerCount)
        {
            switch (markerCount)
            {
                case 1:
                    TbFirstMarkerTypeHeader.Visibility = CbFirstMarkerType.Visibility = Visibility.Visible;
                    TbSecondMarkerTypeHeader.Visibility = CbSecondMarkerType.Visibility = Visibility.Collapsed;
                    TbThirdMarkerTypeHeader.Visibility = CbThirdMarkerType.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    TbFirstMarkerTypeHeader.Visibility = CbFirstMarkerType.Visibility = Visibility.Visible;
                    TbSecondMarkerTypeHeader.Visibility = CbSecondMarkerType.Visibility = Visibility.Visible;
                    TbThirdMarkerTypeHeader.Visibility = CbThirdMarkerType.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    TbFirstMarkerTypeHeader.Visibility = CbFirstMarkerType.Visibility = Visibility.Visible;
                    TbSecondMarkerTypeHeader.Visibility = CbSecondMarkerType.Visibility = Visibility.Visible;
                    TbThirdMarkerTypeHeader.Visibility = CbThirdMarkerType.Visibility = Visibility.Visible;
                    break;
            }
        }
        
        private void TbMarkersCount_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is IntTextBox itb)
            {
                if (int.TryParse(itb.Value.ToString(), out int i))
                    ChangeMarkersTypesVisibility(i);
            }
        }
    }
}
