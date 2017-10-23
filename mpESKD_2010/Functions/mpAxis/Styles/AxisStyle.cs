using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using mpESKD.Base.Helpers;
using mpESKD.Base.Properties;
using mpESKD.Base.Styles;
using mpESKD.Functions.mpAxis.Properties;
using ModPlusAPI;
using ModPlusAPI.Windows;

namespace mpESKD.Functions.mpAxis.Styles
{
    public class AxisStyle : IMPCOStyle
    {
        public AxisStyle()
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

    public class AxisStyleForEditor : MPCOStyleForEditor
    {
        public AxisStyleForEditor(IMPCOStyle style, string currentStyleGuid, StyleToBind parent) : base(style, currentStyleGuid, parent)
        {
            // Properties
            Fracture = StyleHelpers.GetPropertyValue(style, nameof(Fracture),
                AxisProperties.FracturePropertyDescriptive.DefaultValue);
            MarkersPosition = StyleHelpers.GetPropertyValue(style, nameof(MarkersPosition),
                AxisProperties.MarkersPositionPropertyDescriptive.DefaultValue);
            MarkerDiameter = StyleHelpers.GetPropertyValue(style, nameof(MarkerDiameter),
                AxisProperties.MarkerDiameterPropertyDescriptive.DefaultValue);
            LineTypeScale = StyleHelpers.GetPropertyValue(style, nameof(LineTypeScale),
                AxisProperties.LineTypeScalePropertyDescriptive.DefaultValue);
            LayerName = StyleHelpers.GetPropertyValue(style, nameof(LayerName),
                AxisProperties.LayerName.DefaultValue);
            Scale = StyleHelpers.GetPropertyValue<AnnotationScale>(style, nameof(Scale),
                AxisProperties.ScalePropertyDescriptive.DefaultValue);
            LayerXmlData = style.LayerXmlData;
        }

