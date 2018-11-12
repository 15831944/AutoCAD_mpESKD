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
        public XElement TextStyleXmlData { get; set; }

        public override List<T> CreateSystemStyles<T>()
        {
            //todo release
            throw new NotImplementedException();
        }
    }

    public class AxisStyleForEditor : MPCOStyleForEditor
    {
        public AxisStyleForEditor(MPCOStyle style, string currentStyleGuid, StyleToBind parent) : base(style, currentStyleGuid, parent)
        {
            // Properties
            Fracture = StyleHelpers.GetPropertyValue(style, nameof(Fracture),
                AxisProperties.Fracture.DefaultValue);
            BottomFractureOffset = StyleHelpers.GetPropertyValue(style, nameof(BottomFractureOffset),
                AxisProperties.BottomFractureOffset.DefaultValue);
            TopFractureOffset = StyleHelpers.GetPropertyValue(style, nameof(TopFractureOffset),
                AxisProperties.TopFractureOffset.DefaultValue);
            MarkersPosition = StyleHelpers.GetPropertyValue(style, nameof(MarkersPosition),
                AxisProperties.MarkersPosition.DefaultValue);
            MarkersDiameter = StyleHelpers.GetPropertyValue(style, nameof(MarkersDiameter),
                AxisProperties.MarkersDiameter.DefaultValue);
            MarkersCount = StyleHelpers.GetPropertyValue(style, nameof(MarkersCount),
                AxisProperties.MarkersCount.DefaultValue);
            FirstMarkerType = StyleHelpers.GetPropertyValue(style, nameof(FirstMarkerType),
                AxisProperties.FirstMarkerType.DefaultValue);
            SecondMarkerType = StyleHelpers.GetPropertyValue(style, nameof(SecondMarkerType),
                AxisProperties.SecondMarkerType.DefaultValue);
            ThirdMarkerType = StyleHelpers.GetPropertyValue(style, nameof(ThirdMarkerType),
                AxisProperties.ThirdMarkerType.DefaultValue);
            ArrowsSize = StyleHelpers.GetPropertyValue(style, nameof(ArrowsSize),
                AxisProperties.ArrowsSize.DefaultValue);
            LineTypeScale = StyleHelpers.GetPropertyValue(style, nameof(LineTypeScale),
                AxisProperties.LineTypeScale.DefaultValue);
            LineType = StyleHelpers.GetPropertyValue(style, nameof(LineType),
                AxisProperties.LineType.DefaultValue);
            TextStyle = StyleHelpers.GetPropertyValue(style, nameof(TextStyle),
                AxisProperties.TextStyle.DefaultValue);
            TextHeight = StyleHelpers.GetPropertyValue(style, nameof(TextHeight),
                AxisProperties.TextHeight.DefaultValue);
            LayerName = StyleHelpers.GetPropertyValue(style, nameof(LayerName),
                AxisProperties.LayerName.DefaultValue);
            Scale = StyleHelpers.GetPropertyValue<AnnotationScale>(style, nameof(Scale),
                AxisProperties.Scale.DefaultValue);
            LayerXmlData = style.LayerXmlData;
            TextStyleXmlData = ((AxisStyle)style).TextStyleXmlData;
        }

        public AxisStyleForEditor(StyleToBind parent) : base(parent)
        {
            // Properties
            Fracture = AxisProperties.Fracture.DefaultValue;
            BottomFractureOffset = AxisProperties.BottomFractureOffset.DefaultValue;
            TopFractureOffset = AxisProperties.TopFractureOffset.DefaultValue;
            MarkersPosition = AxisProperties.MarkersPosition.DefaultValue;
            MarkersDiameter = AxisProperties.MarkersDiameter.DefaultValue;
            MarkersCount = AxisProperties.MarkersCount.DefaultValue;
            ArrowsSize = AxisProperties.ArrowsSize.DefaultValue;
            FirstMarkerType = AxisProperties.FirstMarkerType.DefaultValue;
            SecondMarkerType = AxisProperties.SecondMarkerType.DefaultValue;
            ThirdMarkerType = AxisProperties.ThirdMarkerType.DefaultValue;
            TextStyle = AxisProperties.TextStyle.DefaultValue;
            TextHeight = AxisProperties.TextHeight.DefaultValue;
            LineTypeScale = AxisProperties.LineTypeScale.DefaultValue;
            LineType = AxisProperties.LineType.DefaultValue;
            LayerName = AxisProperties.LayerName.DefaultValue;
            Scale = AxisProperties.Scale.DefaultValue;
        }

        #region Properties
        
        // Позиция маркеров
        public AxisMarkersPosition MarkersPosition { get; set; }
        
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
        
        public XElement TextStyleXmlData { get; set; }
    }
    
    public class AxisStyleManager
    {
        private const string StylesFileName = "AxisStyles.xml";
        private static string _currentStyleGuid;
        /// <summary>Guid текущего стиля</summary>
        public static string CurrentStyleGuid
        {
            get
            {
                if (string.IsNullOrEmpty(_currentStyleGuid))
                {
                    var savedStyleGuid = UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpAxis", "CurrentStyleGuid");
                    if (!string.IsNullOrEmpty(savedStyleGuid))
                        return savedStyleGuid;
                    const string firstSystemGuid = "00000000-0000-0000-0000-000000000000";
                    UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpAxis", "CurrentStyleGuid", firstSystemGuid, true);
                    return firstSystemGuid;
                }
                return _currentStyleGuid;
            }
            set
            {
                _currentStyleGuid = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpAxis", "CurrentStyleGuid", value, true);
            }
        }
        
        /// <summary>Коллекция стилей</summary>
        public static List<AxisStyle> Styles = new List<AxisStyle>();
        
        /// <summary>Получение стиля из коллекции по его идентификатору или первого системного стиля, если не найден
        /// В случае, если коллекция пустая, то происходит ее загрузка (с созданием, если нужно)</summary>
        /// <returns></returns>
        public static AxisStyle GetCurrentStyle()
        {
            try
            {
                LoadStylesFromXmlFile();

                foreach (AxisStyle axisStyle in Styles)
                {
                    if (axisStyle.Guid.Equals(CurrentStyleGuid))
                        return axisStyle;
                }
                return Styles.First(s => s.StyleType == MPCOStyleType.System);
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
                return CreateSystemStyles().FirstOrDefault();
            }
        }
        
        /// <summary>Загрузка (десериализация) стилей из xml-файла
        /// В случае отсутствия файла - создание коллекции с одним системным стилем и ее сохранение в xml-файл</summary>
        private static void LoadStylesFromXmlFile()
        {
            Styles.Clear();
            // Добавляю системные стили
            Styles.AddRange(CreateSystemStyles());
            // load from file
            var stylesFile = Path.Combine(MainFunction.StylesPath, StylesFileName);
            var fXel = XElement.Load(stylesFile);
            foreach (XElement styleXel in fXel.Elements("UserStyle"))
            {
                AxisStyle style = new AxisStyle
                {
                    StyleType = MPCOStyleType.User,
                    FunctionName = AxisFunction.MPCOEntName
                };
                style.Name = styleXel.Attribute(nameof(style.Name))?.Value;
                style.Description = styleXel.Attribute(nameof(style.Description))?.Value;
                // Guid беру, если есть атрибут. Иначе создаю новый
                var guidAttr = styleXel.Attribute(nameof(style.Guid));
                style.Guid = guidAttr?.Value ?? Guid.NewGuid().ToString();
                // Properties
                foreach (XElement propXel in styleXel.Elements("Property"))
                {
                    var nameAttr = propXel.Attribute("Name");
                    if (nameAttr != null)
                    {
                        switch (nameAttr.Value)
                        {
                            case "MarkersPosition":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.MarkersPosition,
                                    AxisPropertiesHelpers.GetAxisMarkersPositionFromString(propXel.Attribute("Value")?.Value)));
                                break;
                            case "MarkersDiameter":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.MarkersDiameter));
                                break;
                            case "MarkersCount":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.MarkersCount));
                                break;
                            case "FirstMarkerType":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.FirstMarkerType));
                                break;
                            case "SecondMarkerType":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.SecondMarkerType));
                                break;
                            case "ThirdMarkerType":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.ThirdMarkerType));
                                break;
                            case "OrientMarkerType":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.OrientMarkerType));
                                break;
                            case "Fracture":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.Fracture));
                                break;
                            case "BottomFractureOffset":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.BottomFractureOffset));
                                break;
                            case "TopFractureOffset":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.TopFractureOffset));
                                break;
                            case "ArrowsSize":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.ArrowsSize));
                                break;
                            case "LineTypeScale":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.LineTypeScale));
                                break;
                            case "LineType":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.LineType));
                                break;
                            case "LayerName":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.LayerName));
                                break;
                            case "TextStyle":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.TextStyle));
                                break;
                            case "TextHeight":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.TextHeight));
                                break;
                            case "Scale":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.Scale,
                                    Parsers.AnnotationScaleFromString(propXel.Attribute("Value")?.Value)));
                                break;
                        }
                    }
                }
                // get layer
                var layerData = styleXel.Element("LayerTableRecord");
                style.LayerXmlData = layerData ?? null;
                // get text style
                var textStyleData = styleXel.Element("TextStyleTableRecord");
                style.TextStyleXmlData = textStyleData ?? null;
                // add style
                Styles.Add(style);
            }
        }

        public static void SaveStylesToXml(List<AxisStyleForEditor> styles)
        {
            var stylesFile = Path.Combine(MainFunction.StylesPath, StylesFileName);
            // Если файла нет, то создаем
            if (!File.Exists(stylesFile))
                new XElement("Styles").Save(stylesFile);
            try
            {
                var fXel = XElement.Load(stylesFile);
                fXel.RemoveAll();
                foreach (AxisStyleForEditor style in styles)
                {
                    if (!style.CanEdit) continue;
                    XElement styleXel = new XElement("UserStyle");
                    styleXel.SetAttributeValue(nameof(style.Name), style.Name);
                    styleXel.SetAttributeValue(nameof(style.Description), style.Description);
                    styleXel.SetAttributeValue(nameof(style.Guid), style.Guid);
                    // Properties
                    // Цифровые и текстовые значения сохранять через словарь
                    var properties = new Dictionary<string, object>
                    {
                        {nameof(style.MarkersPosition), style.MarkersPosition},
                        {nameof(style.MarkersDiameter), style.MarkersDiameter },
                        {nameof(style.MarkersCount), style.MarkersCount },
                        {nameof(style.FirstMarkerType), style.FirstMarkerType },
                        {nameof(style.SecondMarkerType), style.SecondMarkerType },
                        {nameof(style.ThirdMarkerType), style.ThirdMarkerType },
                        {nameof(style.OrientMarkerType), style.OrientMarkerType },
                        {nameof(style.BottomFractureOffset), style.BottomFractureOffset },
                        {nameof(style.TopFractureOffset), style.TopFractureOffset },
                        {nameof(style.ArrowsSize), style.ArrowsSize },
                        {nameof(style.Fracture), style.Fracture },
                        {nameof(style.TextStyle), style.TextStyle },
                        {nameof(style.TextHeight), style.TextHeight },
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
                    // add text style
                    if(TextStyleHelper.HasTextStyle(style.TextStyle))
                        styleXel.Add(TextStyleHelper.SetTextStyleTableRecordXElement(TextStyleHelper.GetTextStyleTableRecordByName(style.TextStyle)));
                    else if (style.TextStyleXmlData != null) styleXel.Add(style.TextStyleXmlData);

                    fXel.Add(styleXel);
                }
                fXel.Save(stylesFile);
                // reload styles
                LoadStylesFromXmlFile();
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        /// <summary>Создание системных (стандартных) стилей. Их может быть несколько</summary>
        /// <returns></returns>
        public static List<AxisStyle> CreateSystemStyles()
        {
            var styles = new List<AxisStyle>();
            var style = new AxisStyle
            {
                Name = Language.GetItem(MainFunction.LangItem, "h41"), // "Прямая ось",
                FunctionName = AxisFunction.MPCOEntName,
                Description = Language.GetItem(MainFunction.LangItem, "h68"), // "Базовый стиль для прямой оси",
                Guid = "00000000-0000-0000-0000-000000000000",
                StyleType = MPCOStyleType.System
            };
            
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.MarkersDiameter.DefaultValue, AxisProperties.MarkersDiameter));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.BottomFractureOffset.DefaultValue, AxisProperties.BottomFractureOffset));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.TopFractureOffset.DefaultValue, AxisProperties.TopFractureOffset));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.MarkersCount.DefaultValue, AxisProperties.MarkersCount));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.FirstMarkerType.DefaultValue, AxisProperties.FirstMarkerType));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.SecondMarkerType.DefaultValue, AxisProperties.SecondMarkerType));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.ThirdMarkerType.DefaultValue, AxisProperties.ThirdMarkerType));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.OrientMarkerType.DefaultValue, AxisProperties.OrientMarkerType));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.Fracture.DefaultValue, AxisProperties.Fracture));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.ArrowsSize.DefaultValue, AxisProperties.ArrowsSize));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.LineTypeScale.DefaultValue, AxisProperties.LineTypeScale));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.LineType.DefaultValue, AxisProperties.LineType));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.LayerName.DefaultValue, AxisProperties.LayerName));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.TextStyle.DefaultValue, AxisProperties.TextStyle));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.TextHeight.DefaultValue, AxisProperties.TextHeight));
            style.Properties.Add(StyleHelpers.CreateProperty<AxisMarkersPosition>(AxisProperties.MarkersPosition.DefaultValue, AxisProperties.MarkersPosition));
            style.Properties.Add(StyleHelpers.CreateProperty<AnnotationScale>(AxisProperties.Scale.DefaultValue, AxisProperties.Scale));

            styles.Add(style);

            return styles;
        }
        /// <summary>Получение стилей в виде классов-презенторов для редактора</summary>
        /// <returns></returns>
        public static List<AxisStyleForEditor> GetStylesForEditor()
        {
            var stylesForEditor = new List<AxisStyleForEditor>();

            LoadStylesFromXmlFile();

            foreach (AxisStyle axisStyle in Styles)
            {
                stylesForEditor.Add(new AxisStyleForEditor(axisStyle, CurrentStyleGuid, null));
            }

            return stylesForEditor;
        }
    }
}
