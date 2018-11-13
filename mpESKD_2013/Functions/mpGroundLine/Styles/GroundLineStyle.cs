namespace mpESKD.Functions.mpGroundLine.Styles
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Base.Enums;
    using Base.Helpers;
    using Base.Properties;
    using Base.Styles;
    using ModPlusAPI;
    using Properties;

    public class GroundLineStyle : MPCOStyle
    {
        private static GroundLineStyle _instance;

        public static GroundLineStyle Instance => _instance ?? (_instance = new GroundLineStyle());
        
        public override List<T> CreateSystemStyles<T>()
        {
            var styles = new List<GroundLineStyle>();
            var style = new GroundLineStyle
            {
                Name = GroundLineFunction.MPCOEntDisplayName,
                FunctionName = GroundLineFunction.MPCOEntName,
                Description = Language.GetItem(MainFunction.LangItem, "h78"), // "Базовый стиль для линии грунта",
                Guid = "00000000-0000-0000-0000-000000000000",
                StyleType = MPCOStyleType.System
            };
            style.Properties.Add(GroundLineProperties.FirstStrokeOffset.Clone(true));
            style.Properties.Add(GroundLineProperties.StrokeLength.Clone(true));
            style.Properties.Add(GroundLineProperties.StrokeOffset.Clone(true));
            style.Properties.Add(GroundLineProperties.StrokeAngle.Clone(true));
            style.Properties.Add(GroundLineProperties.Space.Clone(true));
            style.Properties.Add(GroundLineProperties.LineType.Clone(true));
            style.Properties.Add(GroundLineProperties.LineTypeScale.Clone(true));
            style.Properties.Add(GroundLineProperties.Scale.Clone(true));
            style.Properties.Add(GroundLineProperties.LayerName.Clone(true));

            styles.Add(style);

            return styles.Cast<T>().ToList();
        }

        public override T ParseStyleFromXElement<T>(XElement styleXel)
        {
            var style = new T()
            {
                StyleType = MPCOStyleType.User,
                FunctionName = GroundLineFunction.MPCOEntName
            };

            // Properties
            foreach (XElement propXel in styleXel.Elements("Property"))
            {
                var nameAttr = propXel.Attribute("Name");
                if (nameAttr != null)
                {
                    switch (nameAttr.Value)
                    {
                        case "FirstStrokeOffset":
                            style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, GroundLineProperties.FirstStrokeOffset,
                                GroundLinePropertiesHelpers.GetFirstStrokeOffsetFromString(propXel.Attribute("Value")?.Value)));
                            break;
                        case "StrokeLength":
                            style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, GroundLineProperties.StrokeLength));
                            break;
                        case "StrokeOffset":
                            style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, GroundLineProperties.StrokeOffset));
                            break;
                        case "StrokeAngle":
                            style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, GroundLineProperties.StrokeAngle));
                            break;
                        case "Space":
                            style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, GroundLineProperties.Space));
                            break;
                        // general
                        case "LineType":
                            style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, GroundLineProperties.LineType));
                            break;
                        case "LineTypeScale":
                            style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, GroundLineProperties.LineTypeScale));
                            break;
                        case "LayerName":
                            style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, GroundLineProperties.LayerName));
                            break;
                        case "Scale":
                            style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, GroundLineProperties.Scale,
                                Parsers.AnnotationScaleFromString(propXel.Attribute("Value")?.Value)));
                            break;
                    }
                }
            }

            return style;
        }
    }

    public class GroundLineStyleForEditor : MPCOStyleForEditor
    {
        public GroundLineStyleForEditor(MPCOStyle style, string currentStyleGuid, StyleToBind parent) 
            : base(style, currentStyleGuid, parent)
        {
            FirstStrokeOffset = StyleHelpers.GetPropertyValue<GroundLineFirstStrokeOffset>(
                style, nameof(FirstStrokeOffset), GroundLineProperties.FirstStrokeOffset.DefaultValue);
            StrokeLength = StyleHelpers.GetPropertyValue(style, nameof(StrokeLength),
                GroundLineProperties.StrokeLength.DefaultValue);
            StrokeOffset = StyleHelpers.GetPropertyValue(style, nameof(StrokeOffset),
                GroundLineProperties.StrokeOffset.DefaultValue);
            StrokeAngle = StyleHelpers.GetPropertyValue(style, nameof(StrokeAngle),
                GroundLineProperties.StrokeAngle.DefaultValue);
            Space = StyleHelpers.GetPropertyValue(style, nameof(Space),
                GroundLineProperties.Space.DefaultValue);
            LineTypeScale = StyleHelpers.GetPropertyValue(style, nameof(LineTypeScale),
                GroundLineProperties.LineTypeScale.DefaultValue);
            LineType = StyleHelpers.GetPropertyValue(style, nameof(LineType),
                GroundLineProperties.LineType.DefaultValue);
            LayerName = StyleHelpers.GetPropertyValue(style, nameof(LayerName),
                GroundLineProperties.LayerName.DefaultValue);
            Scale = StyleHelpers.GetPropertyValue<AnnotationScale>(style, nameof(Scale),
                GroundLineProperties.Scale.DefaultValue);
        }

        public GroundLineStyleForEditor(StyleToBind parent) : base(parent)
        {
            FirstStrokeOffset = GroundLineProperties.FirstStrokeOffset.DefaultValue;
            StrokeLength = GroundLineProperties.StrokeLength.DefaultValue;
            StrokeOffset = GroundLineProperties.StrokeOffset.DefaultValue;
            StrokeAngle = GroundLineProperties.StrokeAngle.DefaultValue;
            Space = GroundLineProperties.Space.DefaultValue;

            // general
            LineTypeScale = GroundLineProperties.LineTypeScale.DefaultValue;
            LineType = GroundLineProperties.LineType.DefaultValue;
            LayerName = GroundLineProperties.LayerName.DefaultValue;
            Scale = GroundLineProperties.Scale.DefaultValue;
        }

        /// <summary>
        /// Отступ первого штриха в каждом сегменте полилинии
        /// </summary>
        public GroundLineFirstStrokeOffset FirstStrokeOffset { get; set; }

        /// <summary>
        /// Длина штриха
        /// </summary>
        public int StrokeLength { get; set; }

        /// <summary>
        /// Расстояние между штрихами
        /// </summary>
        public int StrokeOffset { get; set; }

        /// <summary>
        /// Угол наклона штриха в градусах
        /// </summary>
        public int StrokeAngle { get; set; }

        /// <summary>
        /// Отступ группы штрихов
        /// </summary>
        public int Space { get; set; }

        /// <summary>Получение стилей в виде классов-презенторов для редактора</summary>
        /// <returns></returns>
        public static List<GroundLineStyleForEditor> GetStylesForEditor()
        {
            var stylesForEditor = new List<GroundLineStyleForEditor>();

            foreach (GroundLineStyle groundLineStyle in StyleManager.GetStyles<GroundLineStyle>())
            {
                stylesForEditor.Add(new GroundLineStyleForEditor(groundLineStyle, StyleManager.GetCurrentStyleGuid(typeof(GroundLineStyle)), null));
            }

            return stylesForEditor;
        }

        public static XElement ConvertStyleForEditorToXElement(GroundLineStyleForEditor style)
        {
            XElement styleXel = new XElement("UserStyle");
            styleXel.SetAttributeValue(nameof(style.Name), style.Name);
            styleXel.SetAttributeValue(nameof(style.Description), style.Description);
            styleXel.SetAttributeValue(nameof(style.Guid), style.Guid);
            // Properties
            // Цифровые и текстовые значения сохранять через словарь
            var properties = new Dictionary<string, object>
            {
                {nameof(style.FirstStrokeOffset), style.FirstStrokeOffset},
                {nameof(style.StrokeLength), style.StrokeLength},
                {nameof(style.StrokeOffset), style.StrokeOffset},
                {nameof(style.StrokeAngle), style.StrokeAngle},
                {nameof(style.Space), style.Space},
                //
                {nameof(style.LineType), style.LineType },
                {nameof(style.LineTypeScale), style.LineTypeScale },
                {nameof(style.LayerName), style.LayerName }
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

            return styleXel;
        }
    }
}