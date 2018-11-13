namespace mpESKD.Base.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Annotations;
    using Autodesk.AutoCAD.DatabaseServices;
    using Enums;

    public class SummaryPropertyData : ObservableCollection<SummaryProperty>
    {
        public SummaryPropertyData(IEnumerable<ObjectId> objectIds)
        {
            foreach (ObjectId objectId in objectIds)
            {
                EntityPropertyData data = new EntityPropertyData(objectId);
                if (data.IsValid)
                {
                    foreach (IntellectualEntityProperty entityProperty in data.Properties)
                    {
                        var p = this.FirstOrDefault(si => si.EntityPropertyDataCollection
                            .Any(ep => ep.Category == entityProperty.Category && ep.Name == entityProperty.Name && ep.Value == entityProperty.Value));
                        if (p == null)
                        {
                            SummaryProperty summaryProperty = new SummaryProperty();
                            summaryProperty.EntityPropertyDataCollection.Add(entityProperty);
                            Add(summaryProperty);
                        }
                        else
                        {
                            p.EntityPropertyDataCollection.Add(entityProperty);
                        }
                    }
                }
            }
        }

        public new void Add(SummaryProperty data)
        {
            base.Add(data);
            data.PropertyChanged += Data_AnyPropertyChanged;
        }

        private void Data_AnyPropertyChanged(object sender, EventArgs e)
        {
            //AllPropertyChangedRaise();
            foreach (SummaryProperty summaryProperty in this)
            {
                foreach (IntellectualEntityProperty property in summaryProperty.EntityPropertyDataCollection)
                {
                    OnPropertyChanged(new PropertyChangedEventArgs(property.Name));
                }
            }
        }

        /// <summary>
        /// Вызов события изменения для каждого свойства объекта
        /// </summary>
        protected void AllPropertyChangedRaise()
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

    //todo move to file
    public class SummaryProperty : INotifyPropertyChanged
    {
        public SummaryProperty()
        {
            EntityPropertyDataCollection = new ObservableCollection<IntellectualEntityProperty>();
        }

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

        public object SummaryValue
        {
            get
            {
                var values = EntityPropertyDataCollection.Select(e => e.Value).ToList();
                if (values.Distinct().Count() > 1)
                    return "*DIFFERENT*"; //todo localization
                return values.FirstOrDefault();
            }
            set
            {
                foreach (IntellectualEntityProperty property in EntityPropertyDataCollection)
                {
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
