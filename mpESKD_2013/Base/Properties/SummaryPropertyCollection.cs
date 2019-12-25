﻿namespace mpESKD.Base.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using ModPlusAPI;
    using ModPlusAPI.Windows;

    public class SummaryPropertyCollection : ObservableCollection<SummaryProperty>
    {
        public SummaryPropertyCollection(IEnumerable<ObjectId> objectIds)
        {
            foreach (ObjectId objectId in objectIds)
            {
                EntityPropertyProvider entityPropertyProvider = new EntityPropertyProvider(objectId);
                if (entityPropertyProvider.IsValid)
                {
                    entityPropertyProvider.OnLockedLayerEventHandler += EntityPropertyProviderOnOnLockedLayerEventHandler;
                    foreach (IntellectualEntityProperty entityProperty in entityPropertyProvider.Properties)
                    {
                        var allowableSummaryProperty = this.FirstOrDefault(
                            si => si.EntityPropertyDataCollection
                                .Any(ep => ep.Category == entityProperty.Category &&
                                           ep.EntityType == entityProperty.EntityType &&
                                           ep.Name == entityProperty.Name &&
                                           ep.Value.GetType() == entityProperty.Value.GetType()));
                        if (allowableSummaryProperty == null)
                        {
                            SummaryProperty summaryProperty = new SummaryProperty(entityPropertyProvider.EntityType);
                            summaryProperty.AddProperty(entityProperty);
                            Add(summaryProperty);
                        }
                        else
                        {
                            allowableSummaryProperty.AddProperty(entityProperty);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Событие, происходящее при попытке отредактировать свойство примитива, находящегося на заблокированном слое
        /// </summary>
        public event EventHandler OnLockedLayerEventHandler;

        public new void Add(SummaryProperty data)
        {
            base.Add(data);
            data.PropertyChanged += Data_AnyPropertyChanged;
        }

        private bool _hasObjectOnLockedLayer = false;

        private void Data_AnyPropertyChanged(object sender, EventArgs e)
        {
            foreach (SummaryProperty summaryProperty in this)
            {
                foreach (IntellectualEntityProperty property in summaryProperty.EntityPropertyDataCollection)
                {
                    OnPropertyChanged(new PropertyChangedEventArgs(property.Name));
                }
            }

            if (_hasObjectOnLockedLayer)
            {
                _hasObjectOnLockedLayer = false;
                OnLockedLayerEventHandler?.Invoke(this, EventArgs.Empty);

                // Один или несколько объектов расположены на заблокированном слое, обновить их невозможно
                MessageBox.Show(Language.GetItem("mpESKD", "h104"), MessageBoxIcon.Alert);
            }
        }

        private void EntityPropertyProviderOnOnLockedLayerEventHandler(object sender, IntellectualEntityProperty e)
        {
            _hasObjectOnLockedLayer = true;
        }
    }
}
