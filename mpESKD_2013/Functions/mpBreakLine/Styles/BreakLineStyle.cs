using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using mpESKD.Base.Helpers;
using mpESKD.Base.Properties;
using mpESKD.Base.Styles;
using mpESKD.Functions.mpBreakLine.Properties;
using ModPlusAPI;
using ModPlusAPI.Windows;

namespace mpESKD.Functions.mpBreakLine.Styles
{
    public class BreakLineStyle : IMPCOStyle
    {
        public BreakLineStyle()
        {
            Properties = new List<MPCOBaseProperty>();
        }
        // Global
        public string Name { get; set; }
        public string FunctionName { get; set; }
        public string Description { get; set; }
        public string Guid { get; set; }
        public MPCOStyleType StyleType { get; set; }
        public XElement LayerXmlData { get; set; }
        // Properties
        public List<MPCOBaseProperty> Properties { get; set; }
    }

    public class BreakLineStyleForEditor : MPCOStyleForEditor
    {
        public BreakLineStyleForEditor(IMPCOStyle style, string currentStyleGuid, StyleToBind parent) : base(style, currentStyleGuid, parent)
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
    }

    public static class BreakLineStylesManager
    {
        private const string StylesFileName = "BreakLineStyles.xml";

        private static string _currentStyleGuid;
        
        /// <summary>Guid текущего стиля</summary>
        public static string CurrentStyleGuid
        {
            get
            {
                if (string.IsNullOrEmpty(_currentStyleGuid))
                {
                    var savedStyleGuid = UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpBreakLine", "CurrentStyleGuid");
                    if (!string.IsNullOrEmpty(savedStyleGuid))
                        return savedStyleGuid;
                    const string firstSystemGuid = "00000000-0000-0000-0000-000000000000";
                    UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpBreakLine", "CurrentStyleGuid", firstSystemGuid, true);
                    return firstSystemGuid;
                }
                return _currentStyleGuid;
            }
            set
            {
                _currentStyleGuid = value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpBreakLine", "CurrentStyleGuid", value, true);
            }
        }

        /// <summary>Коллекция стилей</summary>
        public static List<BreakLineStyle> Styles = new List<BreakLineStyle>();
        
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
        public static BreakLineStyle GetCurrentStyle()
        {
            try
            {
                LoadStylesFromXmlFile();

                foreach (BreakLineStyle breakLineStyle in Styles)
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
                BreakLineStyle style = new BreakLineStyle
                {
                    StyleType = MPCOStyleType.User,
                    FunctionName = BreakLineFunction.MPCOEntName
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
                // get layer
                var layerData = styleXel.Element("LayerTableRecord");
                style.LayerXmlData = layerData ?? null;
                // add style
                Styles.Add(style);
            }
        }

        public static void SaveStylesToXml(List<BreakLineStyleForEditor> styles)
        {
            var stylesFile = Path.Combine(MainFunction.StylesPath, StylesFileName);
            // Если файла нет, то создаем
            if (!File.Exists(stylesFile))
                new XElement("Styles").Save(stylesFile);
            try
            {
                var fXel = XElement.Load(stylesFile);
                fXel.RemoveAll();
                foreach (BreakLineStyleForEditor style in styles)
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
        public static List<BreakLineStyle> CreateSystemStyles()
        {
            var styles = new List<BreakLineStyle>();
            var style = new BreakLineStyle
            {
                Name = Language.GetItem(MainFunction.LangItem, "h48") , // "Линия обрыва"
                FunctionName = BreakLineFunction.MPCOEntName,
                Description = Language.GetItem(MainFunction.LangItem, "h53"), // "Базовый стиль для линии обрыва"
                Guid = "00000000-0000-0000-0000-000000000000",
                StyleType = MPCOStyleType.System
            };
            style.Properties.Add(StyleHelpers.CreateProperty(BreakLineProperties.Overhang.DefaultValue, BreakLineProperties.Overhang));
            style.Properties.Add(StyleHelpers.CreateProperty(BreakLineProperties.BreakWidth.DefaultValue, BreakLineProperties.BreakWidth));
            style.Properties.Add(StyleHelpers.CreateProperty(BreakLineProperties.BreakHeight.DefaultValue, BreakLineProperties.BreakHeight));
            style.Properties.Add(StyleHelpers.CreateProperty(BreakLineProperties.LineTypeScale.DefaultValue, BreakLineProperties.LineTypeScale));
            style.Properties.Add(StyleHelpers.CreateProperty(BreakLineProperties.LayerName.DefaultValue, BreakLineProperties.LayerName));
            style.Properties.Add(StyleHelpers.CreateProperty<BreakLineType>(BreakLineProperties.BreakLineType.DefaultValue, BreakLineProperties.BreakLineType));
            style.Properties.Add(StyleHelpers.CreateProperty<AnnotationScale>(BreakLineProperties.Scale.DefaultValue, BreakLineProperties.Scale));

            styles.Add(style);

            return styles;
        }

        /// <summary>Получение стилей в виде классов-презенторов для редактора</summary>
        /// <returns></returns>
        public static List<BreakLineStyleForEditor> GetStylesForEditor()
        {
            var stylesForEditor = new List<BreakLineStyleForEditor>();

            LoadStylesFromXmlFile();

            foreach (BreakLineStyle breakLineStyle in Styles)
            {
                stylesForEditor.Add(new BreakLineStyleForEditor(breakLineStyle, CurrentStyleGuid, null));
            }

            return stylesForEditor;
        }
    }
}
