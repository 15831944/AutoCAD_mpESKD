namespace mpESKD.Functions.mpAxis.Styles
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Base.Helpers;
    using mpESKD.Base.Properties;
    using mpESKD.Base.Styles;
    using Properties;
    using ModPlusAPI;
    using ModPlusAPI.Windows;

    public class AxisStyle : MPCOStyle
    {
        private static AxisStyle _instance;

        public static AxisStyle Instance => _instance ?? (_instance = new AxisStyle());

        public override List<T> CreateSystemStyles<T>()
        {
            var styles = new List<AxisStyle>();
            var style = new AxisStyle
            {
                Name = Language.GetItem(MainFunction.LangItem, "h41"), // "Прямая ось",
                FunctionName = AxisInterface.Name,
                Description = Language.GetItem(MainFunction.LangItem, "h68"), // "Базовый стиль для прямой оси",
                Guid = "00000000-0000-0000-0000-000000000000",
                StyleType = MPCOStyleType.System
            };

            //style.Properties.Add(AxisProperties.MarkersDiameter.Clone(true));
            //style.Properties.Add(AxisProperties.BottomFractureOffset.Clone(true));
            //style.Properties.Add(AxisProperties.TopFractureOffset.Clone(true));
            //style.Properties.Add(AxisProperties.MarkersCount.Clone(true));
            //style.Properties.Add(AxisProperties.FirstMarkerType.Clone(true));
            //style.Properties.Add(AxisProperties.SecondMarkerType.Clone(true));
            //style.Properties.Add(AxisProperties.ThirdMarkerType.Clone(true));
            //style.Properties.Add(AxisProperties.OrientMarkerType.Clone(true));
            //style.Properties.Add(AxisProperties.Fracture.Clone(true));
            //style.Properties.Add(AxisProperties.ArrowsSize.Clone(true));
            //style.Properties.Add(AxisProperties.LineTypeScale.Clone(true));
            //style.Properties.Add(AxisProperties.LineType.Clone(true));
            //style.Properties.Add(AxisProperties.LayerName.Clone(true));
            //style.Properties.Add(AxisProperties.TextStyle.Clone(true));
            //style.Properties.Add(AxisProperties.TextHeight.Clone(true));
            //style.Properties.Add(AxisProperties.MarkersPosition.Clone(true));
            //style.Properties.Add(AxisProperties.Scale.Clone(true));

            styles.Add(style);

            return styles.Cast<T>().ToList();
        }

        public override T ParseStyleFromXElement<T>(XElement styleXel)
        {
            var style = new T()
            {
                StyleType = MPCOStyleType.User,
                FunctionName = AxisInterface.Name
            };

            // Properties
            foreach (XElement propXel in styleXel.Elements("Property"))
            {
                var nameAttr = propXel.Attribute("Name");
                if (nameAttr != null)
                {
                    switch (nameAttr.Value)
                    {
                        //case "MarkersPosition":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.MarkersPosition,
                        //        AxisPropertiesHelpers.GetAxisMarkersPositionFromString(propXel.Attribute("Value")?.Value)));
                        //    break;
                        //case "MarkersDiameter":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.MarkersDiameter));
                        //    break;
                        //case "MarkersCount":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.MarkersCount));
                        //    break;
                        //case "FirstMarkerType":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.FirstMarkerType));
                        //    break;
                        //case "SecondMarkerType":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.SecondMarkerType));
                        //    break;
                        //case "ThirdMarkerType":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.ThirdMarkerType));
                        //    break;
                        //case "OrientMarkerType":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.OrientMarkerType));
                        //    break;
                        //case "Fracture":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.Fracture));
                        //    break;
                        //case "BottomFractureOffset":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.BottomFractureOffset));
                        //    break;
                        //case "TopFractureOffset":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.TopFractureOffset));
                        //    break;
                        //case "ArrowsSize":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.ArrowsSize));
                        //    break;
                        //case "LineTypeScale":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.LineTypeScale));
                        //    break;
                        //case "LineType":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.LineType));
                        //    break;
                        //case "LayerName":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.LayerName));
                        //    break;
                        //case "TextStyle":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.TextStyle));
                        //    break;
                        //case "TextHeight":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.TextHeight));
                        //    break;
                        //case "Scale":
                        //    style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.Scale,
                        //        Parsers.AnnotationScaleFromString(propXel.Attribute("Value")?.Value)));
                        //    break;
                    }
                }
            }

            return style;
        }
    }

    public class AxisStyleForEditor : MPCOStyleForEditor
    {
        public AxisStyleForEditor(MPCOStyle style, string currentStyleGuid, StyleToBind parent) : base(style, currentStyleGuid, parent)
        {
            // Properties
            //Fracture = StyleHelpers.GetPropertyValue(style, nameof(Fracture),
            //    AxisProperties.Fracture.DefaultValue);
            //BottomFractureOffset = StyleHelpers.GetPropertyValue(style, nameof(BottomFractureOffset),
            //    AxisProperties.BottomFractureOffset.DefaultValue);
            //TopFractureOffset = StyleHelpers.GetPropertyValue(style, nameof(TopFractureOffset),
            //    AxisProperties.TopFractureOffset.DefaultValue);
            //MarkersPosition = StyleHelpers.GetPropertyValue(style, nameof(MarkersPosition),
            //    AxisProperties.MarkersPosition.DefaultValue);
            //MarkersDiameter = StyleHelpers.GetPropertyValue(style, nameof(MarkersDiameter),
            //    AxisProperties.MarkersDiameter.DefaultValue);
            //MarkersCount = StyleHelpers.GetPropertyValue(style, nameof(MarkersCount),
            //    AxisProperties.MarkersCount.DefaultValue);
            //FirstMarkerType = StyleHelpers.GetPropertyValue(style, nameof(FirstMarkerType),
            //    AxisProperties.FirstMarkerType.DefaultValue);
            //SecondMarkerType = StyleHelpers.GetPropertyValue(style, nameof(SecondMarkerType),
            //    AxisProperties.SecondMarkerType.DefaultValue);
            //ThirdMarkerType = StyleHelpers.GetPropertyValue(style, nameof(ThirdMarkerType),
            //    AxisProperties.ThirdMarkerType.DefaultValue);
            //ArrowsSize = StyleHelpers.GetPropertyValue(style, nameof(ArrowsSize),
            //    AxisProperties.ArrowsSize.DefaultValue);
            //LineTypeScale = StyleHelpers.GetPropertyValue(style, nameof(LineTypeScale),
            //    AxisProperties.LineTypeScale.DefaultValue);
            //LineType = StyleHelpers.GetPropertyValue(style, nameof(LineType),
            //    AxisProperties.LineType.DefaultValue);
            //TextStyle = StyleHelpers.GetPropertyValue(style, nameof(TextStyle),
            //    AxisProperties.TextStyle.DefaultValue);
            //TextHeight = StyleHelpers.GetPropertyValue(style, nameof(TextHeight),
            //    AxisProperties.TextHeight.DefaultValue);
            //LayerName = StyleHelpers.GetPropertyValue(style, nameof(LayerName),
            //    AxisProperties.LayerName.DefaultValue);
            //Scale = StyleHelpers.GetPropertyValue<AnnotationScale>(style, nameof(Scale),
            //    AxisProperties.Scale.DefaultValue);
            //LayerXmlData = style.LayerXmlData;
            //TextStyleXmlData = ((AxisStyle)style).TextStyleXmlData;
        }

        public AxisStyleForEditor(StyleToBind parent) : base(parent)
        {
            // Properties
            //Fracture = AxisProperties.Fracture.DefaultValue;
            //BottomFractureOffset = AxisProperties.BottomFractureOffset.DefaultValue;
            //TopFractureOffset = AxisProperties.TopFractureOffset.DefaultValue;
            //MarkersPosition = AxisProperties.MarkersPosition.DefaultValue;
            //MarkersDiameter = AxisProperties.MarkersDiameter.DefaultValue;
            //MarkersCount = AxisProperties.MarkersCount.DefaultValue;
            //ArrowsSize = AxisProperties.ArrowsSize.DefaultValue;
            //FirstMarkerType = AxisProperties.FirstMarkerType.DefaultValue;
            //SecondMarkerType = AxisProperties.SecondMarkerType.DefaultValue;
            //ThirdMarkerType = AxisProperties.ThirdMarkerType.DefaultValue;
            //TextStyle = AxisProperties.TextStyle.DefaultValue;
            //TextHeight = AxisProperties.TextHeight.DefaultValue;
            //LineTypeScale = AxisProperties.LineTypeScale.DefaultValue;
            //LineType = AxisProperties.LineType.DefaultValue;
            //LayerName = AxisProperties.LayerName.DefaultValue;
            //Scale = AxisProperties.Scale.DefaultValue;
        }

        #region Properties

        // Позиция маркеров
        //public AxisMarkersPosition MarkersPosition { get; set; }

        // Диаметр маркеров
        public int MarkersDiameter { get; set; }

        // Количество маркеров
        public int MarkersCount { get; set; }

        // Типы маркеров
        public int FirstMarkerType { get; set; }

        public int SecondMarkerType { get; set; }

        public int ThirdMarkerType { get; set; }

        public int OrientMarkerType { get; set; }

        // Излом
        public int Fracture { get; set; }

        // Отступы излома
        public int BottomFractureOffset { get; set; }

        public int TopFractureOffset { get; set; }

        // Arrow size
        public int ArrowsSize { get; set; }

        // Text
        public string TextStyle { get; set; }

        public double TextHeight { get; set; }

        #endregion

        /// <summary>Получение стилей в виде классов-презенторов для редактора</summary>
        /// <returns></returns>
        public static List<AxisStyleForEditor> GetStylesForEditor()
        {
            var stylesForEditor = new List<AxisStyleForEditor>();

            foreach (AxisStyle axisStyle in StyleManager.GetStyles<AxisStyle>())
            {
                stylesForEditor.Add(new AxisStyleForEditor(axisStyle, StyleManager.GetCurrentStyleGuid(typeof(AxisStyle)), null));
            }

            return stylesForEditor;
        }

        public static XElement ConvertStyleForEditorToXElement(AxisStyleForEditor style)
        {
            XElement styleXel = new XElement("UserStyle");
            styleXel.SetAttributeValue(nameof(style.Name), style.Name);
            styleXel.SetAttributeValue(nameof(style.Description), style.Description);
            styleXel.SetAttributeValue(nameof(style.Guid), style.Guid);
            // Properties
            // Цифровые и текстовые значения сохранять через словарь
            var properties = new Dictionary<string, object>
                    {
                        //{nameof(style.MarkersPosition), style.MarkersPosition},
                        //{nameof(style.MarkersDiameter), style.MarkersDiameter },
                        //{nameof(style.MarkersCount), style.MarkersCount },
                        //{nameof(style.FirstMarkerType), style.FirstMarkerType },
                        //{nameof(style.SecondMarkerType), style.SecondMarkerType },
                        //{nameof(style.ThirdMarkerType), style.ThirdMarkerType },
                        //{nameof(style.OrientMarkerType), style.OrientMarkerType },
                        //{nameof(style.BottomFractureOffset), style.BottomFractureOffset },
                        //{nameof(style.TopFractureOffset), style.TopFractureOffset },
                        //{nameof(style.ArrowsSize), style.ArrowsSize },
                        //{nameof(style.Fracture), style.Fracture },
                        //{nameof(style.TextStyle), style.TextStyle },
                        //{nameof(style.TextHeight), style.TextHeight },
                        ////
                        //{nameof(style.LineType), style.LineType },
                        //{nameof(style.LineTypeScale), style.LineTypeScale },
                        //{nameof(style.LayerName), style.LayerName }
                    };
            foreach (KeyValuePair<string, object> property in properties)
                styleXel.Add(StyleHelpers.CreateXElementFromProperty(property));
            // Масштаб сохранять отдельно
            var propXel = new XElement("Property");
            propXel.SetAttributeValue("Name", nameof(style.Scale));
            propXel.SetAttributeValue("PropertyType", style.Scale.GetType().Name);
            propXel.SetAttributeValue("Value", style.Scale.Name);
            styleXel.Add(propXel);
            // add layer
            if (LayerHelper.HasLayer(style.LayerName))
                styleXel.Add(LayerHelper.SetLayerXml(LayerHelper.GetLayerTableRecordByLayerName(style.LayerName)));
            else if (style.LayerXmlData != null) styleXel.Add(style.LayerXmlData);
            // add text style
            if (TextStyleHelper.HasTextStyle(style.TextStyle))
                styleXel.Add(TextStyleHelper.SetTextStyleTableRecordXElement(TextStyleHelper.GetTextStyleTableRecordByName(style.TextStyle)));
            else if (style.TextStyleXmlData != null) styleXel.Add(style.TextStyleXmlData);

            return styleXel;
        }
    }
}
