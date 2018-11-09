namespace mpESKD.Functions.mpGroundLine.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Windows;
    using Base.Helpers;
    using Base.Properties;
    using Styles;

    public partial class GroundLinePropertiesPalette
    {
        private readonly PropertiesPalette _parentPalette;

        public GroundLinePropertiesPalette(PropertiesPalette palette)
        {
            _parentPalette = palette;
            InitializeComponent();
            ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources);
            // styles
            var sNames = new List<string>();
            foreach (var style in GroundLineStyleManager.Styles)
            {
                sNames.Add(style.Name);
            }
            CbStyle.ItemsSource = sNames;

            // get FirstStrokeOffset values
            CbFirstStrokeOffset.ItemsSource = GroundLinePropertiesHelpers.FirstStrokeOffsetNames;
            
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

        private GroundLineSummaryProperties _groundLineSummaryProperties;

        private void ShowProperties()
        {
            PromptSelectionResult psr = AcadHelpers.Editor.SelectImplied();

            if (psr.Status != PromptStatus.OK || psr.Value == null || psr.Value.Count == 0)
            {
                _groundLineSummaryProperties = null;
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
                            if (ExtendedDataHelpers.IsApplicable(obj, GroundLineFunction.MPCOEntName))
                            {
                                objectIds.Add(selectedObject.ObjectId);
                            }
                        }
                    }
                }
                if (objectIds.Any())
                {
                    Expander.Header = GroundLineFunction.MPCOEntDisplayName + " (" + objectIds.Count + ")";
                    _groundLineSummaryProperties = new GroundLineSummaryProperties(objectIds);
                    SetData(_groundLineSummaryProperties);
                }
            }
        }

        public void SetData(GroundLineSummaryProperties data)
        {
            DataContext = data;
        }

        private void FrameworkElement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if(!(sender is FrameworkElement fe)) return;

            if (fe.Name.Equals("CbStyle"))
                _parentPalette.ShowDescription(ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h52"));
            if (fe.Name.Equals("CbFirstStrokeOffset"))
                _parentPalette.ShowDescription(GroundLineProperties.FirstStrokeOffset.Description);
            if (fe.Name.Equals("TbStrokeLength"))
                _parentPalette.ShowDescription(GroundLineProperties.StrokeLength.Description);
            if (fe.Name.Equals("TbStrokeOffset"))
                _parentPalette.ShowDescription(GroundLineProperties.StrokeOffset.Description);
            if (fe.Name.Equals("TbStrokeAngle"))
                _parentPalette.ShowDescription(GroundLineProperties.StrokeAngle.Description);
            if (fe.Name.Equals("TbSpace"))
                _parentPalette.ShowDescription(GroundLineProperties.Space.Description);
            if (fe.Name.Equals("CbScale"))
                _parentPalette.ShowDescription(GroundLineProperties.Scale.Description);
            if (fe.Name.Equals("TbLineTypeScale"))
                _parentPalette.ShowDescription(GroundLineProperties.LineTypeScale.Description);
            if (fe.Name.Equals("TbLineType"))
                _parentPalette.ShowDescription(GroundLineProperties.LineType.Description);
            if (fe.Name.Equals("CbLayerName"))
                _parentPalette.ShowDescription(GroundLineProperties.LayerName.Description);
        }

        private void FrameworkElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            _parentPalette.ShowDescription(String.Empty);
        }

        private void GroundLinePropertiesPalette_OnUnloaded(object sender, RoutedEventArgs e)
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
                                    //TODO Uncomment
                                    //((GroundLineSummaryProperties) DataContext).LineType = ltr.Name;
                                }
                            }
                            tr.Commit();
                        }
                }
            }
        }
    }
}
