namespace mpESKD.Base.Properties
{
    using System.Windows;
    using System.Windows.Input;
    using ModPlusAPI;

    public partial class PaletteSettings
    {
        public PaletteSettings()
        {
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem(Invariables.LangItem, "h1");
            Loaded += LmSettings_Loaded;
        }

        private void LmSettings_Loaded(object sender, RoutedEventArgs e)
        {
            ChkAutoLoad.IsChecked = bool.TryParse(UserConfigFile.GetValue("mpESKD", "AutoLoad"), out bool flag) & flag;
            ChkAddToMpPalette.IsChecked = !bool.TryParse(UserConfigFile.GetValue("mpESKD", "AddToMpPalette"), out flag) | flag;
        }

        private void BtClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ChkAddToMpPalette_OnChecked_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var flag = ChkAddToMpPalette.IsChecked ?? false;
            UserConfigFile.SetValue("mpESKD", "AddToMpPalette", flag.ToString(), true);
        }

        private void ChkAutoLoad_OnChecked_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var flag = ChkAutoLoad.IsChecked ?? false;
            UserConfigFile.SetValue("mpESKD", "AutoLoad", flag.ToString(), true);
        }

        private void LmSettings_OnKeyDown(object sender, KeyEventArgs e)
        {
            Close();
        }
    }
}
