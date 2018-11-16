using System.Windows;
using System.Windows.Input;

namespace mpESKD.Functions.mpAxis
{
    public partial class AxisValueEditor
    {
        public Axis Axis;

        public AxisValueEditor()
        {
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem(MainFunction.LangItem, "h67");
            // markers positions
            //todo do do
            //CbMarkersPosition.ItemsSource = AxisPropertiesHelpers.AxisMarkersTypeLocalNames;
        }

        private void AxisValueEditor_OnLoaded(object sender, RoutedEventArgs e)
        {
            // visibility
            ChangeOrientVisibility();
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
            // markers position
            //todo do do
            //CbMarkersPosition.SelectedItem = AxisPropertiesHelpers.GetLocalAxisMarkersPositionName(Axis.MarkersPosition);
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
            // markers position
            //todo do do
            //Axis.MarkersPosition = AxisPropertiesHelpers.GetAxisMarkersPositionByLocalName(CbMarkersPosition.SelectedItem.ToString());
        }

        #region Visibility

        void ChangeOrientVisibility()
        {
            //todo do do
            //if (Axis.MarkersPosition == AxisMarkersPosition.Both || Axis.MarkersPosition == AxisMarkersPosition.Top)
            //    TbTopOrientText.Visibility = Axis.TopOrientMarkerVisible ? Visibility.Visible : Visibility.Collapsed;
            //else TbTopOrientText.Visibility = Visibility.Collapsed;

            //if (Axis.MarkersPosition == AxisMarkersPosition.Both || Axis.MarkersPosition == AxisMarkersPosition.Bottom)
            //    TbBottomOrientText.Visibility =
            //        Axis.BottomOrientMarkerVisible ? Visibility.Visible : Visibility.Collapsed;
            //else TbBottomOrientText.Visibility = Visibility.Collapsed;
        }

        void ChangeSecondVisibility(bool show)
        {
            if (show)
            {
                TbSecondPrefix.Visibility = TbSecondText.Visibility = TbSecondSuffix.Visibility = Visibility.Visible;
            }
            else
            {
                TbSecondPrefix.Visibility = TbSecondText.Visibility = TbSecondSuffix.Visibility = Visibility.Collapsed;
            }
        }
        void ChangeThirdVisibility(bool show)
        {
            if (show)
            {
                TbThirdPrefix.Visibility = TbThirdText.Visibility = TbThirdSuffix.Visibility = Visibility.Visible;
            }
            else
            {
                TbThirdPrefix.Visibility = TbThirdText.Visibility = TbThirdSuffix.Visibility = Visibility.Collapsed;
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
