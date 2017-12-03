using System.Windows;
using System.Windows.Input;
using mpESKD.Functions.mpAxis.Properties;
using ModPlusAPI.Windows.Helpers;

namespace mpESKD.Functions.mpAxis
{
    public partial class AxisValueEditor
    {
        public Axis Axis;

        public AxisValueEditor()
        {
            InitializeComponent();
            this.OnWindowStartUp();

        }

        private void AxisValueEditor_OnLoaded(object sender, RoutedEventArgs e)
        {
            // visibility
            if (Axis.TopOrientMarkerVisible || Axis.BottomOrientMarkerVisible)
                ChangeOrientVisibility(true);
            else ChangeOrientVisibility(false);
            if (Axis.MarkersCount > 1)
            {
                ChangeSecondVisibility(true);
                ChangeThirdVisibility(Axis.MarkersCount > 2);
            }
            else
            {
                ChangeSecondVisibility(false);
                ChangeThirdVisibility(false);
            }
            // values
            TbFirstPrefix.Text = Axis.FirstTextPrefix;
            TbFirstText.Text = Axis.FirstText;
            TbFirstSuffix.Text = Axis.FirstTextSuffix;

            TbSecondPrefix.Text = Axis.SecondTextPrefix;
            TbSecondText.Text = Axis.SecondText;
            TbSecondSuffix.Text = Axis.SecondTextSuffix;

            TbThirdPrefix.Text = Axis.ThirdTextPrefix;
            TbThirdText.Text = Axis.ThirdText;
            TbThirdSuffix.Text = Axis.ThirdTextSuffix;

            TbBottomOrientText.Text = Axis.BottomOrientText;
            TbTopOrientText.Text = Axis.TopOrientText;
            // focus
            TbFirstText.Focus();
        }

        private void BtAccept_OnClick(object sender, RoutedEventArgs e)
        {
            OnAccept();
            DialogResult = true;
        }

        private void OnAccept()
        {
            // values
            Axis.FirstTextPrefix = TbFirstPrefix.Text;
            Axis.FirstText = TbFirstText.Text;
            Axis.FirstTextSuffix = TbFirstSuffix.Text;

            Axis.SecondTextPrefix = TbSecondPrefix.Text;
            Axis.SecondText = TbSecondText.Text;
            Axis.SecondTextSuffix = TbSecondSuffix.Text;

            Axis.ThirdTextPrefix = TbThirdPrefix.Text;
            Axis.ThirdText = TbThirdText.Text;
            Axis.ThirdTextSuffix = TbThirdSuffix.Text;

            Axis.BottomOrientText = TbBottomOrientText.Text;
            Axis.TopOrientText = TbTopOrientText.Text;
            //
        }

        #region Visibility

        void ChangeOrientVisibility(bool show)
        {
            if (show)
            {
                OrientEllipse.Visibility = OrientLine.Visibility =
                OrientArrow.Visibility = 
                Visibility.Visible;
            }
            else
            {
                OrientEllipse.Visibility = OrientLine.Visibility =
                    OrientArrow.Visibility = 
                    Visibility.Collapsed;
            }
            if (Axis.MarkersPosition == AxisMarkersPosition.Both || Axis.MarkersPosition == AxisMarkersPosition.Top)
                TbTopOrientText.Visibility = Axis.TopOrientMarkerVisible ? Visibility.Visible : Visibility.Collapsed;
            else TbTopOrientText.Visibility = Visibility.Collapsed;

            if (Axis.MarkersPosition == AxisMarkersPosition.Both || Axis.MarkersPosition == AxisMarkersPosition.Bottom)
                TbBottomOrientText.Visibility =
                    Axis.BottomOrientMarkerVisible ? Visibility.Visible : Visibility.Collapsed;
            else TbBottomOrientText.Visibility = Visibility.Collapsed;
        }

        void ChangeSecondVisibility(bool show)
        {
            if (show)
            {
                SecondEllipse.Visibility = TbSecondPrefix.Visibility =
                TbSecondText.Visibility = TbSecondSuffix.Visibility =
                Visibility.Visible;
            }
            else
            {
                SecondEllipse.Visibility = TbSecondPrefix.Visibility =
                    TbSecondText.Visibility = TbSecondSuffix.Visibility =
                        Visibility.Collapsed;
            }
        }
        void ChangeThirdVisibility(bool show)
        {
            if (show)
            {
                ThirdEllipse.Visibility = TbThirdPrefix.Visibility =
                    TbThirdText.Visibility = TbThirdSuffix.Visibility =
                    Visibility.Visible;
            }
            else
            {
                ThirdEllipse.Visibility = TbThirdPrefix.Visibility =
                    TbThirdText.Visibility = TbThirdSuffix.Visibility =
                        Visibility.Collapsed;
            }
        }

        #endregion

        private void AxisValueEditor_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) DialogResult = false;
            if (e.Key == Key.Enter)
            {
                OnAccept();
                DialogResult = true;
            }
        }
    }
}
