using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Autodesk.AutoCAD.DatabaseServices;
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
        // Properties
        public List<MPCOBaseProperty> Properties { get; set; }
    }

    public class BreakLineStyleForEditor : MPCOStyleForEditor
    {
        public BreakLineStyleForEditor(IMPCOStyle style, string currentStyleGuid, StyleToBind parent) : base(style, currentStyleGuid, parent)
        {
            // Properties
            Overhang = StyleHelpers.GetPropertyValue(style, nameof(Overhang),
                mpBreakLineProperties.OverhangPropertyDescriptive.DefaultValue);
            BreakWidth = StyleHelpers.GetPropertyValue(style, nameof(BreakWidth),
                mpBreakLineProperties.BreakWidthPropertyDescriptive.DefaultValue);
            BreakHeight = StyleHelpers.GetPropertyValue(style, nameof(BreakHeight),
                mpBreakLineProperties.BreakHeightPropertyDescriptive.DefaultValue);
            LineTypeScale = StyleHelpers.GetPropertyValue(style, nameof(LineTypeScale),
                mpBreakLineProperties.LineTypeScalePropertyDescriptive.DefaultValue);
            LayerName = StyleHelpers.GetPropertyValue(style, nameof(LayerName),
                mpBreakLineProperties.LayerName.DefaultValue);
            Scale = StyleHelpers.GetPropertyValue<AnnotationScale>(style, nameof(Scale),
                mpBreakLineProperties.ScalePropertyDescriptive.DefaultValue);
        }

        public BreakLineStyleForEditor(StyleToBind parent) : base(parent)
        {
            // Properties
            Overhang = mpBreakLineProperties.OverhangPropertyDescriptive.DefaultValue;
            BreakWidth = mpBreakLineProperties.BreakWidthPropertyDescriptive.DefaultValue;
            BreakHeight = mpBreakLineProperties.BreakHeightPropertyDescriptive.DefaultValue;
            LineTypeScale = mpBreakLineProperties.LineTypeScalePropertyDescriptive.DefaultValue;
            LayerName = mpBreakLineProperties.LayerName.DefaultValue;
            Scale = mpBreakLineProperties.ScalePropertyDescriptive.DefaultValue;
        }

        #region Properties
        public int Overhang { get; set; }
        public int BreakHeight { get; set; }
        public int BreakWidth { get; set; }
        public double LineTypeScale { get; set; }
        public string LayerName { get; set; }
        public AnnotationScale Scale { get; set; }
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
                //if (!Styles.Any())
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
                        int i;
                        double d;
                        switch (nameAttr.Value)
                        {
                            case "Overhang":
                                style.Properties.Add(new MPCOIntProperty
                                {
                                    Name = nameAttr.Value,
                                    Value = int.TryParse(propXel.Attribute("Value")?.Value, out i) ? i : mpBreakLineProperties.OverhangPropertyDescriptive.DefaultValue,
                                    Description = mpBreakLineProperties.OverhangPropertyDescriptive.Description,
                                    DefaultValue = mpBreakLineProperties.OverhangPropertyDescriptive.DefaultValue,
                                    PropertyType = mpBreakLineProperties.OverhangPropertyDescriptive.PropertyType,
                                    DisplayName = mpBreakLineProperties.OverhangPropertyDescriptive.DisplayName,
                                    Minimum = mpBreakLineProperties.OverhangPropertyDescriptive.Minimum,
                                    Maximum = mpBreakLineProperties.OverhangPropertyDescriptive.Maximum
                                });
                                break;
                            case "BreakHeight":
                                style.Properties.Add(new MPCOIntProperty
                                {
                                    Name = nameAttr.Value,
                                    Value = int.TryParse(propXel.Attribute("Value")?.Value, out i) ? i : mpBreakLineProperties.BreakHeightPropertyDescriptive.DefaultValue,
                                    Description = mpBreakLineProperties.BreakHeightPropertyDescriptive.Description,
                                    DefaultValue = mpBreakLineProperties.BreakHeightPropertyDescriptive.DefaultValue,
                                    PropertyType = mpBreakLineProperties.BreakHeightPropertyDescriptive.PropertyType,
                                    DisplayName = mpBreakLineProperties.BreakHeightPropertyDescriptive.DisplayName,
                                    Minimum = mpBreakLineProperties.BreakHeightPropertyDescriptive.Minimum,
                                    Maximum = mpBreakLineProperties.BreakHeightPropertyDescriptive.Maximum
                                });
                                break;
                            case "BreakWidth":
                                style.Properties.Add(new MPCOIntProperty
                                {
                                    Name = nameAttr.Value,
                                    Value = int.TryParse(propXel.Attribute("Value")?.Value, out i) ? i : mpBreakLineProperties.BreakWidthPropertyDescriptive.DefaultValue,
                                    Description = mpBreakLineProperties.BreakWidthPropertyDescriptive.Description,
                                    DefaultValue = mpBreakLineProperties.BreakWidthPropertyDescriptive.DefaultValue,
                                    PropertyType = mpBreakLineProperties.BreakWidthPropertyDescriptive.PropertyType,
                                    DisplayName = mpBreakLineProperties.BreakWidthPropertyDescriptive.DisplayName,
                                    Minimum = mpBreakLineProperties.BreakWidthPropertyDescriptive.Minimum,
                                    Maximum = mpBreakLineProperties.BreakWidthPropertyDescriptive.Maximum
                                });
                                break;
                            case "LineTypeScale":
                                style.Properties.Add(new MPCODoubleProperty
                                {
                                    Name = nameAttr.Value,
                                    Value = double.TryParse(propXel.Attribute("Value")?.Value, out d) ? d : mpBreakLineProperties.LineTypeScalePropertyDescriptive.DefaultValue,
                                    Description = mpBreakLineProperties.LineTypeScalePropertyDescriptive.Description,
                                    DefaultValue = mpBreakLineProperties.LineTypeScalePropertyDescriptive.DefaultValue,
                                    PropertyType = mpBreakLineProperties.LineTypeScalePropertyDescriptive.PropertyType,
                                    DisplayName = mpBreakLineProperties.LineTypeScalePropertyDescriptive.DisplayName,
                                    Minimum = mpBreakLineProperties.LineTypeScalePropertyDescriptive.Minimum,
                                    Maximum = mpBreakLineProperties.LineTypeScalePropertyDescriptive.Maximum
                                });
                                break;
                            case "LayerName":
                                style.Properties.Add(new MPCOStringProperty
                                {
                                    Name = nameAttr.Value,
                                    Value = propXel.Attribute("Value")?.Value,
                                    Description = mpBreakLineProperties.LayerName.Description,
                                    DefaultValue = mpBreakLineProperties.LayerName.DefaultValue,
                                    PropertyType = mpBreakLineProperties.LayerName.PropertyType,
                                    DisplayName = mpBreakLineProperties.LayerName.DisplayName
                                });
                                break;
                            case "Scale":
                                style.Properties.Add(new MPCOTypeProperty<AnnotationScale>
                                {
                                    Name = nameAttr.Value,
                                    Value = Parsers.AnnotationScaleFromString(propXel.Attribute("Value")?.Value),
                                    Description = mpBreakLineProperties.ScalePropertyDescriptive.Description,
                                    DefaultValue = mpBreakLineProperties.ScalePropertyDescriptive.DefaultValue,
                                    PropertyType = mpBreakLineProperties.ScalePropertyDescriptive.PropertyType,
                                    DisplayName = mpBreakLineProperties.ScalePropertyDescriptive.DisplayName
                                });
                                break;
                        }
                    }
                }
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
                    var propXel = new XElement("Property");
                    propXel.SetAttributeValue("Name", nameof(style.BreakHeight));
                    propXel.SetAttributeValue("PropertyType", style.BreakHeight.GetType().Name);
                    propXel.SetAttributeValue("Value", style.BreakHeight);
                    styleXel.Add(propXel);
                    propXel = new XElement("Property");
                    propXel.SetAttributeValue("Name", nameof(style.BreakWidth));
                    propXel.SetAttributeValue("PropertyType", style.BreakWidth.GetType().Name);
                    propXel.SetAttributeValue("Value", style.BreakWidth);
                    styleXel.Add(propXel);
                    propXel = new XElement("Property");
                    propXel.SetAttributeValue("Name", nameof(style.LineTypeScale));
                    propXel.SetAttributeValue("PropertyType", style.LineTypeScale.GetType().Name);
                    propXel.SetAttributeValue("Value", style.LineTypeScale);
                    styleXel.Add(propXel);
                    propXel = new XElement("Property");
                    propXel.SetAttributeValue("Name", nameof(style.Overhang));
                    propXel.SetAttributeValue("PropertyType", style.Overhang.GetType().Name);
                    propXel.SetAttributeValue("Value", style.Overhang);
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
        public static List<BreakLineStyle> CreateSystemStyles()
        {
            var styles = new List<BreakLineStyle>();
            var style = new BreakLineStyle
            {
                Name = "Линия обрыва",
                FunctionName = BreakLineFunction.MPCOEntName,
                Description = "Базовый стиль для линии обрыва",
                Guid = "00000000-0000-0000-0000-000000000000",
                StyleType = MPCOStyleType.System
            };
            style.Properties.Add(StyleHelpers.CreateProperty(mpBreakLineProperties.OverhangPropertyDescriptive.DefaultValue, mpBreakLineProperties.OverhangPropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(mpBreakLineProperties.BreakWidthPropertyDescriptive.DefaultValue, mpBreakLineProperties.BreakWidthPropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(mpBreakLineProperties.BreakHeightPropertyDescriptive.DefaultValue, mpBreakLineProperties.BreakHeightPropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(mpBreakLineProperties.LineTypeScalePropertyDescriptive.DefaultValue, mpBreakLineProperties.LineTypeScalePropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty(mpBreakLineProperties.LayerName.DefaultValue, mpBreakLineProperties.LayerName));
            style.Properties.Add(StyleHelpers.CreateProperty<BreakLineType>(mpBreakLineProperties.BreakLineTypePropertyDescriptive.DefaultValue, mpBreakLineProperties.BreakLineTypePropertyDescriptive));
            style.Properties.Add(StyleHelpers.CreateProperty<AnnotationScale>(mpBreakLineProperties.ScalePropertyDescriptive.DefaultValue, mpBreakLineProperties.ScalePropertyDescriptive));

            styles.Add(style);

            return styles;
        }
        /// <summary>Получение стилей в виде классов-презенторов для редактора</summary>
        /// <returns></returns>
        public static List<BreakLineStyleForEditor> GetStylesForEditor()
        {
            var stylesForEditor = new List<BreakLineStyleForEditor>();

            //if (!Styles.Any())
                LoadStylesFromXmlFile();

            foreach (BreakLineStyle breakLineStyle in Styles)
            {
                stylesForEditor.Add(new BreakLineStyleForEditor(breakLineStyle, CurrentStyleGuid, null));
            }

            return stylesForEditor;
        }
    }
}
