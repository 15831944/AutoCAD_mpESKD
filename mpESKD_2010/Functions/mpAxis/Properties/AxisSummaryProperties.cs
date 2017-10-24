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
        #region General

        public string Scale
        {
            get => GetStrProp(nameof(Scale));
            set
            {
                SetPropValue(nameof(Scale), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Scale)));
            }
        }

        public string LayerName
        {
            get => GetStrProp(nameof(LayerName));
            set
            {
                SetPropValue(nameof(LayerName), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(LayerName)));
            }
        }

        public double? LineTypeScale
        {
            get => GetDoubleProp(nameof(LineTypeScale));
            set
            {
                SetPropValue(nameof(LineTypeScale), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(LineTypeScale)));
            }
        }

        #endregion

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
