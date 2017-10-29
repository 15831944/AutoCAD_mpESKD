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
                    _axisSummaryProperties = new AxisSummaryProperties(objectIds);
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
            if (fe.Name.Equals("CbScale"))
                _parentPalette.ShowDescription(AxisProperties.ScalePropertyDescriptive.Description);
            if (fe.Name.Equals("TbLineTypeScale"))
                _parentPalette.ShowDescription(AxisProperties.LineTypeScalePropertyDescriptive.Description);
            if (fe.Name.Equals("TbLineType"))
                _parentPalette.ShowDescription(AxisProperties.LineTypePropertyDescriptive.Description);
            if (fe.Name.Equals("CbLayerName"))
                _parentPalette.ShowDescription(AxisProperties.LayerName.Description);
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
    }
}
