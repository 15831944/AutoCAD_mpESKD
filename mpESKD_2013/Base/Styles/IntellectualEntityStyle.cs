﻿namespace mpESKD.Base.Styles
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Xml.Linq;
    using Annotations;
    using Enums;
    using Properties;

    public class IntellectualEntityStyle : INotifyPropertyChanged
    {
        private string _name;
        private string _description;
        private FontWeight _fontWeight;
        private bool _isCurrent;

        public IntellectualEntityStyle()
        {
            Properties = new ObservableCollection<IntellectualEntityProperty>();
        }

        public ObservableCollection<IntellectualEntityProperty> Properties { get; }

        /// <summary>
        /// Имя стиля
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Имя функции
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// Описание стиля
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (value == _description) return;
                _description = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Идентификатор стиля
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Тип стиля (системный, пользовательский)
        /// </summary>
        public StyleType StyleType { get; set; }

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
        /// Можно ли редактировать
        /// <remarks>Свойство для редактора стилей</remarks>
        /// </summary>
        public bool CanEdit => StyleType != StyleType.System;

        /// <summary>
        /// Толщина текста в редакторе
        /// <remarks>Свойство для редактора стилей</remarks>
        /// </summary>
        public FontWeight FontWeight
        {
            get => _fontWeight;
            set
            {
                _fontWeight = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Является ли стиль текущем
        /// <remarks>Свойство для редактора стилей</remarks>
        /// </summary>
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
