namespace mpESKD.Functions.mpBreakLine.Styles
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Base.Helpers;
    using mpESKD.Base.Properties;
    using mpESKD.Base.Styles;
    using Properties;
    using ModPlusAPI;

    public class BreakLineStyle : MPCOStyle
    {
        private static BreakLineStyle _instance;

        public static BreakLineStyle Instance => _instance ?? (_instance = new BreakLineStyle());

        public override List<T> CreateSystemStyles<T>()
        {
            var styles = new List<BreakLineStyle>();
            var style = new BreakLineStyle
            {
                Name = Language.GetItem(MainFunction.LangItem, "h48") , // "Линия обрыва"
                FunctionName = BreakLineInterface.Name,
                Description = Language.GetItem(MainFunction.LangItem, "h53"), // "Базовый стиль для линии обрыва"
                Guid = "00000000-0000-0000-0000-000000000000",
                StyleType = MPCOStyleType.System
            };
            style.Properties.Add(BreakLineProperties.Overhang.Clone(true));
            style.Properties.Add(BreakLineProperties.BreakWidth.Clone(true));
            style.Properties.Add(BreakLineProperties.BreakHeight.Clone(true));
            style.Properties.Add(BreakLineProperties.LineTypeScale.Clone(true));
            style.Properties.Add(BreakLineProperties.LayerName.Clone(true));
            style.Properties.Add(BreakLineProperties.BreakLineType.Clone(true));
            style.Properties.Add(BreakLineProperties.Scale.Clone(true));

            styles.Add(style);

            return styles.Cast<T>().ToList();
        }

        public override T ParseStyleFromXElement<T>(XElement styleXel)
        {
            var style = new T()
            {
                StyleType = MPCOStyleType.User,
                FunctionName = BreakLineInterface.Name
            };

            // Properties
            foreach (XElement propXel in styleXel.Elements("Property"))
            {
                var nameAttr = propXel.Attribute("Name");
                if (nameAttr != null)
                {
                    switch (nameAttr.Value)
                    {
                        case "Overhang":
                            style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, BreakLineProperties.Overhang));
                            break;
                        case "BreakHeight":
                            style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, BreakLineProperties.BreakHeight));
                            break;
                        case "BreakWidth":
                            style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, BreakLineProperties.BreakWidth));
                            break;
                        case "LineTypeScale":
                            style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, BreakLineProperties.LineTypeScale));
                            break;
                        case "LayerName":
                            style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, BreakLineProperties.LayerName));
                            break;
                        case "Scale":
                            style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, BreakLineProperties.Scale,
                                Parsers.AnnotationScaleFromString(propXel.Attribute("Value")?.Value)));
                            break;
                    }
                }
            }

            return style;
        }
    }

    public class BreakLineStyleForEditor : MPCOStyleForEditor
    {
        public BreakLineStyleForEditor(MPCOStyle style, string currentStyleGuid, StyleToBind parent) : base(style, currentStyleGuid, parent)
        {
            // Properties
            Overhang = StyleHelpers.GetPropertyValue(style, nameof(Overhang),
                BreakLineProperties.Overhang.DefaultValue);
            BreakWidth = StyleHelpers.GetPropertyValue(style, nameof(BreakWidth),
                BreakLineProperties.BreakWidth.DefaultValue);
            BreakHeight = StyleHelpers.GetPropertyValue(style, nameof(BreakHeight),
                BreakLineProperties.BreakHeight.DefaultValue);
            LineTypeScale = StyleHelpers.GetPropertyValue(style, nameof(LineTypeScale),
                BreakLineProperties.LineTypeScale.DefaultValue);
            LayerName = StyleHelpers.GetPropertyValue(style, nameof(LayerName),
                BreakLineProperties.LayerName.DefaultValue);
            Scale = StyleHelpers.GetPropertyValue<AnnotationScale>(style, nameof(Scale),
                BreakLineProperties.Scale.DefaultValue);
            LayerXmlData = style.LayerXmlData;
        }

        public BreakLineStyleForEditor(StyleToBind parent) : base(parent)
        {
            // Properties
            Overhang = BreakLineProperties.Overhang.DefaultValue;
            BreakWidth = BreakLineProperties.BreakWidth.DefaultValue;
            BreakHeight = BreakLineProperties.BreakHeight.DefaultValue;
            LineTypeScale = BreakLineProperties.LineTypeScale.DefaultValue;
            LayerName = BreakLineProperties.LayerName.DefaultValue;
            Scale = BreakLineProperties.Scale.DefaultValue;
        }

        #region Properties
        
        public int Overhang { get; set; }
        
        public int BreakHeight { get; set; }
        
        public int BreakWidth { get; set; }
        
        #endregion

        /// <summary>Получение стилей в виде классов-презенторов для редактора</summary>
        /// <returns></returns>
        public static List<BreakLineStyleForEditor> GetStylesForEditor()
        {
            var stylesForEditor = new List<BreakLineStyleForEditor>();

            foreach (BreakLineStyle breakLineStyle in StyleManager.GetStyles<BreakLineStyle>())
            {
                stylesForEditor.Add(new BreakLineStyleForEditor(breakLineStyle, StyleManager.GetCurrentStyleGuid(typeof(BreakLineStyle)), null));
            }

            return stylesForEditor;
        }

        public static XElement ConvertStyleForEditorToXElement(BreakLineStyleForEditor style)
        {
            XElement styleXel = new XElement("UserStyle");
            styleXel.SetAttributeValue(nameof(style.Name), style.Name);
            styleXel.SetAttributeValue(nameof(style.Description), style.Description);
            styleXel.SetAttributeValue(nameof(style.Guid), style.Guid);
            // Properties
            // Цифровые и текстовые значения сохранять через словарь
            var properties = new Dictionary<string, object>
            {
                {nameof(style.BreakHeight), style.BreakHeight},
                {nameof(style.BreakWidth), style.BreakWidth },
                {nameof(style.Overhang), style.Overhang },
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
