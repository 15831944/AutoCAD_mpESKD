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
        /// <summary>Нижний отступ излома</summary>
        public int? BottomFractureOffset
        {
            get => GetIntProp(nameof(BottomFractureOffset));
            set
            {
                SetPropValue(nameof(BottomFractureOffset), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(BottomFractureOffset)));
            }
        }
        /// <summary>Верхний отступ излома</summary>
        public int? TopFractureOffset
        {
            get => GetIntProp(nameof(TopFractureOffset));
            set
            {
                SetPropValue(nameof(TopFractureOffset), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(TopFractureOffset)));
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
        /// <summary>Текстовый стиль</summary>
        public string TextStyle
        {
            get => GetStrProp(nameof(TextStyle));
            set
            {
                SetPropValue(nameof(TextStyle), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(TextStyle)));
            }
        }
        /// <summary>Высота текста</summary>
        public double? TextHeight
        {
            get => GetDoubleProp(nameof(TextHeight));
            set
            {
                SetPropValue(nameof(TextHeight), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(TextHeight)));
            }
        }

        #region Text

        public string FirstTextPrefix
        {
            get => GetStrProp(nameof(FirstTextPrefix));
            set
            {
                SetPropValue(nameof(FirstTextPrefix), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(FirstTextPrefix)));
            }
        }
        public string FirstText
        {
            get => GetStrProp(nameof(FirstText));
            set
            {
                SetPropValue(nameof(FirstText), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(FirstText)));
            }
        }
        public string FirstTextSuffix
        {
            get => GetStrProp(nameof(FirstTextSuffix));
            set
            {
                SetPropValue(nameof(FirstTextSuffix), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(FirstTextSuffix)));
            }
        }
        public string SecondTextPrefix
        {
            get => GetStrProp(nameof(SecondTextPrefix));
            set
            {
                SetPropValue(nameof(SecondTextPrefix), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SecondTextPrefix)));
            }
        }
        public string SecondText
        {
            get => GetStrProp(nameof(SecondText));
            set
            {
                SetPropValue(nameof(SecondText), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SecondText)));
            }
        }
        public string SecondTextSuffix
        {
            get => GetStrProp(nameof(SecondTextSuffix));
            set
            {
                SetPropValue(nameof(SecondTextSuffix), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SecondTextSuffix)));
            }
        }
        public string ThirdTextPrefix
        {
            get => GetStrProp(nameof(ThirdTextPrefix));
            set
            {
                SetPropValue(nameof(ThirdTextPrefix), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ThirdTextPrefix)));
            }
        }
        public string ThirdText
        {
            get => GetStrProp(nameof(ThirdText));
            set
            {
                SetPropValue(nameof(ThirdText), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ThirdText)));
            }
        }
        public string ThirdTextSuffix
        {
            get => GetStrProp(nameof(ThirdTextSuffix));
            set
            {
                SetPropValue(nameof(ThirdTextSuffix), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ThirdTextSuffix)));
            }
        }

        #endregion

        #region Типы маркеров
        /// <summary>Тип первого маркера</summary>
        public string FirstMarkerType
        {
            get => GetStrProp(nameof(FirstMarkerType));
            set
            {
                SetPropValue(nameof(FirstMarkerType), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(FirstMarkerType)));
            }
        }
        /// <summary>Тип второго маркера</summary>
        public string SecondMarkerType
        {
            get => GetStrProp(nameof(SecondMarkerType));
            set
            {
                SetPropValue(nameof(SecondMarkerType), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SecondMarkerType)));
            }
        }
        /// <summary>Тип третьего маркера</summary>
        public string ThirdMarkerType
        {
            get => GetStrProp(nameof(ThirdMarkerType));
            set
            {
                SetPropValue(nameof(ThirdMarkerType), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ThirdMarkerType)));
            }
        }
        #endregion

        #region Orient markers

        public int? ArrowSize
        {
            get => GetIntProp(nameof(ArrowSize));
            set
            {
                SetPropValue(nameof(ArrowSize), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ArrowSize)));
            }
        }

        public string BottomOrientText
        {
            get => GetStrProp(nameof(BottomOrientText));
            set
            {
                SetPropValue(nameof(BottomOrientText), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(BottomOrientText)));
            }
        }

        public string TopOrientText
        {
            get => GetStrProp(nameof(TopOrientText));
            set
            {
                SetPropValue(nameof(TopOrientText), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(TopOrientText)));
            }
        }
        
        public bool? BottomOrientMarkerVisible
        {
            get => GetBoolProp(nameof(BottomOrientMarkerVisible));
            set
            {
                SetPropValue(nameof(BottomOrientMarkerVisible), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(BottomOrientMarkerVisible)));
            }
        }

        public bool? TopOrientMarkerVisible
        {
            get => GetBoolProp(nameof(TopOrientMarkerVisible));
            set
            {
                SetPropValue(nameof(TopOrientMarkerVisible), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(TopOrientMarkerVisible)));
            }
        }

        public string OrientMarkerType
        {
            get => GetStrProp(nameof(OrientMarkerType));
            set
            {
                SetPropValue(nameof(OrientMarkerType), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrientMarkerType)));
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
        public new string Scale
        {
            get => GetStrProp(nameof(Scale));
            set
            {
                SetPropValue(nameof(Scale), value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Scale)));
                if (MainStaticSettings.Settings.AxisLineTypeScaleProportionScale)
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(LineTypeScale)));
            }
        }

        public AxisSummaryProperties(IEnumerable<ObjectId> objectIds, out int maxCount)
        {
            maxCount = 1;
            foreach (ObjectId objectId in objectIds)
            {
                AxisPropertiesData data = new AxisPropertiesData(objectId);
                if (data.MarkersCount > maxCount)
                    maxCount = data.MarkersCount;
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
    }
}
