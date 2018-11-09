namespace mpESKD.Base.Properties
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using ModPlusAPI;
    using System.Reflection;

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
            var vals = this.Select(data => (string)data.GetType().GetProperty(propName).GetValue(data, null)).ToArray();
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
            var vals = this.Select(data => (int)data.GetType().GetProperty(propName).GetValue(data, null)).ToArray();
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
            var vals = this.Select(data => (double)(data.GetType().GetProperty(propName).GetValue(data, null))).ToArray();
            return GetSummaryDoubleValue(vals);
        }

        protected bool? GetBoolProp(string propName)
        {
            var vals = this.Select(data => (bool) data.GetType().GetProperty(propName).GetValue(data, null)).ToArray();
            return GetSummaryBoolValue(vals);
        }
        /// <summary>
        /// Объединение значений свойств типа Double в суммарное
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        protected int? GetSummaryIntValue(int[] vals)
        {
            if (vals.Distinct().Count() > 1)
                return null;
            return vals.FirstOrDefault();
        }
        protected double? GetSummaryDoubleValue(double[] vals)
        {
            if (vals.Distinct(new DoubleEqComparer(0.00001)).Count() > 1)
                return null;
            return vals.FirstOrDefault();
        }

        protected string GetSummaryStrValue(string[] vals)
        {
            if (vals.Distinct().Count() > 1)
                return "*" + Language.GetItem(MainFunction.LangItem, "vc1") + "*"; // РАЗЛИЧНЫЕ
            return vals.FirstOrDefault();
        }

        protected bool? GetSummaryBoolValue(bool[] vals)
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
                data.GetType().GetProperty(propName)?.SetValue(data, value, null);
            }
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
