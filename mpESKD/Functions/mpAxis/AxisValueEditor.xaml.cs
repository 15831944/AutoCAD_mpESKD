namespace mpESKD.Functions.mpAxis
{
    using System.Windows;
    using Base;
    using Base.Enums;

    /// <summary>
    /// Редактор значений прямой оси
    /// </summary>
    public partial class AxisValueEditor
    {
        private readonly Axis _intellectualEntity;

        /// <summary>
        /// Initializes a new instance of the <see cref="AxisValueEditor"/> class.
        /// </summary>
        /// <param name="intellectualEntity">Редактируемый экземпляр интеллектуального объекта</param>
        public AxisValueEditor(IntellectualEntity intellectualEntity)
        {
            _intellectualEntity = (Axis)intellectualEntity;
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem(Invariables.LangItem, "h41");

            SetValues();
        }

        private void SetValues()
        {
            // visibility
            ChangeOrientVisibility();
            if (_intellectualEntity.MarkersCount > 1)
            {
                ChangeSecondVisibility(true);
                ChangeThirdVisibility(_intellectualEntity.MarkersCount > 2);
            }
            else
            {
                ChangeSecondVisibility(false);
                ChangeThirdVisibility(false);
            }

            // values
            TbFirstPrefix.Text = _intellectualEntity.FirstTextPrefix;
            TbFirstText.Text = _intellectualEntity.FirstText;
            TbFirstSuffix.Text = _intellectualEntity.FirstTextSuffix;

            TbSecondPrefix.Text = _intellectualEntity.SecondTextPrefix;
            TbSecondText.Text = _intellectualEntity.SecondText;
            TbSecondSuffix.Text = _intellectualEntity.SecondTextSuffix;

            TbThirdPrefix.Text = _intellectualEntity.ThirdTextPrefix;
            TbThirdText.Text = _intellectualEntity.ThirdText;
            TbThirdSuffix.Text = _intellectualEntity.ThirdTextSuffix;

            TbBottomOrientText.Text = _intellectualEntity.BottomOrientText;
            TbTopOrientText.Text = _intellectualEntity.TopOrientText;

            // markers position
            CbMarkersPosition.SelectedItem = _intellectualEntity.MarkersPosition;

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
            _intellectualEntity.FirstTextPrefix = TbFirstPrefix.Text;
            _intellectualEntity.FirstText = TbFirstText.Text;
            _intellectualEntity.FirstTextSuffix = TbFirstSuffix.Text;

            _intellectualEntity.SecondTextPrefix = TbSecondPrefix.Text;
            _intellectualEntity.SecondText = TbSecondText.Text;
            _intellectualEntity.SecondTextSuffix = TbSecondSuffix.Text;

            _intellectualEntity.ThirdTextPrefix = TbThirdPrefix.Text;
            _intellectualEntity.ThirdText = TbThirdText.Text;
            _intellectualEntity.ThirdTextSuffix = TbThirdSuffix.Text;

            _intellectualEntity.BottomOrientText = TbBottomOrientText.Text;
            _intellectualEntity.TopOrientText = TbTopOrientText.Text;

            // markers position
            _intellectualEntity.MarkersPosition = (AxisMarkersPosition)CbMarkersPosition.SelectedItem;
        }

        #region Visibility

        private void ChangeOrientVisibility()
        {
            if (_intellectualEntity.MarkersPosition == AxisMarkersPosition.Both || _intellectualEntity.MarkersPosition == AxisMarkersPosition.Top)
            {
                TbTopOrientText.Visibility = _intellectualEntity.TopOrientMarkerVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                TbTopOrientText.Visibility = Visibility.Collapsed;
            }

            if (_intellectualEntity.MarkersPosition == AxisMarkersPosition.Both || _intellectualEntity.MarkersPosition == AxisMarkersPosition.Bottom)
            {
                TbBottomOrientText.Visibility =
                    _intellectualEntity.BottomOrientMarkerVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                TbBottomOrientText.Visibility = Visibility.Collapsed;
            }
        }

        private void ChangeSecondVisibility(bool show)
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

        private void ChangeThirdVisibility(bool show)
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
    }
}
