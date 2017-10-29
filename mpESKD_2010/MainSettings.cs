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
                       UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", nameof(UseScaleFromStyle)),
                       out _useScaleFromStyle) && _useScaleFromStyle; // false
            set
            {
                _useScaleFromStyle = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", nameof(UseScaleFromStyle), value.ToString(), true);
                OnPropertyChanged();
            }
        }

        private bool _useLayerFromStyle;
        /// <summary>Использовать слой из стиля</summary>
        public bool UseLayerFromStyle
        {
            get => bool.TryParse(
                       UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", nameof(UseLayerFromStyle)),
                       out _useLayerFromStyle) && _useLayerFromStyle; // false
            set
            {
                _useLayerFromStyle = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", nameof(UseLayerFromStyle), value.ToString(), true);
                OnPropertyChanged();
            }
        }

        private int _ifNoLayer;
        /// <summary>Поведение при отсутствии слоя: 0 - применить текущий, 1 - создать новый</summary>
        public int IfNoLayer
        {
            get => int.TryParse(
                UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", nameof(IfNoLayer)),
                out _ifNoLayer)
                ? _ifNoLayer
                : 0;
            set
            {
                _ifNoLayer = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", nameof(IfNoLayer), value.ToString(), true);
                OnPropertyChanged();
            }
        }

        private bool _axisLineTypeScaleProportionScale;
        /// <summary>Менять масштаб типа линии прямой оси пропорционально масштабу примитива</summary>
        public bool AxisLineTypeScaleProportionScale
        {
            get => !bool.TryParse(
                       UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD",
                           nameof(AxisLineTypeScaleProportionScale)),
                       out _axisLineTypeScaleProportionScale) || _axisLineTypeScaleProportionScale; // true
            set
            {
                _axisLineTypeScaleProportionScale = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", nameof(AxisLineTypeScaleProportionScale), value.ToString(), true);
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class MainStaticSettings
    {
        public static MainSettings Settings;
        static MainStaticSettings()
        {
            if (Settings == null)
                ReloadSettings();
        }

        public static void ReloadSettings()
        {
            Settings = new MainSettings();
        }
    }
}
