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
            get => bool.TryParse(
                       UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", "UseScaleFromStyle"),
                       out _useScaleFromStyle) && _useScaleFromStyle; // false
            set
            {
                _useScaleFromStyle = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", "UseScaleFromStyle", value.ToString(), true);
                OnPropertyChanged(nameof(UseScaleFromStyle));
            }
        }

        private bool _useLayerFromStyle;
        /// <summary>Использовать слой из стиля</summary>
        public bool UseLayerFromStyle
        {
            get => bool.TryParse(
                       UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", "UseLayerFromStyle"),
                       out _useLayerFromStyle) && _useLayerFromStyle; // false
            set
            {
                _useLayerFromStyle = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", "UseLayerFromStyle", value.ToString(), true);
                OnPropertyChanged(nameof(UseLayerFromStyle));
            }
        }

        private int _ifNoLayer;
        /// <summary>Поведение при отсутствии слоя: 0 - применить текущий, 1 - создать новый</summary>
        public int IfNoLayer
        {
            get => int.TryParse(
                UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", "IfNoLayer"),
                out _ifNoLayer)
                ? _ifNoLayer
                : 0;
            set
            {
                _ifNoLayer = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", "IfNoLayer", value.ToString(), true);
                OnPropertyChanged(nameof(IfNoLayer));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
