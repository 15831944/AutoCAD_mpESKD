namespace mpESKD.Functions.mpSection
{
    using System.Windows;
    using Base;

    /// <summary>
    /// Редактор значений разреза
    /// </summary>
    public partial class SectionValueEditor
    {
        private readonly Section _intellectualEntity;

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionValueEditor"/> class.
        /// </summary>
        /// <param name="intellectualEntity">Редактируемый экземпляр интеллектуального объекта</param>
        public SectionValueEditor(IntellectualEntity intellectualEntity)
        {
            _intellectualEntity = (Section)intellectualEntity;
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem(Invariables.LangItem, "h79");

            SetValues();
        }

        private void SetValues()
        {
            TbDesignation.Text = _intellectualEntity.Designation;
            TbDesignationPrefix.Text = _intellectualEntity.DesignationPrefix;
            TbSheetNumber.Text = _intellectualEntity.SheetNumber;
            TbDesignation.Focus();
        }

        private void BtAccept_OnClick(object sender, RoutedEventArgs e)
        {
            OnAccept();
            DialogResult = true;
        }

        private void OnAccept()
        {
            _intellectualEntity.Designation = TbDesignation.Text;
            _intellectualEntity.DesignationPrefix = TbDesignationPrefix.Text;
            _intellectualEntity.SheetNumber = TbSheetNumber.Text;

            if (ChkRestoreTextPosition.IsChecked.HasValue && ChkRestoreTextPosition.IsChecked.Value)
            {
                _intellectualEntity.AlongBottomShelfTextOffset = double.NaN;
                _intellectualEntity.AlongTopShelfTextOffset = double.NaN;
                _intellectualEntity.AcrossBottomShelfTextOffset = double.NaN;
                _intellectualEntity.AcrossTopShelfTextOffset = double.NaN;
            }
        }
    }
}