        public AxisStyleForEditor(StyleToBind parent) : base(parent)
        {
            // Properties
            Fracture = AxisProperties.FracturePropertyDescriptive.DefaultValue;
            MarkersPosition = AxisProperties.MarkersPositionPropertyDescriptive.DefaultValue;
            MarkerDiameter = AxisProperties.MarkerDiameterPropertyDescriptive.DefaultValue;
            LineTypeScale = AxisProperties.LineTypeScalePropertyDescriptive.DefaultValue;
            LayerName = AxisProperties.LayerName.DefaultValue;
            Scale = AxisProperties.ScalePropertyDescriptive.DefaultValue;
        }
        #region Properties
        // Позиция маркеров
        public AxisMarkersPosition MarkersPosition { get; set; }
        // Диаметр маркеров
        public int MarkerDiameter { get; set; }
        // Излом
        public int Fracture { get; set; }
        // Стандартные
        public double LineTypeScale { get; set; }
        public string LayerName { get; set; }
        public AnnotationScale Scale { get; set; }
        #endregion
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
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpBreakLine", "CurrentStyleGuid", value, true);
            }
        }
        /// <summary>Коллекция стилей</summary>
        public static List<AxisStyle> Styles = new List<AxisStyle>();
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
        /// В случае отсутсвия файла - создание коллекции с одним системным стилем и ее сохранение в xml-файл</summary>
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
                        int i;
                        switch (nameAttr.Value)
                        {
                            case "MarkersPosition":
                                style.Properties.Add(new MPCOTypeProperty<AxisMarkersPosition>
                                {
                                    Name = nameAttr.Value,
                                    Value = AxisPropertiesHelpers.GetAxisMarkersPositionFromString(propXel.Attribute("Value")?.Value),
                                    Description = AxisProperties.MarkersPositionPropertyDescriptive.Description,
                                    DefaultValue = AxisProperties.MarkersPositionPropertyDescriptive.DefaultValue,
                                    PropertyType = AxisProperties.MarkersPositionPropertyDescriptive.PropertyType,
                                    DisplayName = AxisProperties.MarkersPositionPropertyDescriptive.DisplayName
                                });
                                break;
                            case "MarkerDiameter":
                                style.Properties.Add(new MPCOIntProperty
                                {
                                    Name = nameAttr.Value,
                                    Value = int.TryParse(propXel.Attribute("Value")?.Value, out i) ? i : AxisProperties.MarkerDiameterPropertyDescriptive.DefaultValue,
                                    Description = AxisProperties.MarkerDiameterPropertyDescriptive.Description,
                                    DefaultValue = AxisProperties.MarkerDiameterPropertyDescriptive.DefaultValue,
                                    PropertyType = AxisProperties.MarkerDiameterPropertyDescriptive.PropertyType,
                                    DisplayName = AxisProperties.MarkerDiameterPropertyDescriptive.DisplayName,
                                    Minimum = AxisProperties.MarkerDiameterPropertyDescriptive.Minimum,
                                    Maximum = AxisProperties.MarkerDiameterPropertyDescriptive.Maximum
                                });
                                break;
                            case "Fracture":
                                style.Properties.Add(new MPCOIntProperty
                                {
                                    Name = nameAttr.Value,
                                    Value = int.TryParse(propXel.Attribute("Value")?.Value, out i) ? i : AxisProperties.FracturePropertyDescriptive.DefaultValue,
                                    Description = AxisProperties.FracturePropertyDescriptive.Description,
                                    DefaultValue = AxisProperties.FracturePropertyDescriptive.DefaultValue,
                                    PropertyType = AxisProperties.FracturePropertyDescriptive.PropertyType,
                                    DisplayName = AxisProperties.FracturePropertyDescriptive.DisplayName,
                                    Minimum = AxisProperties.FracturePropertyDescriptive.Minimum,
                                    Maximum = AxisProperties.FracturePropertyDescriptive.Maximum
                                });
                                break;
                            case "LineTypeScale":
                                style.Properties.Add(new MPCODoubleProperty
                                {
                                    Name = nameAttr.Value,
                                    Value = double.TryParse(propXel.Attribute("Value")?.Value, out double d) ? d : AxisProperties.LineTypeScalePropertyDescriptive.DefaultValue,
                                    Description = AxisProperties.LineTypeScalePropertyDescriptive.Description,
                                    DefaultValue = AxisProperties.LineTypeScalePropertyDescriptive.DefaultValue,
                                    PropertyType = AxisProperties.LineTypeScalePropertyDescriptive.PropertyType,
                                    DisplayName = AxisProperties.LineTypeScalePropertyDescriptive.DisplayName,
                                    Minimum = AxisProperties.LineTypeScalePropertyDescriptive.Minimum,
                                    Maximum = AxisProperties.LineTypeScalePropertyDescriptive.Maximum
                                });
                                break;
                            case "LayerName":
                                style.Properties.Add(new MPCOStringProperty
                                {
                                    Name = nameAttr.Value,
                                    Value = propXel.Attribute("Value")?.Value,
                                    Description = AxisProperties.LayerName.Description,
                                    DefaultValue = AxisProperties.LayerName.DefaultValue,
                                    PropertyType = AxisProperties.LayerName.PropertyType,
                                    DisplayName = AxisProperties.LayerName.DisplayName
                                });
                                break;
                            case "Scale":
                                style.Properties.Add(new MPCOTypeProperty<AnnotationScale>
                                {
                                    Name = nameAttr.Value,
                                    Value = Parsers.AnnotationScaleFromString(propXel.Attribute("Value")?.Value),
                                    Description = AxisProperties.ScalePropertyDescriptive.Description,
                                    DefaultValue = AxisProperties.ScalePropertyDescriptive.DefaultValue,
                                    PropertyType = AxisProperties.ScalePropertyDescriptive.PropertyType,
                                    DisplayName = AxisProperties.ScalePropertyDescriptive.DisplayName
                                });
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
                    var propXel = new XElement("Property");
                    propXel.SetAttributeValue("Name", nameof(style.MarkersPosition));
                    propXel.SetAttributeValue("PropertyType", style.MarkersPosition.GetType().Name);
                    propXel.SetAttributeValue("Value", style.MarkersPosition);
                    styleXel.Add(propXel);
                    propXel = new XElement("Property");
                    propXel.SetAttributeValue("Name", nameof(style.MarkerDiameter));
                    propXel.SetAttributeValue("PropertyType", style.MarkerDiameter.GetType().Name);
                    propXel.SetAttributeValue("Value", style.MarkerDiameter);
                    styleXel.Add(propXel);
                    propXel = new XElement("Property");
                    propXel.SetAttributeValue("Name", nameof(style.Fracture));
                    propXel.SetAttributeValue("PropertyType", style.Fracture.GetType().Name);
                    propXel.SetAttributeValue("Value", style.Fracture);
                    styleXel.Add(propXel);
                    propXel = new XElement("Property");
                    propXel.SetAttributeValue("Name", nameof(style.LineTypeScale));
                    propXel.SetAttributeValue("PropertyType", style.LineTypeScale.GetType().Name);
                    propXel.SetAttributeValue("Value", style.LineTypeScale);
                    styleXel.Add(propXel);
                    propXel = new XElement("Property");
                    propXel.SetAttributeValue("Name", nameof(style.Scale));
                    propXel.SetAttributeValue("PropertyType", style.Scale.GetType().Name);
                    propXel.SetAttributeValue("Value", style.Scale.Name);
                    styleXel.Add(propXel);
                    propXel = new XElement("Property");
                    propXel.SetAttributeValue("Name", nameof(style.LayerName));
                    propXel.SetAttributeValue("PropertyType", style.LayerName.GetType().Name);
                    propXel.SetAttributeValue("Value", style.LayerName);
                    styleXel.Add(propXel);
                    // add layer
                    if (LayerHelper.HasLayer(style.LayerName))
                        styleXel.Add(LayerHelper.SetLayerXml(LayerHelper.GetLayerTableRecordByLayerName(style.LayerName)));
                    else if (style.LayerXmlData != null) styleXel.Add(style.LayerXmlData);
                    fXel.Add(styleXel);
                }
                fXel.Save(stylesFile);
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
                Name = "Прямая ось",
                FunctionName = AxisFunction.MPCOEntName,
                Description = "Базовый стиль для прямой оси",
                Guid = "00000000-0000-0000-0000-000000000000",
                StyleType = MPCOStyleType.System
            };
            //style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.OverhangPropertyDescriptive.DefaultValue, AxisProperties.OverhangPropertyDescriptive));
            //style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.BreakWidthPropertyDescriptive.DefaultValue, AxisProperties.BreakWidthPropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.FracturePropertyDescriptive.DefaultValue, AxisProperties.FracturePropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.LineTypeScalePropertyDescriptive.DefaultValue, AxisProperties.LineTypeScalePropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.LayerName.DefaultValue, AxisProperties.LayerName));
            style.Properties.Add(StyleHelpers.CreateProperty<AxisMarkersPosition>(AxisProperties.MarkersPositionPropertyDescriptive.DefaultValue, AxisProperties.MarkersPositionPropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty<AnnotationScale>(AxisProperties.ScalePropertyDescriptive.DefaultValue, AxisProperties.ScalePropertyDescriptive));

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
