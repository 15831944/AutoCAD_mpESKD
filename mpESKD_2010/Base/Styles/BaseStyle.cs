using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Linq;
using mpESKD.Base.Properties;
using ModPlusAPI;

// ReSharper disable InconsistentNaming

namespace mpESKD.Base.Styles
{
    /// <summary>Интерфейс стиля для элемента</summary>
    public interface IMPCOStyle
    {
        string Name { get; set; }
        string FunctionName { get; set; }
        string Description { get; set; }
        string Guid { get; set; }
        MPCOStyleType StyleType { get; set; }
        XElement LayerXmlData { get; set; }
        List<MPCOBaseProperty> Properties { get; set; }
    }
    /// <inheritdoc />
    /// <summary>Базовый класс презентора стиля для работы в редакторе стилей</summary>
    public class MPCOStyleForEditor : INotifyPropertyChanged
    {
        /// <summary>Базовый конструктор</summary>
        /// <param name="style"></param>
        /// <param name="currentStyleGuid"></param>
        /// <param name="parent"></param>
        public MPCOStyleForEditor(IMPCOStyle style, string currentStyleGuid, StyleToBind parent)
        {
            Parent = parent;
            if (style.StyleType == MPCOStyleType.System)
            {
                CanEdit = false;
                Name = style.Name + " (Системный)";
            }
            else
            {
                CanEdit = true;
                Name = style.Name;
            }
            Description = style.Description;
            Guid = style.Guid;
            FunctionName = style.FunctionName;
            IsCurrent = style.Guid == currentStyleGuid;
        }

        public MPCOStyleForEditor(StyleToBind parent)
        {
            Parent = parent;
            Name = "Новый пользовательский стиль";
            Description = string.Empty;
            FunctionName = parent.FunctionLocalName;
            CanEdit = true;
            IsCurrent = false;
            Guid = System.Guid.NewGuid().ToString();
        }

        public StyleToBind Parent { get; set; }
        /// <summary>Можно ли редактировать</summary>
        public bool CanEdit { get; set; }

        private FontWeight _fontWeight;
        /// <summary>Толщина текста в редакторе</summary>
        public FontWeight FontWeight
        {
            get => _fontWeight; set { _fontWeight = value; OnPropertyChanged(nameof(FontWeight)); }
        }

        private bool _isCurrent;
        /// <summary>Является ли стиль текущем</summary>
        public bool IsCurrent {
            get => _isCurrent;
            set
            {
                FontWeight = value ? FontWeights.SemiBold : FontWeights.Normal;
                _isCurrent = value;
                if(value)
                    UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpBreakLine", "CurrentStyleGuid", Guid, true);
                OnPropertyChanged(nameof(IsCurrent));
            } 
        }

        private string _name;
        /// <summary>Название стиля</summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public string FunctionName { get; set; }
        public string Description { get; set; }
        public string Guid { get; set; }
        public XElement LayerXmlData { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    public enum MPCOStyleType
    {
        System = 1,
        User = 2
    }
}
