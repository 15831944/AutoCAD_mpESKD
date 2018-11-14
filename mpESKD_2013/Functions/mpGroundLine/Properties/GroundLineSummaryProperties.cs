//todo remove it all
namespace mpESKD.Functions.mpGroundLine.Properties
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Autodesk.AutoCAD.DatabaseServices;
    using Base.Properties;

    public class GroundLineSummaryProperties : BaseSummaryProperties<GroundLinePropertiesData>
    {
        public GroundLineSummaryProperties(IEnumerable<ObjectId> objectIds)
        {
            foreach (ObjectId objectId in objectIds)
            {
                GroundLinePropertiesData data = new GroundLinePropertiesData(objectId);
                if (data.IsValid)
                    Add(data);
            }
        }

        #region Properties

        /// <summary>
        /// Отступ первого штриха в каждом сегменте полилинии
        /// </summary>
        public string FirstStrokeOffset
        {
            get => GetStrProp(nameof(FirstStrokeOffset));
            set
            {
                SetPropValue(nameof(FirstStrokeOffset), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(FirstStrokeOffset)));
            }
        }

        /// <summary>
        /// Длина штриха
        /// </summary>
        public int? StrokeLength
        {
            get => GetIntProp(nameof(StrokeLength));
            set
            {
                SetPropValue(nameof(StrokeLength), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(StrokeLength)));
            }
        }

        /// <summary>
        /// Расстояние между штрихами
        /// </summary>
        public int? StrokeOffset
        {
            get => GetIntProp(nameof(StrokeOffset));
            set
            {
                SetPropValue(nameof(StrokeOffset), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(StrokeOffset)));
            }
        }

        /// <summary>
        /// Угол наклона штриха в градусах
        /// </summary>
        public int? StrokeAngle
        {
            get => GetIntProp(nameof(StrokeAngle));
            set
            {
                SetPropValue(nameof(StrokeAngle), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(StrokeAngle)));
            }
        }

        /// <summary>
        /// Отступ группы штрихов
        /// </summary>
        public int? Space
        {
            get => GetIntProp(nameof(Space));
            set
            {
                SetPropValue(nameof(Space), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Space)));
            }
        }

        #endregion

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
        
        public new void Add(GroundLinePropertiesData data)
        {
            base.Add(data);
            data.AnyPropertyChanged += Data_AnyPropertyChanged;
        }

        private void Data_AnyPropertyChanged(object sender, EventArgs e)
        {
            AllPropertyChangedReise();
        }
    }
}
