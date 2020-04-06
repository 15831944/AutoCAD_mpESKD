﻿namespace mpESKD.Base.Properties
{
    using System.Windows;

    public partial class PaletteSettings
    {
        public PaletteSettings()
        {
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem(Invariables.LangItem, "h1");
        }

        private void BtClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
