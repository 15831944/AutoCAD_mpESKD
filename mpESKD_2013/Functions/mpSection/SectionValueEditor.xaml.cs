namespace mpESKD.Functions.mpSection
{
    using System.Windows;

    public partial class SectionValueEditor
    {
        public Section Section;

        public SectionValueEditor()
        {
            InitializeComponent();
            //todo translate
            Title = "Section value change";
        }

        private void SectionValueEditor_OnLoaded(object sender, RoutedEventArgs e)
        {
            TbDesignation.Text = Section.Designation;
            TbDesignationPrefix.Text = Section.DesignationPrefix;
            TbSheetNumber.Text = Section.SheetNumber;
            TbDesignation.Focus();
        }

        private void BtAccept_OnClick(object sender, RoutedEventArgs e)
        {
            OnAccept();
            DialogResult = true;
        }

        private void OnAccept()
        {
            Section.Designation = TbDesignation.Text;
            Section.DesignationPrefix = TbDesignationPrefix.Text;
            Section.SheetNumber = TbSheetNumber.Text;

            if (ChkRestoreTextPosition.IsChecked.HasValue && ChkRestoreTextPosition.IsChecked.Value)
            {
                Section.AlongBottomShelfTextOffset = double.NaN;
                Section.AlongTopShelfTextOffset = double.NaN;
                Section.AcrossBottomShelfTextOffset = double.NaN;
                Section.AcrossTopShelfTextOffset = double.NaN;
            }
        }
    }
}
