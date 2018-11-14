namespace mpESKD.Base.Properties
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Annotations;
    using Autodesk.AutoCAD.DatabaseServices;
    using Enums;
    using Helpers;

    public class SummaryProperty : INotifyPropertyChanged
    {
        public SummaryProperty(string entityName)
        {
            EntityName = entityName;
            EntityPropertyDataCollection = new ObservableCollection<IntellectualEntityProperty>();
        }

        public string EntityName { get; }

        public ObservableCollection<IntellectualEntityProperty> EntityPropertyDataCollection { get; }

        public PropertiesCategory Category
        {
            get
            {
                var p = EntityPropertyDataCollection.FirstOrDefault();
                if (p != null) return p.Category;
                return PropertiesCategory.Undefined;
            }
        }

        public int OrderIndex
        {
            get
            {
                var property = EntityPropertyDataCollection.FirstOrDefault();
                if (property != null)
                    return property.OrderIndex;
                return 0;
            }
        }

        public string PropertyName => EntityPropertyDataCollection.FirstOrDefault()?.Name;

        public string DisplayNameLocalizationKey => EntityPropertyDataCollection.FirstOrDefault()?.DisplayNameLocalizationKey;

        public string DescriptionLocalizationKey => EntityPropertyDataCollection.FirstOrDefault()?.DescriptionLocalizationKey;

        public object SummaryValue
        {
            get
            {
                var different = $"*{ModPlusAPI.Language.GetItem(MainFunction.LangItem, "vc1")}*";
                var undefined = $"*{ModPlusAPI.Language.GetItem(MainFunction.LangItem, "vc2")}*";
                var values = EntityPropertyDataCollection.Select(e => e.Value).ToList();
                var value = values.FirstOrDefault();
                if (value != null)
                {
                    var valueType = value.GetType();

                    if (valueType == typeof(AnnotationScale))
                    {
                        var scales = values.Select(v => ((AnnotationScale) v).Name).ToList();
                        if (scales.Distinct().Count() > 1)
                            return different;
                        return scales.First();
                    }

                    if (valueType == typeof(int))
                    {
                        if (values.Cast<int>().Distinct().Count() > 1)
                            return null;
                    }

                    if (valueType == typeof(double))
                    {
                        if (values.Cast<double>().Distinct(new DoubleEqComparer()).Count() > 1)
                            return null;
                    }

                    if (valueType == typeof(string))
                    {
                        if (values.Cast<string>().Distinct().Count() > 1)
                            return different;
                    }
                    
                    return value;
                }
                return undefined;
            }
            set
            {
                foreach (IntellectualEntityProperty property in EntityPropertyDataCollection)
                {
                    if (property.Value.GetType() == typeof(AnnotationScale))
                        property.Value = AcadHelpers.GetAnnotationScaleByName(value.ToString());
                    else
                        property.Value = value;

                }
                OnPropertyChanged();
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