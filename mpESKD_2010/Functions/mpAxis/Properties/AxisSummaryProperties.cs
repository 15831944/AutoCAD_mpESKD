using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;
using mpESKD.Base.Properties;

namespace mpESKD.Functions.mpAxis.Properties
{
    public class AxisSummaryProperties : BaseSummaryProperties<AxisPropertiesData>
    {
        /// <summary>Позиция маркеров</summary>
        public string MarkersPosition
        {
            get => GetStrProp(nameof(MarkersPosition));
            set
            {
                SetPropValue(nameof(MarkersPosition), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(MarkersPosition)));
            }
        }
        /// <summary>Излом</summary>
        public int? Fracture
        {
            get => GetIntProp(nameof(Fracture));
            set
            {
                SetPropValue(nameof(Fracture), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Fracture)));
            }
        }
        /// <summary>Нижний отступ излома</summary>
        public int? BottomFractureOffset
        {
            get => GetIntProp(nameof(BottomFractureOffset));
            set
            {
                SetPropValue(nameof(BottomFractureOffset), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(BottomFractureOffset)));
            }
        }
        /// <summary>Верхний отступ излома</summary>
        public int? TopFractureOffset
        {
            get => GetIntProp(nameof(TopFractureOffset));
            set
            {
                SetPropValue(nameof(TopFractureOffset), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(TopFractureOffset)));
            }
        }
        /// <summary>Маркер диаметров</summary>
        public int? MarkersDiameter
        {
            get => GetIntProp(nameof(MarkersDiameter));
            set
            {
                SetPropValue(nameof(MarkersDiameter), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(MarkersDiameter)));
            }
        }
        /// <summary>Количество маркеров</summary>
        public int? MarkersCount
        {
            get => GetIntProp(nameof(MarkersCount));
            set
            {
                SetPropValue(nameof(MarkersCount), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(MarkersCount)));
            }
        }
        /// <summary>Тип линии</summary>
        public string LineType
        {
            get => GetStrProp(nameof(LineType));
            set
            {
                SetPropValue(nameof(LineType), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(LineType)));
            }
        }
        public new string Scale
        {
            get => GetStrProp(nameof(Scale));
            set
            {
                SetPropValue(nameof(Scale), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Scale)));
                if (MainStaticSettings.Settings.AxisLineTypeScaleProportionScale)
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(LineTypeScale)));
            }
        }

        public AxisSummaryProperties(IEnumerable<ObjectId> objectIds)
        {
            foreach (ObjectId objectId in objectIds)
            {
                AxisPropertiesData data = new AxisPropertiesData(objectId);
                if (data.IsValid)
                    Add(data);
            }
        }

        public new void Add(AxisPropertiesData data)
        {
            base.Add(data);
            data.AnyPropertyChanged += Data_AnyPropertyChanged;
        }

        private void Data_AnyPropertyChanged(object sender, EventArgs e)
        {
            AllPropertyChangedReise();
        }
        /// <summary>
        /// Вызов события изменения для каждого свойства объекта
        /// </summary>
        protected void AllPropertyChangedReise()
        {
            string[] propsNames = this.GetType()
                .GetProperties
                (BindingFlags.Instance
                 | BindingFlags.Public
                 | BindingFlags.DeclaredOnly)
                .Select(prop => prop.Name)
                .ToArray();
            foreach (string propName in propsNames)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(propName));
            }
        }
    }
}
