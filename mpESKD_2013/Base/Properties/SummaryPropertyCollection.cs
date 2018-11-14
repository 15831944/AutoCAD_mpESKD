namespace mpESKD.Base.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;

    public class SummaryPropertyCollection : ObservableCollection<SummaryProperty>
    {
        public SummaryPropertyCollection(IEnumerable<ObjectId> objectIds)
        {
            foreach (ObjectId objectId in objectIds)
            {
                EntityPropertyData data = new EntityPropertyData(objectId);
                if (data.IsValid)
                {
                    foreach (IntellectualEntityProperty entityProperty in data.Properties)
                    {
                        var p = this.FirstOrDefault(si => si.EntityPropertyDataCollection
                            .Any(ep => ep.Category == entityProperty.Category &&
                                       ep.Name == entityProperty.Name &&
                                       ep.Value.GetType() == entityProperty.Value.GetType()));
                        if (p == null)
                        {
                            SummaryProperty summaryProperty = new SummaryProperty(data.EntityName);
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
            foreach (SummaryProperty summaryProperty in this)
            {
                foreach (IntellectualEntityProperty property in summaryProperty.EntityPropertyDataCollection)
                {
                    OnPropertyChanged(new PropertyChangedEventArgs(property.Name));
                }
            }
        }
    }
}
