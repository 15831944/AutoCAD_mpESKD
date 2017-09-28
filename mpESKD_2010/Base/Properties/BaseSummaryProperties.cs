using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace mpESKD.Base.Properties
{
    public class BaseSummaryProperties<T> : ObservableCollection<T>
    {
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
