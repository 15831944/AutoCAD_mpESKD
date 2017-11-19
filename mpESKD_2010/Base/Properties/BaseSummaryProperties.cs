﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace mpESKD.Base.Properties
{
    public class BaseSummaryProperties<T> : ObservableCollection<T>
    {
        #region Общие свойства - свойства которые есть у всех примитивов
        public string Style
        {
            get => GetStrProp(nameof(Style));
            set
            {
                SetPropValue(nameof(Style), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Style)));
            }
        }
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

        protected string GetStrProp(string propName)
        {
            IEnumerable<string> vals = this.Select(data => (string)data.GetType().GetProperty(propName).GetValue(data, null)).ToArray();
            return GetSummaryStrValue(vals);
        }
        /// <summary>
        /// Получение значения суммарного свойства
        /// типа Double объектов коллекции
        /// </summary>
        /// <param name="propName">Название свойства</param>
        /// <returns></returns>
        protected int? GetIntProp(string propName)
        {
            IEnumerable<int> vals = this.Select(data => (int)data.GetType().GetProperty(propName).GetValue(data, null)).ToArray();
            return GetSummaryIntValue(vals);
        }
        /// <summary>
        /// Получение значения суммарного свойства
        /// типа Double объектов коллекции
        /// </summary>
        /// <param name="propName">Название свойства</param>
        /// <returns></returns>
        protected double? GetDoubleProp(string propName)
        {
            IEnumerable<double> vals = this.Select(data => (double)(data.GetType().GetProperty(propName).GetValue(data, null))).ToArray();
            return GetSummaryDoubleValue(vals);
        }

        protected bool? GetBoolProp(string propName)
        {
            IEnumerable<bool> vals = this.Select(data => (bool) (data.GetType().GetProperty(propName).GetValue(data, null))).ToArray();
            return GetSummaryBoolValue(vals);
        }
        /// <summary>
        /// Объединение значений свойств типа Double в суммарное
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        protected int? GetSummaryIntValue(IEnumerable<int> vals)
        {
            if (vals.Distinct().Count() > 1)
                return null;
            return vals.FirstOrDefault();
        }
        protected double? GetSummaryDoubleValue(IEnumerable<double> vals)
        {
            if (vals.Distinct(new DoubleEqComparer(0.00001)).Count() > 1)
                return null;
            return vals.FirstOrDefault();
        }

        protected string GetSummaryStrValue(IEnumerable<string> vals)
        {
            if (vals.Distinct().Count() > 1)
                return "*РАЗЛИЧНЫЕ*";
            return vals.FirstOrDefault();
        }

        protected bool? GetSummaryBoolValue(IEnumerable<bool> vals)
        {
            if (vals.Distinct().Count() > 1) return null;
            return vals.FirstOrDefault();
        }
        /// <summary>
        /// Задание значения свойства всем объектам коллекции
        /// </summary>
        /// <param name="propName">Название свойства</param>
        /// <param name="value">Новое значение свойства</param>
        protected void SetPropValue(string propName, object value)
        {
            foreach (var data in this)
            {
                data.GetType().GetProperty(propName).SetValue(data, value, null);
            }
        }
    }
}
