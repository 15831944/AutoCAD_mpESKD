namespace mpESKD.Functions.mpGroundLine.Styles
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Base.Enums;
    using Base.Helpers;
    using Base.Properties;
    using Base.Styles;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using Properties;

    public class GroundLineStyle : MPCOStyle
    {
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
    }

    public class GroundLineStyleManager
    {
        private const string StylesFileName = "GroundLineStyles.xml";

        private static string _currentStyleGuid;

        /// <summary>Guid текущего стиля</summary>
        public static string CurrentStyleGuid
        {
            get
            {
                if (string.IsNullOrEmpty(_currentStyleGuid))
                {
                    var savedStyleGuid = UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpGroundLine", "CurrentStyleGuid");
                    if (!string.IsNullOrEmpty(savedStyleGuid))
                        return savedStyleGuid;
                    const string firstSystemGuid = "00000000-0000-0000-0000-000000000000";
                    UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpGroundLine", "CurrentStyleGuid", firstSystemGuid, true);
                    return firstSystemGuid;
                }
                return _currentStyleGuid;
            }
            set
            {
                _currentStyleGuid = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpGroundLine", "CurrentStyleGuid", value, true);
            }
        }

        /// <summary>Коллекция стилей</summary>
        public static List<GroundLineStyle> Styles = new List<GroundLineStyle>();

        /// <summary>Проверка и создание в случае необходимости файла стилей</summary>
        public static void CheckStylesFile()
        {
            bool needToCreate;
            var stylesFile = Path.Combine(MainFunction.StylesPath, StylesFileName);
            if (File.Exists(stylesFile))
            {
                try
                {
                    XElement.Load(stylesFile);
                    needToCreate = false;
                }
                catch
                {
                    needToCreate = true;
                }
            }
            else needToCreate = true;

            if (needToCreate)
            {
                XElement fXel = new XElement("Styles");
                fXel.Save(stylesFile);
            }
        }

        /// <summary>Получение стиля из коллекции по его идентификатору или первого системного стиля, если не найден
        /// В случае, если коллекция пустая, то происходит ее загрузка (с созданием, если нужно)</summary>
        /// <returns></returns>
        public static GroundLineStyle GetCurrentStyle()
        {
            try
            {
                LoadStylesFromXmlFile();

                foreach (GroundLineStyle breakLineStyle in Styles)
                {
                    if (breakLineStyle.Guid.Equals(CurrentStyleGuid))
                        return breakLineStyle;
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
                GroundLineStyle style = new GroundLineStyle
                {
                    StyleType = MPCOStyleType.User,
                    FunctionName = GroundLineFunction.MPCOEntName
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
                // get layer
                var layerData = styleXel.Element("LayerTableRecord");
                style.LayerXmlData = layerData ?? null;
                // add style
                Styles.Add(style);
            }
        }

        public static void SaveStylesToXml(List<GroundLineStyleForEditor> styles)
        {
            var stylesFile = Path.Combine(MainFunction.StylesPath, StylesFileName);
            // Если файла нет, то создаем
            if (!File.Exists(stylesFile))
                new XElement("Styles").Save(stylesFile);
            try
            {
                var fXel = XElement.Load(stylesFile);
                fXel.RemoveAll();
                foreach (GroundLineStyleForEditor style in styles)
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
        public static List<GroundLineStyle> CreateSystemStyles()
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
            style.Properties.Add(StyleHelpers.CreateProperty<GroundLineFirstStrokeOffset>(
                GroundLineProperties.FirstStrokeOffset.DefaultValue, GroundLineProperties.FirstStrokeOffset));
            style.Properties.Add(StyleHelpers.CreateProperty(GroundLineProperties.StrokeLength.DefaultValue, GroundLineProperties.StrokeLength));
            style.Properties.Add(StyleHelpers.CreateProperty(GroundLineProperties.StrokeAngle.DefaultValue, GroundLineProperties.StrokeAngle));
            style.Properties.Add(StyleHelpers.CreateProperty(GroundLineProperties.StrokeOffset.DefaultValue, GroundLineProperties.StrokeOffset));
            style.Properties.Add(StyleHelpers.CreateProperty(GroundLineProperties.Space.DefaultValue, GroundLineProperties.Space));
            style.Properties.Add(StyleHelpers.CreateProperty(GroundLineProperties.LineType.DefaultValue, GroundLineProperties.LineType));
            style.Properties.Add(StyleHelpers.CreateProperty(GroundLineProperties.LineTypeScale.DefaultValue, GroundLineProperties.LineTypeScale));
            style.Properties.Add(StyleHelpers.CreateProperty(GroundLineProperties.LayerName.DefaultValue, GroundLineProperties.LayerName));
            style.Properties.Add(StyleHelpers.CreateProperty<AnnotationScale>(GroundLineProperties.Scale.DefaultValue, GroundLineProperties.Scale));

            styles.Add(style);

            return styles;
        }

        /// <summary>Получение стилей в виде классов-презенторов для редактора</summary>
        /// <returns></returns>
        public static List<GroundLineStyleForEditor> GetStylesForEditor()
        {
            var stylesForEditor = new List<GroundLineStyleForEditor>();

            LoadStylesFromXmlFile();

            foreach (GroundLineStyle breakLineStyle in Styles)
            {
                stylesForEditor.Add(new GroundLineStyleForEditor(breakLineStyle, CurrentStyleGuid, null));
            }

            return stylesForEditor;
        }
    }
}
