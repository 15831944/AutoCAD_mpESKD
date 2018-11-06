namespace mpESKD
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using ModPlusAPI;

    public class MainSettings : INotifyPropertyChanged
    {
        #region Main

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

        private bool _useTextStyleFromStyle;
        /// <summary>Использовать текстовый стиль из стиля</summary>
        public bool UseTextStyleFromStyle
        {
            get => bool.TryParse(
                       UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", nameof(UseTextStyleFromStyle)),
                       out _useTextStyleFromStyle) && _useTextStyleFromStyle; // false
            set
            {
                _useTextStyleFromStyle = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", nameof(UseTextStyleFromStyle), value.ToString(), true);
                OnPropertyChanged();
            }
        }

        private int _ifNoTextStyle;
        /// <summary>Поведение при отсутствии текстового стиля: 0 - применить текущий, 1 - создать новый</summary>
        public int IfNoTextStyle
        {
            get => int.TryParse(
                UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", nameof(IfNoTextStyle)),
                out _ifNoTextStyle)
                ? _ifNoTextStyle
                : 0;
            set
            {
                _ifNoTextStyle = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", nameof(IfNoTextStyle), value.ToString(), true);
                OnPropertyChanged();
            }
        }

        private int _maxSelectedObject;
        /// <summary>Предельное количество выбранных объектов для работы палитры</summary>
        public int MaxSelectedObjects
        {
            get => int.TryParse(
                UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", nameof(MaxSelectedObjects)),
                out _maxSelectedObject)
                ? _maxSelectedObject
                : 100;
            set
            {
                _maxSelectedObject = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", nameof(MaxSelectedObjects), value.ToString(), true);
                OnPropertyChanged();
            }
        }

        #endregion

        #region Axis

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

        private bool _axisSaveLastTextAndContinueNew;
        /// <summary>Сохранять значения последних созданных осей и продолжать значения создаваемых осей</summary>
        public bool AxisSaveLastTextAndContinueNew
        {
            get => !bool.TryParse(
                       UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD",
                           nameof(AxisSaveLastTextAndContinueNew)),
                       out _axisSaveLastTextAndContinueNew) || _axisSaveLastTextAndContinueNew; // true
            set
            {
                _axisSaveLastTextAndContinueNew = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", nameof(AxisSaveLastTextAndContinueNew), value.ToString(), true);
                OnPropertyChanged();
            }
        }

        private bool _axisUsePluginTextEditor;
        /// <summary>Использовать редактор значений оси из плагина</summary>
        public bool AxisUsePluginTextEditor
        {
            get => !bool.TryParse(
                       UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD",
                           nameof(AxisUsePluginTextEditor)),
                       out _axisUsePluginTextEditor) || _axisUsePluginTextEditor; // true
            set
            {
                _axisUsePluginTextEditor = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpESKD", nameof(AxisUsePluginTextEditor), value.ToString(), true);
                OnPropertyChanged();
            }
        }
        #endregion

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
