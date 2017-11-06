﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
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
        public XElement TextStyleXmlData { get; set; }
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
            BottomFractureOffset = StyleHelpers.GetPropertyValue(style, nameof(BottomFractureOffset),
                AxisProperties.BottomFractureOffsetPropertyDescriptive.DefaultValue);
            TopFractureOffset = StyleHelpers.GetPropertyValue(style, nameof(TopFractureOffset),
                AxisProperties.TopFractureOffsetPropertyDescriptive.DefaultValue);
            MarkersPosition = StyleHelpers.GetPropertyValue(style, nameof(MarkersPosition),
                AxisProperties.MarkersPositionPropertyDescriptive.DefaultValue);
            MarkersDiameter = StyleHelpers.GetPropertyValue(style, nameof(MarkersDiameter),
                AxisProperties.MarkersDiameterPropertyDescriptive.DefaultValue);
            MarkersCount = StyleHelpers.GetPropertyValue(style, nameof(MarkersCount),
                AxisProperties.MarkersCountPropertyDescriptive.DefaultValue);
            FirstMarkerType = StyleHelpers.GetPropertyValue(style, nameof(FirstMarkerType),
                AxisProperties.FirstMarkerTypePropertyDescriptive.DefaultValue);
            SecondMarkerType = StyleHelpers.GetPropertyValue(style, nameof(SecondMarkerType),
                AxisProperties.SecondMarkerTypePropertyDescriptive.DefaultValue);
            ThirdMarkerType = StyleHelpers.GetPropertyValue(style, nameof(ThirdMarkerType),
                AxisProperties.ThirdMarkerTypePropertyDescriptive.DefaultValue);
            LineTypeScale = StyleHelpers.GetPropertyValue(style, nameof(LineTypeScale),
                AxisProperties.LineTypeScalePropertyDescriptive.DefaultValue);
            LineType = StyleHelpers.GetPropertyValue(style, nameof(LineType),
                AxisProperties.LineTypePropertyDescriptive.DefaultValue);
            TextStyle = StyleHelpers.GetPropertyValue(style, nameof(TextStyle),
                AxisProperties.TextStylePropertyDescriptive.DefaultValue);
            TextHeight = StyleHelpers.GetPropertyValue(style, nameof(TextHeight),
                AxisProperties.TextHeightPropertyDescriptive.DefaultValue);
            LayerName = StyleHelpers.GetPropertyValue(style, nameof(LayerName),
                AxisProperties.LayerName.DefaultValue);
            Scale = StyleHelpers.GetPropertyValue<AnnotationScale>(style, nameof(Scale),
                AxisProperties.ScalePropertyDescriptive.DefaultValue);
            LayerXmlData = style.LayerXmlData;
            TextStyleXmlData = ((AxisStyle) style).TextStyleXmlData;
        }

        public AxisStyleForEditor(StyleToBind parent) : base(parent)
        {
            // Properties
            Fracture = AxisProperties.FracturePropertyDescriptive.DefaultValue;
            BottomFractureOffset = AxisProperties.BottomFractureOffsetPropertyDescriptive.DefaultValue;
            TopFractureOffset = AxisProperties.TopFractureOffsetPropertyDescriptive.DefaultValue;
            MarkersPosition = AxisProperties.MarkersPositionPropertyDescriptive.DefaultValue;
            MarkersDiameter = AxisProperties.MarkersDiameterPropertyDescriptive.DefaultValue;
            MarkersCount = AxisProperties.MarkersCountPropertyDescriptive.DefaultValue;
            FirstMarkerType = AxisProperties.FirstMarkerTypePropertyDescriptive.DefaultValue;
            SecondMarkerType = AxisProperties.SecondMarkerTypePropertyDescriptive.DefaultValue;
            ThirdMarkerType = AxisProperties.ThirdMarkerTypePropertyDescriptive.DefaultValue;
            TextStyle = AxisProperties.TextStylePropertyDescriptive.DefaultValue;
            TextHeight = AxisProperties.TextHeightPropertyDescriptive.DefaultValue;
            LineTypeScale = AxisProperties.LineTypeScalePropertyDescriptive.DefaultValue;
            LineType = AxisProperties.LineTypePropertyDescriptive.DefaultValue;
            LayerName = AxisProperties.LayerName.DefaultValue;
            Scale = AxisProperties.ScalePropertyDescriptive.DefaultValue;
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
        // Излом
        public int Fracture { get; set; }
        // Отступы излома
        public int BottomFractureOffset { get; set; }
        public int TopFractureOffset { get; set; }
        // Text
        public string TextStyle { get; set; }
        public double TextHeight { get; set; }
        // Стандартные
        public double LineTypeScale { get; set; }
        public string LineType { get; set; }
        public string LayerName { get; set; }
        public AnnotationScale Scale { get; set; }
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
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel,AxisProperties.MarkersPositionPropertyDescriptive,
                                    AxisPropertiesHelpers.GetAxisMarkersPositionFromString(propXel.Attribute("Value")?.Value)));
                                break;
                            case "MarkersDiameter":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel,AxisProperties.MarkersDiameterPropertyDescriptive));
                                break;
                            case "MarkersCount":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.MarkersCountPropertyDescriptive));
                                break;
                            case "FirstMarkerType":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.FirstMarkerTypePropertyDescriptive));
                                break;
                            case "SecondMarkerType":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.SecondMarkerTypePropertyDescriptive));
                                break;
                            case "ThirdMarkerType":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.ThirdMarkerTypePropertyDescriptive));
                                break;
                            case "Fracture":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.FracturePropertyDescriptive));
                                break;
                            case "BottomFractureOffset":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.BottomFractureOffsetPropertyDescriptive));
                                break;
                            case "TopFractureOffset":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.TopFractureOffsetPropertyDescriptive));
                                break;
                            case "LineTypeScale":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.LineTypeScalePropertyDescriptive));
                                break;
                            case "LineType":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.LineTypePropertyDescriptive));
                                break;
                            case "LayerName":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.LayerName));
                                break;
                            case "TextStyle":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.TextStylePropertyDescriptive));
                                break;
                            case "TextHeight":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.TextHeightPropertyDescriptive));
                                break;
                            case "Scale":
                                style.Properties.Add(StyleHelpers.CreatePropertyFromXml(propXel, AxisProperties.ScalePropertyDescriptive,
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
                        {nameof(style.BottomFractureOffset), style.BottomFractureOffset },
                        {nameof(style.TopFractureOffset), style.TopFractureOffset },
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
                    fXel.Add(styleXel);
                    // add text style
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
                Name = "Прямая ось",
                FunctionName = AxisFunction.MPCOEntName,
                Description = "Базовый стиль для прямой оси",
                Guid = "00000000-0000-0000-0000-000000000000",
                StyleType = MPCOStyleType.System
            };

            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.MarkersDiameterPropertyDescriptive.DefaultValue, AxisProperties.MarkersDiameterPropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.BottomFractureOffsetPropertyDescriptive.DefaultValue, AxisProperties.BottomFractureOffsetPropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.TopFractureOffsetPropertyDescriptive.DefaultValue, AxisProperties.TopFractureOffsetPropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.MarkersCountPropertyDescriptive.DefaultValue, AxisProperties.MarkersCountPropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.FirstMarkerTypePropertyDescriptive.DefaultValue, AxisProperties.FirstMarkerTypePropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.SecondMarkerTypePropertyDescriptive.DefaultValue, AxisProperties.SecondMarkerTypePropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.ThirdMarkerTypePropertyDescriptive.DefaultValue, AxisProperties.ThirdMarkerTypePropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.FracturePropertyDescriptive.DefaultValue, AxisProperties.FracturePropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.LineTypeScalePropertyDescriptive.DefaultValue, AxisProperties.LineTypeScalePropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.LineTypePropertyDescriptive.DefaultValue, AxisProperties.LineTypePropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.LayerName.DefaultValue, AxisProperties.LayerName));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.TextStylePropertyDescriptive.DefaultValue, AxisProperties.TextStylePropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(AxisProperties.TextHeightPropertyDescriptive.DefaultValue, AxisProperties.TextHeightPropertyDescriptive));
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