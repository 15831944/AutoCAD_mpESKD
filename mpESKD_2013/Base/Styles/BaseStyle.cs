// ReSharper disable InconsistentNaming

namespace mpESKD.Base.Styles
{
    using Autodesk.AutoCAD.DatabaseServices;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Xml.Linq;
    using Properties;
    using ModPlusAPI;
    using ModPlusAPI.Annotations;

    public abstract class MPCOStyle
    {
        protected MPCOStyle()
        {
            Properties = new List<MPCOBaseProperty>();
        }

        /// <summary>
        /// Имя стиля
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Имя функции
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// Описание стиля
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Идентификатор стиля
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Тип стиля (системный, пользовательский)
        /// </summary>
        public MPCOStyleType StyleType { get; set; }

        /// <summary>
        /// Xml данные слоя
        /// </summary>
        public XElement LayerXmlData { get; set; }

        /// <summary>
        /// Xml данные текстового стиля (может быть null)
        /// </summary>
        [CanBeNull]
        public XElement TextStyleXmlData { get; set; }

        /// <summary>
        /// Свойства
        /// </summary>
        public List<MPCOBaseProperty> Properties { get; set; }
        
        /// <summary>
        /// Создание системных стилей для указанного типа стиля
        /// </summary>
        /// <typeparam name="T">Тип стиля</typeparam>
        /// <returns></returns>
        public abstract List<T> CreateSystemStyles<T>() where T : MPCOStyle;

        public abstract T ParseStyleFromXElement<T>(XElement styleXel) where T : MPCOStyle, new();
    }

    /// <inheritdoc />
    /// <summary>Базовый класс презентора стиля для работы в редакторе стилей</summary>
    public class MPCOStyleForEditor : INotifyPropertyChanged
    {
        /// <summary>Базовый конструктор</summary>
        /// <param name="style"></param>
        /// <param name="currentStyleGuid"></param>
        /// <param name="parent"></param>
        public MPCOStyleForEditor(MPCOStyle style, string currentStyleGuid, StyleToBind parent)
        {
            Parent = parent;
            if (style.StyleType == MPCOStyleType.System)
            {
                CanEdit = false;
                Name = style.Name + " (" + Language.GetItem(MainFunction.LangItem, "h12") + ")"; // Системный
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
            Name = Language.GetItem(MainFunction.LangItem, "h13"); // Новый пользовательский стиль
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
        public bool IsCurrent
        {
            get => _isCurrent;
            set
            {
                FontWeight = value ? FontWeights.SemiBold : FontWeights.Normal;
                _isCurrent = value;
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

        public XElement TextStyleXmlData { get; set; }

        #region Common properties

        public double LineTypeScale { get; set; }

        public string LineType { get; set; }

        public string LayerName { get; set; }

        public AnnotationScale Scale { get; set; }

        #endregion

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
