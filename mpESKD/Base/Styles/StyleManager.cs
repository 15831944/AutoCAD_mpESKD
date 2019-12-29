namespace mpESKD.Base.Styles
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using Attributes;
    using Autodesk.AutoCAD.DatabaseServices;
    using Enums;
    using Helpers;
    using JetBrains.Annotations;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using Properties;

    public static class StyleManager
    {
        static StyleManager()
        {
            if (EntityStyles == null)
            {
                EntityStyles = new List<IntellectualEntityStyle>();
                CheckStyleFiles();
                CreateSystemStyles();
            }
        }

        /// <summary>
        /// Статическая коллекция стилей интеллектуальных примитивов
        /// </summary>
        private static readonly List<IntellectualEntityStyle> EntityStyles;

        /// <summary>
        /// Проверка и создание в случае отсутствия файлов хранения пользовательских стилей для всех типов примитивов
        /// </summary>
        private static void CheckStyleFiles()
        {
            var entityTypes = TypeFactory.Instance.GetEntityTypes();
            entityTypes.ForEach(et =>
            {
                bool needToCreate;
                var stylesFile = Path.Combine(MainFunction.StylesPath, et.Name + "Styles.xml");
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
                else
                {
                    needToCreate = true;
                }

                if (needToCreate)
                {
                    XElement fXel = new XElement("Styles");
                    fXel.Save(stylesFile);
                }
            });
        }

        /// <summary>
        /// Создание стандартных системных стилей
        /// </summary>
        private static void CreateSystemStyles()
        {
            var entityTypes = TypeFactory.Instance.GetEntityTypes();
            entityTypes.ForEach(et =>
            {
                var systemStyle = new IntellectualEntityStyle(et)
                {
                    Name = LocalizationHelper.GetEntityLocalizationName(et) + " [" + Language.GetItem(Invariables.LangItem, "h12") + "]",
                    Description = TypeFactory.Instance.GetSystemStyleLocalizedDescription(et),
                    Guid = "00000000-0000-0000-0000-000000000000",
                    StyleType = StyleType.System
                };
                if (!HasStyle(systemStyle))
                {
                    FillStyleDefaultProperties(systemStyle, et);

                    EntityStyles.Add(systemStyle);
                }
            });
        }

        /// <summary>
        /// Наполнение стиля свойствами со значениями по умолчанию
        /// </summary>
        public static void FillStyleDefaultProperties(this IntellectualEntityStyle style, Type entityType)
        {
            foreach (PropertyInfo propertyInfo in entityType.GetProperties())
            {
                var attribute = propertyInfo.GetCustomAttribute<EntityPropertyAttribute>();
                var keyForEditorAttribute = propertyInfo.GetCustomAttribute<PropertyNameKeyInStyleEditor>();
                if (attribute != null && attribute.Name != "Style")
                {
                    if (attribute.PropertyScope != PropertyScope.PaletteAndStyleEditor)
                    {
                        continue;
                    }

                    if (attribute.Name == "Scale")
                    {
                        style.Properties.Add(new IntellectualEntityProperty(
                            attribute, keyForEditorAttribute, entityType,
                            Parsers.AnnotationScaleFromString(attribute.DefaultValue.ToString()),
                            ObjectId.Null));
                    }
                    else if (attribute.Name == "LayerName")
                    {
                        style.Properties.Add(new IntellectualEntityProperty(
                            attribute, keyForEditorAttribute, entityType,
                            AcadHelpers.Layers.Contains(attribute.DefaultValue.ToString())
                                ? attribute.DefaultValue
                                : Language.GetItem(Invariables.LangItem, "defl"),
                            ObjectId.Null));
                    }
                    else
                    {
                        style.Properties.Add(new IntellectualEntityProperty(
                            attribute,
                            keyForEditorAttribute,
                            entityType,
                            attribute.DefaultValue,
                            ObjectId.Null));
                    }
                }
            }
        }

        /// <summary>
        /// Чтение свойств из примитива и запись их в стиль примитива
        /// </summary>
        /// <param name="style">Стиль примитива</param>
        /// <param name="entity">Интеллектуальный примитив</param>
        /// <param name="blockReference">Вставка блока, представляющая интеллектуальный примитив в AutoCAD</param>
        public static void GetPropertiesFromEntity(this IntellectualEntityStyle style, IntellectualEntity entity, BlockReference blockReference)
        {
            Type entityType = entity.GetType();
            foreach (PropertyInfo propertyInfo in entityType.GetProperties())
            {
                var attribute = propertyInfo.GetCustomAttribute<EntityPropertyAttribute>();
                var keyForEditorAttribute = propertyInfo.GetCustomAttribute<PropertyNameKeyInStyleEditor>();
                if (attribute != null && attribute.Name != "Style")
                {
                    if (attribute.PropertyScope != PropertyScope.PaletteAndStyleEditor)
                    {
                        continue;
                    }

                    if (attribute.Name == "LayerName")
                    {
                        style.Properties.Add(new IntellectualEntityProperty(
                            attribute, keyForEditorAttribute, entityType,
                            blockReference.Layer,
                            ObjectId.Null));
                    }
                    else if (attribute.Name == "LineType")
                    {
                        style.Properties.Add(new IntellectualEntityProperty(
                            attribute,
                            keyForEditorAttribute,
                            entityType,
                            blockReference.Linetype,
                            ObjectId.Null));
                    }
                    else
                    {
                        style.Properties.Add(new IntellectualEntityProperty(
                            attribute,
                            keyForEditorAttribute,
                            entityType,
                            propertyInfo.GetValue(entity),
                            ObjectId.Null));
                    }
                }
            }
        }

        private static bool HasStyle(IntellectualEntityStyle style)
        {
            return EntityStyles.Any(s => s.EntityType == style.EntityType &&
                                         s.StyleType == style.StyleType &&
                                         s.Guid == style.Guid);
        }

        /// <summary>
        /// Загрузка пользовательских стилей из xml-файла для указанного типа примитива
        /// </summary>
        /// <param name="entityType">Тип примитива</param>
        private static void LoadStylesFromXmlFile(Type entityType)
        {
            var stylesFile = Path.Combine(MainFunction.StylesPath, entityType.Name + "Styles.xml");
            if (File.Exists(stylesFile))
            {
                for (var i = EntityStyles.Count - 1; i >= 0; i--)
                {
                    var style = EntityStyles[i];
                    if (style.StyleType == StyleType.System)
                    {
                        continue;
                    }

                    if (style.EntityType != entityType)
                    {
                        continue;
                    }

                    EntityStyles.RemoveAt(i);
                }

                var fXel = XElement.Load(stylesFile);
                foreach (XElement styleXel in fXel.Elements("UserStyle"))
                {
                    var style = new IntellectualEntityStyle(entityType);
                    style.StyleType = StyleType.User;
                    style.Name = styleXel.Attribute(nameof(style.Name))?.Value;
                    style.Description = styleXel.Attribute(nameof(style.Description))?.Value;

                    // Guid беру, если есть атрибут. Иначе создаю новый
                    var guidAttr = styleXel.Attribute(nameof(style.Guid));
                    style.Guid = guidAttr?.Value ?? Guid.NewGuid().ToString();

                    if (HasStyle(style))
                    {
                        continue;
                    }

                    // get layer xml data
                    var layerData = styleXel.Element("LayerTableRecord");
                    style.LayerXmlData = layerData ?? null;

                    // get text style xml data
                    var textStyleData = styleXel.Element("TextStyleTableRecord");
                    style.TextStyleXmlData = textStyleData ?? null;

                    // get properties from file
                    foreach (PropertyInfo propertyInfo in entityType.GetProperties())
                    {
                        var attribute = propertyInfo.GetCustomAttribute<EntityPropertyAttribute>();
                        var keyForEditorAttribute = propertyInfo.GetCustomAttribute<PropertyNameKeyInStyleEditor>();
                        if (attribute != null)
                        {
                            XElement xmlProperty = styleXel.Elements("Property").FirstOrDefault(e => e.Attribute("Name")?.Value == attribute.Name);
                            if (xmlProperty != null)
                            {
                                var xmlValue = xmlProperty.Attribute("Value")?.Value;
                                var valueType = attribute.DefaultValue.GetType();
                                if (attribute.Name == "Scale")
                                {
                                    style.Properties.Add(new IntellectualEntityProperty(
                                        attribute, keyForEditorAttribute, entityType,
                                        Parsers.AnnotationScaleFromString(xmlValue),
                                        ObjectId.Null));
                                }
                                else
                                {
                                    if (valueType == typeof(string))
                                    {
                                        style.Properties.Add(new IntellectualEntityProperty(
                                            attribute, keyForEditorAttribute, entityType, xmlProperty.Attribute("Value").Value, ObjectId.Null));
                                    }
                                    else if (valueType == typeof(int))
                                    {
                                        style.Properties.Add(new IntellectualEntityProperty(
                                            attribute, keyForEditorAttribute, entityType,
                                            int.TryParse(xmlValue, out var i) ? i : attribute.DefaultValue,
                                            ObjectId.Null));
                                    }
                                    else if (valueType == typeof(double))
                                    {
                                        style.Properties.Add(new IntellectualEntityProperty(
                                            attribute, keyForEditorAttribute, entityType,
                                            double.TryParse(xmlValue, out var d) ? d : attribute.DefaultValue,
                                            ObjectId.Null));
                                    }
                                    else if (valueType == typeof(bool))
                                    {
                                        style.Properties.Add(new IntellectualEntityProperty(
                                            attribute, keyForEditorAttribute, entityType,
                                            bool.TryParse(xmlValue, out var b) ? b : attribute.DefaultValue,
                                            ObjectId.Null));
                                    }
                                    else if (valueType.IsEnum)
                                    {
                                        try
                                        {
                                            style.Properties.Add(new IntellectualEntityProperty(
                                                attribute, keyForEditorAttribute, entityType,
                                                Enum.Parse(attribute.DefaultValue.GetType(), xmlValue),
                                                ObjectId.Null));
                                        }
                                        catch
                                        {
                                            style.Properties.Add(new IntellectualEntityProperty(attribute, keyForEditorAttribute, entityType, attribute.DefaultValue, ObjectId.Null));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    style.CheckMissedProperties(entityType);

                    // add style
                    EntityStyles.Add(style);
                }
            }
        }

        /// <summary>
        /// Проверка стиля на наличие отсутствующих свойств. Свойства могут отсутствовать в случае,
        /// если стиль уже был сохранен, но позже вышло обновление с добавлением нового свойства
        /// </summary>
        /// <param name="style">Проверяемый стиль</param>
        /// <param name="entityType">Тип примитива</param>
        private static void CheckMissedProperties(this IntellectualEntityStyle style, Type entityType)
        {
            foreach (PropertyInfo propertyInfo in entityType.GetProperties())
            {
                var attribute = propertyInfo.GetCustomAttribute<EntityPropertyAttribute>();
                var keyForEditorAttribute = propertyInfo.GetCustomAttribute<PropertyNameKeyInStyleEditor>();
                if (attribute != null && attribute.Name != "Style")
                {
                    if (attribute.PropertyScope != PropertyScope.PaletteAndStyleEditor)
                    {
                        continue;
                    }

                    // ReSharper disable once SimplifyLinqExpression
                    if (!style.Properties.Any(p => p.Name == attribute.Name))
                    {
                        style.Properties.Add(new IntellectualEntityProperty(
                        attribute,
                        keyForEditorAttribute,
                        entityType,
                        attribute.DefaultValue,
                        ObjectId.Null));
                    }
                }
            }
        }

        /// <summary>
        /// Сохранение стилей указанного типа
        /// </summary>
        public static void SaveStylesToXml(Type entityType)
        {
            var stylesFile = Path.Combine(MainFunction.StylesPath, entityType.Name + "Styles.xml");

            // Если файла нет, то создаем
            if (!File.Exists(stylesFile))
            {
                new XElement("Styles").Save(stylesFile);
            }

            try
            {
                var fXel = XElement.Load(stylesFile);
                fXel.RemoveAll();

                foreach (IntellectualEntityStyle style in EntityStyles.Where(s => s.EntityType == entityType))
                {
                    if (!style.CanEdit)
                    {
                        continue;
                    }

                    fXel.Add(style.ConvertToXElement());
                }

                fXel.Save(stylesFile);
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        /// <summary>
        /// Конвертировать стиль в XElement
        /// </summary>
        private static XElement ConvertToXElement(this IntellectualEntityStyle style)
        {
            XElement styleXel = new XElement("UserStyle");
            styleXel.SetAttributeValue(nameof(style.Name), style.Name);
            styleXel.SetAttributeValue(nameof(style.Description), style.Description);
            styleXel.SetAttributeValue(nameof(style.Guid), style.Guid);

            foreach (IntellectualEntityProperty entityProperty in style.Properties)
            {
                if (entityProperty.Name == "Scale")
                {
                    var propXel = new XElement("Property");
                    propXel.SetAttributeValue("Name", entityProperty.Name);
                    propXel.SetAttributeValue("Value", ((AnnotationScale)entityProperty.Value).Name);
                    styleXel.Add(propXel);
                }
                else
                {
                    var propXel = new XElement("Property");
                    propXel.SetAttributeValue("Name", entityProperty.Name);
                    propXel.SetAttributeValue("Value", entityProperty.Value);
                    styleXel.Add(propXel);
                }
            }

            if (LayerHelper.HasLayer(style.GetLayerNameProperty()))
            {
                styleXel.Add(LayerHelper.SetLayerXml(LayerHelper.GetLayerTableRecordByLayerName(style.GetLayerNameProperty())));
            }
            else if (style.LayerXmlData != null)
            {
                styleXel.Add(style.LayerXmlData);
            }

            // add text style
            if (TextStyleHelper.HasTextStyle(style.GetTextStyleProperty()))
            {
                styleXel.Add(TextStyleHelper.SetTextStyleTableRecordXElement(TextStyleHelper.GetTextStyleTableRecordByName(style.GetTextStyleProperty())));
            }
            else if (style.TextStyleXmlData != null)
            {
                styleXel.Add(style.TextStyleXmlData);
            }

            return styleXel;
        }

        public static string GetCurrentStyleGuidForEntity(Type entityType)
        {
            return CurrentStyleGuid("mp" + entityType.Name);
        }

        /// <summary>
        /// Получение идентификатора текущего стиля, сохраненного в настройках или идентификатор первого системного стиля
        /// </summary>
        /// <param name="styleType">Тип стиля</param>
        public static string GetCurrentStyleGuid(Type styleType)
        {
            var functionName = GetFunctionNameByStyleType(styleType);
            return CurrentStyleGuid(functionName);
        }

        private static string CurrentStyleGuid(string functionName)
        {
            var savedStyleGuid = UserConfigFile.GetValue(functionName, "CurrentStyleGuid");
            if (!string.IsNullOrEmpty(savedStyleGuid))
            {
                return savedStyleGuid;
            }

            const string firstSystemGuid = "00000000-0000-0000-0000-000000000000";
            UserConfigFile.SetValue(functionName, "CurrentStyleGuid", firstSystemGuid, true);
            return firstSystemGuid;
        }

        /// <summary>
        /// Сохранить стиль как Текущий в настройки пользователя
        /// </summary>
        /// <param name="style"></param>
        public static void SaveCurrentStyleToSettings(IntellectualEntityStyle style)
        {
            UserConfigFile.SetValue(
                UserConfigFile.ConfigFileZone.Settings,
                "mp" + style.EntityType.Name,
                "CurrentStyleGuid",
                style.Guid,
                true);
        }

        /// <summary>
        /// Получить имя функции по имени типа стиля
        /// </summary>
        /// <param name="styleType">Тип стиля</param>
        /// <returns></returns>
        private static string GetFunctionNameByStyleType(Type styleType)
        {
            return "mp" + styleType.Name.Replace("Style", string.Empty);
        }

        /// <summary>
        /// Получить текущий стиль для типа примитива
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static IntellectualEntityStyle GetCurrentStyle(Type entityType)
        {
            try
            {
                LoadStylesFromXmlFile(entityType);

                var currentStyleGuid = GetCurrentStyleGuid(entityType);

                foreach (IntellectualEntityStyle style in EntityStyles)
                {
                    if (style.EntityType == entityType &&
                        style.Guid == currentStyleGuid)
                    {
                        return style;
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }

            return EntityStyles.First(s => s.EntityType == entityType && s.StyleType == StyleType.System);
        }

        /// <summary>
        /// Получить все стили для типа примитива
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static List<IntellectualEntityStyle> GetStyles(Type entityType)
        {
            LoadStylesFromXmlFile(entityType);

            return EntityStyles.Where(s => s.EntityType == entityType).ToList();
        }

        /// <summary>
        /// Возвращает имя стиля с указанными идентификатором для указанного типа примитива
        /// </summary>
        public static string GetStyleNameByGuid(Type entityType, string guid)
        {
            IntellectualEntityStyle style = EntityStyles.FirstOrDefault(s => s.EntityType == entityType && s.Guid == guid);
            if (style != null)
            {
                return style.Name;
            }

            // Стиль отсутствует в базе
            return Language.GetItem(Invariables.LangItem, "h103");
        }

        /// <summary>
        /// Возвращает стиль для указанного типа примитива по указанному имени стиля
        /// </summary>
        [CanBeNull]
        public static IntellectualEntityStyle GetStyleByName(Type entityType, string styleName)
        {
            return EntityStyles.FirstOrDefault(s => s.EntityType == entityType && s.Name == styleName);
        }

        /// <summary>
        /// Добавить стиль в список
        /// </summary>
        public static void AddStyle(IntellectualEntityStyle style)
        {
            EntityStyles.Add(style);
        }

        /// <summary>
        /// Удаление стиля из списка
        /// </summary>
        public static void RemoveStyle(IntellectualEntityStyle style)
        {
            EntityStyles.Remove(style);
        }

        /// <summary>
        /// Возвращает значение свойства "Имя слоя" из стиля
        /// </summary>
        public static string GetLayerNameProperty(this IntellectualEntityStyle style)
        {
            foreach (IntellectualEntityProperty property in style.Properties)
            {
                if (property.Name == "LayerName")
                {
                    return property.Value.ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Возвращает значение свойства "Тип линии" из стиля
        /// </summary>
        /// <returns>В случае неудачи возвращает "Continuous"</returns>
        public static string GetLineTypeProperty(this IntellectualEntityStyle style)
        {
            foreach (IntellectualEntityProperty property in style.Properties)
            {
                if (property.Name == "LineType")
                {
                    return property.Value.ToString();
                }
            }

            return "Continuous";
        }

        /// <summary>
        /// Возвращает значение свойства "Текстовый стиль" из стиля
        /// </summary>
        /// <returns>В случае неудачи возвращает "Standard"</returns>
        public static string GetTextStyleProperty(this IntellectualEntityStyle style)
        {
            foreach (IntellectualEntityProperty property in style.Properties)
            {
                if (property.Name == "TextStyle")
                {
                    return property.Value.ToString();
                }
            }

            return "Standard";
        }

        /// <summary>
        /// Применить к указанному интеллектуальному примитиву свойства из стиля
        /// </summary>
        /// <param name="entity">Экземпляр примитива</param>
        /// <param name="style">Стиль</param>
        /// <param name="isOnEntityCreation">True - применение стиля происходит при создании примитива.
        /// False - применение стиля происходит при выборе стиля в палитре</param>
        public static void ApplyStyle(this IntellectualEntity entity, IntellectualEntityStyle style, bool isOnEntityCreation)
        {
            var type = entity.GetType();
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                var attribute = propertyInfo.GetCustomAttribute<EntityPropertyAttribute>();
                if (attribute != null)
                {
                    var propertyFromStyle = style.Properties.FirstOrDefault(sp => sp.Name == attribute.Name);
                    if (propertyFromStyle != null)
                    {
                        if (attribute.Name == "Scale")
                        {
                            if (isOnEntityCreation)
                            {
                                if (MainStaticSettings.Settings.UseScaleFromStyle)
                                {
                                    propertyInfo.SetValue(entity, propertyFromStyle.Value);
                                }
                                else
                                {
                                    entity.Scale = AcadHelpers.GetCurrentScale();
                                }
                            }
                            else
                            {
                                propertyInfo.SetValue(entity, propertyFromStyle.Value);
                            }
                        }
                        else if (attribute.Name == "LayerName")
                        {
                            var layerName = propertyFromStyle.Value.ToString();
                            if (string.IsNullOrEmpty(layerName))
                            {
                                layerName = Language.GetItem(Invariables.LangItem, "defl");
                            }

                            if (isOnEntityCreation)
                            {
                                if (MainStaticSettings.Settings.UseLayerFromStyle)
                                {
                                    propertyInfo.SetValue(entity, layerName);
                                    AcadHelpers.SetLayerByName(entity.BlockId, layerName, style.LayerXmlData);
                                }
                            }
                            else
                            {
                                propertyInfo.SetValue(entity, layerName);
                                AcadHelpers.SetLayerByName(entity.BlockId, layerName, style.LayerXmlData);
                            }
                        }
                        else if (attribute.Name == "LineType")
                        {
                            var lineType = propertyFromStyle.Value.ToString();
                            AcadHelpers.SetLineType(entity.BlockId, lineType);
                        }
                        else if (attribute.Name == "TextStyle")
                        {
                            var apply = false;
                            if (isOnEntityCreation)
                            {
                                if (MainStaticSettings.Settings.UseTextStyleFromStyle)
                                {
                                    apply = true;
                                }
                            }
                            else
                            {
                                apply = true;
                            }

                            if (apply)
                            {
                                var textStyleName = propertyFromStyle.Value.ToString();
                                if (TextStyleHelper.HasTextStyle(textStyleName))
                                {
                                    propertyInfo.SetValue(entity, textStyleName);
                                }
                                else
                                {
                                    if (MainStaticSettings.Settings.IfNoTextStyle == 1 &&
                                        TextStyleHelper.CreateTextStyle(style.TextStyleXmlData))
                                    {
                                        propertyInfo.SetValue(entity, textStyleName);
                                    }
                                }
                            }
                        }
                        else
                        {
                            propertyInfo.SetValue(entity, propertyFromStyle.Value);
                        }
                    }
                }
                else
                {
                    if (propertyInfo.Name == "StyleGuid")
                    {
                        propertyInfo.SetValue(entity, style.Guid);
                    }
                }
            }
        }
    }
}