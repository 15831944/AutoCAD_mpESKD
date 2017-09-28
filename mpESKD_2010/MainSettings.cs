using System.ComponentModel;
using System.Runtime.CompilerServices;
using ModPlusAPI;

namespace mpESKD
{
    public class MainSettings : INotifyPropertyChanged
    {
        private bool _useScaleFromStyle;
        /// <summary>Использовать масштаб из стиля</summary>
        public bool UseScaleFromStyle
        {
            get => !bool.TryParse(
                       UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", "UseScaleFromStyle"),
                       out _useScaleFromStyle) || _useScaleFromStyle; // true
            set
            {
                _useScaleFromStyle = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", "UseScaleFromStyle", value.ToString(), true);
                OnPropertyChanged(nameof(UseScaleFromStyle));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
