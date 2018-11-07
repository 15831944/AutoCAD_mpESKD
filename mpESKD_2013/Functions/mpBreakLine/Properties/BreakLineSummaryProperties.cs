﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;
using mpESKD.Base.Properties;

namespace mpESKD.Functions.mpBreakLine.Properties
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class BreakLineSummaryProperties: BaseSummaryProperties<BreakLinePropertiesData>
    {
        
        public int? Overhang
        {
            get => GetIntProp(nameof(Overhang));
            set
            {
                SetPropValue(nameof(Overhang), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Overhang)));
            }
        }
        public int? BreakHeight
        {
            get => GetIntProp(nameof(BreakHeight));
            set
            {
                SetPropValue(nameof(BreakHeight), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(BreakHeight)));
            }
        }
        public int? BreakWidth
        {
            get => GetIntProp(nameof(BreakWidth));
            set
            {
                SetPropValue(nameof(BreakWidth), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(BreakWidth)));
            }
        }
        public string BreakLineType
        {
            get => GetStrProp(nameof(BreakLineType));
            set
            {
                SetPropValue(nameof(BreakLineType), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(BreakLineType)));
            }
        }
        
        public BreakLineSummaryProperties(IEnumerable<ObjectId> objectIds)
        {
            foreach (ObjectId objectId in objectIds)
            {
                BreakLinePropertiesData data = new BreakLinePropertiesData(objectId);
                if (data.IsValid)
                    Add(data);
            }
        }

        public new void Add(BreakLinePropertiesData data)
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