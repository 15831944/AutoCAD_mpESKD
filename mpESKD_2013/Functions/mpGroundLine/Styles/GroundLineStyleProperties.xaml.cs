namespace mpESKD.Functions.mpGroundLine.Styles
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Windows;
    using Base.Enums;
    using Base.Helpers;
    using Base.Styles;
    using Properties;

    public partial class GroundLineStyleProperties
    {
        public GroundLineStyleProperties(string layerNameFromStyle)
        {
            InitializeComponent();
            ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources);
            ModPlusAPI.Windows.Helpers.WindowHelpers.ChangeStyleForResourceDictionary(Resources);
            // FirstStrokeOffset
            //CbFirstStrokeOffset.ItemsSource = GroundLinePropertiesHelpers.FirstStrokeOffsetNames;
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
            //if (!(sender is FrameworkElement fe)) return;
            //if (fe.Name.Equals("TbFirstStrokeOffset"))
            //    StyleEditorWork.ShowDescription(GroundLineProperties.FirstStrokeOffset.Description);
            //if (fe.Name.Equals("TbStrokeLength"))
            //    StyleEditorWork.ShowDescription(GroundLineProperties.StrokeLength.Description);
            //if (fe.Name.Equals("TbStrokeOffset"))
            //    StyleEditorWork.ShowDescription(GroundLineProperties.StrokeOffset.Description);
            //if (fe.Name.Equals("TbStrokeAngle"))
            //    StyleEditorWork.ShowDescription(GroundLineProperties.StrokeAngle.Description);
            //if (fe.Name.Equals("TbSpace"))
            //    StyleEditorWork.ShowDescription(GroundLineProperties.Space.Description);
            //if (fe.Name.Equals("CbScale"))
            //    StyleEditorWork.ShowDescription(GroundLineProperties.Scale.Description);
            //if (fe.Name.Equals("TbLineTypeScale"))
            //    StyleEditorWork.ShowDescription(GroundLineProperties.LineTypeScale.Description);
            //if (fe.Name.Equals("CbLayerName"))
            //    StyleEditorWork.ShowDescription(GroundLineProperties.LayerName.Description);
        }

        private void FrameworkElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            StyleEditorWork.ShowDescription(string.Empty);
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
                                    TbLineType.Text = ltr.Name;
                                }
                            }
                            tr.Commit();
                        }
                }
            }
        }
    }

    public class GroundLineFirstStrokeOffsetValueConverter : IValueConverter
    {
        //public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    return GroundLinePropertiesHelpers.GetLocalFirstStrokeOffsetName((GroundLineFirstStrokeOffset)value);
        //}

        //public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    return GroundLinePropertiesHelpers.GetFirstStrokeOffsetByLocalName((string)value);
        //}
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
