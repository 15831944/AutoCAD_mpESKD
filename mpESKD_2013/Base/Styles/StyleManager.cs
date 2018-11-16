namespace mpESKD.Base.Styles
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Enums;
    using Helpers;
    using ModPlusAPI;
    using ModPlusAPI.Annotations;
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

            //todo remove
            if (AllStyles == null)
                AllStyles = new List<StyleMapItem>();
        }

        /// <summary>
        /// Список всех стилей всех примитивов
        /// </summary>
        private static readonly List<StyleMapItem> AllStyles;

        /// <summary>
        /// Получить список стилей для указанного типа стиля
        /// </summary>
        /// <typeparam name="T">Тип стиля</typeparam>
        public static List<T> GetStyles<T>() where T : MPCOStyle
        {
            List<T> styles = new List<T>();

            foreach (StyleMapItem styleMapItem in AllStyles)
            {
                if (styleMapItem.StyleType == typeof(T))
                    styles.AddRange(styleMapItem.Styles.Cast<T>());
            }

            return styles;
        }

        public static List<MPCOStyle> GetStyles(string type)
        {
            List<MPCOStyle> stylesNames = new List<MPCOStyle>();
            foreach (StyleMapItem styleMapItem in AllStyles)
            {
                if (styleMapItem.StyleType.Name == type)
                    stylesNames.AddRange(styleMapItem.Styles);
            }

            return stylesNames;
        }

        public static string GetStyleNameByGuid(string type, string guid)
        {
            foreach (StyleMapItem styleMapItem in AllStyles)
            {
                if (styleMapItem.StyleType.Name != type)
                    continue;

                foreach (var mpcoStyle in styleMapItem.Styles)
                {
                    if (mpcoStyle.Guid == guid)
                        return mpcoStyle.Name;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Удаление стиля из списка по его идентификатору
        /// </summary>
        /// <param name="styleGuid"></param>
        public static void RemoveStyle(string styleGuid)
        {
            var stopIteration = false;
            foreach (StyleMapItem styleMapItem in AllStyles)
            {
                if (stopIteration)
                    break;
                for (var i = styleMapItem.Styles.Count - 1; i >= 0; i--)
                {
                    if (styleMapItem.Styles[i].Guid == styleGuid)
                    {
                        styleMapItem.Styles.RemoveAt(i);
                        stopIteration = true;
                        break;
                    }
                }
            }
        }

        /// <summary>Получение стиля из коллекции по его идентификатору или первого системного стиля, если не найден
        /// В случае, если коллекция пустая, то происходит ее загрузка (с созданием, если нужно)</summary>
        /// <returns></returns>
        public static T GetCurrentStyle<T>(
            List<T> systemStyles,
            Func<XElement, T> parseStylePropertiesFromXElementAction) where T : MPCOStyle
        {
            try
            {
                LoadStylesFromXmlFile(systemStyles, parseStylePropertiesFromXElementAction);

                var currentStyleGuid = GetCurrentStyleGuid(typeof(T));

                foreach (StyleMapItem styleMapItem in AllStyles)
                {
                    if (styleMapItem.StyleType == typeof(T))
                    {
                        foreach (var style in styleMapItem.Styles)
                        {
                            if (style.Guid.Equals(currentStyleGuid))
                                return (T)style;
                        }
                    }
                }

                return AllStyles
                    .Where(smi => smi.StyleType == typeof(T))
                    .SelectMany(smi => smi.Styles)
                    .Cast<T>()
                    .First(s => s.StyleType == MPCOStyleType.System);

            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
                return systemStyles.First();
            }
        }

        /// <summary>
        /// Перезагрузка стилей указанного стиля: из списка удаляются все стили типа, а затем грузятся заново
        /// </summary>
        /// <typeparam name="T">Тип стиля</typeparam>
        /// <param name="systemStyles">Список системных стилей указанного типа</param>
        /// <param name="parseStylePropertiesFromXElementAction">Метод парсинга свойств стиля их XElement</param>
        public static void ReloadStyles<T>(List<T> systemStyles, Func<XElement, T> parseStylePropertiesFromXElementAction) where T : MPCOStyle
        {
            var styleMapItem = AllStyles.FirstOrDefault(smi => smi.StyleType == typeof(T));
            if (styleMapItem != null)
                AllStyles.Remove(styleMapItem);

            LoadStylesFromXmlFile(systemStyles, parseStylePropertiesFromXElementAction);
        }

        /// <summary>
        /// Загрузка стилей указанного типа из xml-файла хранения стилей указанного типа
        /// </summary>
        /// <typeparam name="T">Тип стиля</typeparam>
        /// <param name="systemStyles">Список системных стилей указанного типа</param>
        /// <param name="parseStylePropertiesFromXElementAction">Метод парсинга свойств стиля их XElement</param>
        public static void LoadStylesFromXmlFile<T>(List<T> systemStyles, Func<XElement, T> parseStylePropertiesFromXElementAction) where T : MPCOStyle
        {
            var styleMapItem = AllStyles.FirstOrDefault(smi => smi.StyleType == typeof(T));
            if (styleMapItem != null)
            {
                foreach (T systemStyle in systemStyles)
                {
                    if (!styleMapItem.Styles.Any(s => s.Guid == systemStyle.Guid))
                        styleMapItem.Styles.Add(systemStyle);
                }
            }
            else
            {
                styleMapItem = new StyleMapItem(typeof(T));
                styleMapItem.Styles.AddRange(systemStyles);
                AllStyles.Add(styleMapItem);
            }

            var stylesFile = Path.Combine(MainFunction.StylesPath, GetStylesFileName(typeof(T)));
            if (File.Exists(stylesFile))
            {
                var fXel = XElement.Load(stylesFile);
                foreach (XElement styleXel in fXel.Elements("UserStyle"))
                {
                    var style = parseStylePropertiesFromXElementAction(styleXel);

                    style.Name = styleXel.Attribute(nameof(style.Name))?.Value;
                    style.Description = styleXel.Attribute(nameof(style.Description))?.Value;

                    // Guid беру, если есть атрибут. Иначе создаю новый
                    var guidAttr = styleXel.Attribute(nameof(style.Guid));
                    style.Guid = guidAttr?.Value ?? Guid.NewGuid().ToString();

                    if (styleMapItem.Styles.Any(s => s.Guid == style.Guid))
                        continue;

                    // get layer
                    var layerData = styleXel.Element("LayerTableRecord");
                    style.LayerXmlData = layerData ?? null;

                    // get text style
                    var textStyleData = styleXel.Element("TextStyleTableRecord");
                    style.TextStyleXmlData = textStyleData ?? null;

                    // add style
                    styleMapItem.Styles.Add(style);
                }
            }
        }

        /// <summary>
        /// Сохранение стилей указанного типа, представленных типом-презентором, в xml-файл хранения стилей указанного типа
        /// </summary>
        /// <typeparam name="TOne">Тип стиля</typeparam>
        /// <typeparam name="TTwo">Тип презентора</typeparam>
        /// <param name="stylesForEditor">Список стилей-презенторов</param>
        /// <param name="convertStyleForEditorToXElement">Метод парсинга стиля-презентора в XElement</param>
        public static void SaveStylesToXml<TOne, TTwo>(List<TTwo> stylesForEditor, Func<TTwo, XElement> convertStyleForEditorToXElement)
            where TOne : MPCOStyle
            where TTwo : MPCOStyleForEditor
        {
            var stylesFile = Path.Combine(MainFunction.StylesPath, GetStylesFileName(typeof(TOne)));

            // Если файла нет, то создаем
            if (!File.Exists(stylesFile))
                new XElement("Styles").Save(stylesFile);
            try
            {
                var fXel = XElement.Load(stylesFile);
                fXel.RemoveAll();
                foreach (var style in stylesForEditor)
                {
                    if (!style.CanEdit)
                        continue;

                    XElement styleXel = convertStyleForEditorToXElement(style);

                    fXel.Add(styleXel);
                }
                fXel.Save(stylesFile);
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        /// <summary>
        /// Проверка и создание в случае необходимости файла стилей
        /// </summary>
        public static void CheckStylesFile<T>() where T : MPCOStyle
        {
            bool needToCreate;
            var stylesFile = Path.Combine(MainFunction.StylesPath, GetStylesFileName(typeof(T)));
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

        /// <summary>
        /// Получение имени файла хранения стилей. Так как все классы имеют типичные имена ([entity]Style)
        /// то имя файла получаю по имени класса с прибавлением "s.xml". Типа GroundLineStyle + s.xml
        /// </summary>
        /// <param name="styleType"></param>
        private static string GetStylesFileName(Type styleType)
        {
            return styleType.Name + "s.xml";
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
        /// Класс, представляющий тип стиля и список стилей этого типа
        /// </summary>
        internal class StyleMapItem
        {
            public StyleMapItem(Type styleType)
            {
                StyleType = styleType;
                Styles = new List<MPCOStyle>();
            }

            /// <summary>
            /// Тип стиля
            /// </summary>
            public Type StyleType { get; }

            /// <summary>
            /// Список стилей
            /// </summary>
            public List<MPCOStyle> Styles { get; }
        }

        #region NEW

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
                else needToCreate = true;

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
                    Name = LocalizationHelper.GetEntityLocalizationName(et),
                    Description = TypeFactory.Instance.GetSystemStyleLocalizedDescription(et),
                    Guid = "00000000-0000-0000-0000-000000000000",
                    StyleType = StyleType.System
                };
                if (!HasStyle(systemStyle))
                {
                    foreach (PropertyInfo propertyInfo in et.GetProperties())
                    {
                        var attribute = propertyInfo.GetCustomAttribute<EntityPropertyAttribute>();
                        if (attribute != null && attribute.Name != "Style")
                        {
                            systemStyle.Properties.Add(new IntellectualEntityProperty(attribute, et, attribute.DefaultValue, ObjectId.Null));
                        }
                    }

                    EntityStyles.Add(systemStyle);
                }
            });
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
                        continue;

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
                                        attribute, entityType,
                                        Parsers.AnnotationScaleFromString(xmlValue),
                                        ObjectId.Null));
                                }
                                else
                                {
                                    if (valueType == typeof(string))
                                    {
                                        style.Properties.Add(new IntellectualEntityProperty(attribute, entityType, xmlProperty.Attribute("Value").Value, ObjectId.Null));
                                    }
                                    else if (valueType == typeof(int))
                                    {
                                        style.Properties.Add(new IntellectualEntityProperty(
                                            attribute, entityType,
                                            int.TryParse(xmlValue, out var i) ? i : attribute.DefaultValue,
                                            ObjectId.Null));
                                    }
                                    else if (valueType == typeof(double))
                                    {
                                        style.Properties.Add(new IntellectualEntityProperty(
                                            attribute, entityType,
                                            double.TryParse(xmlValue, out var d) ? d : attribute.DefaultValue,
                                            ObjectId.Null));
                                    }
                                    else if (valueType == typeof(bool))
                                    {
                                        style.Properties.Add(new IntellectualEntityProperty(
                                            attribute, entityType,
                                            bool.TryParse(xmlValue, out var b) ? b : attribute.DefaultValue,
                                            ObjectId.Null));
                                    }
                                    else if (valueType == typeof(Enum))
                                    {
                                        try
                                        {
                                            style.Properties.Add(new IntellectualEntityProperty(
                                                attribute, entityType,
                                                Enum.Parse(attribute.DefaultValue.GetType(), xmlValue),
                                                ObjectId.Null));
                                        }
                                        catch
                                        {
                                            style.Properties.Add(new IntellectualEntityProperty(attribute, entityType, attribute.DefaultValue, ObjectId.Null));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // add style
                    EntityStyles.Add(style);
                }
            }
        }

        //todo save to xml

        /// <summary>
        /// Получение идентификатора текущего стиля, сохраненного в настройках или идентификатор первого системного стиля
        /// </summary>
        /// <param name="styleType">Тип стиля</param>
        /// <returns></returns>
        public static string GetCurrentStyleGuid(Type styleType)
        {
            var functionName = GetFunctionNameByStyleType(styleType);
            var savedStyleGuid = UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, functionName, "CurrentStyleGuid");
            if (!string.IsNullOrEmpty(savedStyleGuid))
                return savedStyleGuid;

            const string firstSystemGuid = "00000000-0000-0000-0000-000000000000";
            UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, functionName, "CurrentStyleGuid", firstSystemGuid, true);
            return firstSystemGuid;
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
                        return style;
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
            return EntityStyles.Where(s => s.EntityType == entityType).ToList();
        }

        /// <summary>
        /// Возвращает имя стиля с указанными идентификатором для указанного типа примитива
        /// </summary>
        public static string GetStyleNameByGuid(Type entityType, string guid)
        {
            return EntityStyles.FirstOrDefault(s => s.EntityType == entityType && s.Guid == guid)?.Name;
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
        /// Возвращает значение свойства "Имя слоя" из стиля
        /// </summary>
        public static string GetLayerNameProperty(this IntellectualEntityStyle style)
        {
            foreach (IntellectualEntityProperty property in style.Properties)
            {
                if (property.Name == "LayerName")
                    return property.Value.ToString();
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
                    return property.Value.ToString();
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
                    return property.Value.ToString();
            }

            return "Standard";
        }

        /// <summary>
        /// Применить к указанному интеллектуальному примитиву свойства из стиля
        /// </summary>
        /// <param name="entity">Экземпляр примитива</param>
        /// <param name="style">Стиль</param>
        public static void ApplyStyle(this IntellectualEntity entity, IntellectualEntityStyle style)
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
                            propertyInfo.SetValue(
                                entity,
                                MainStaticSettings.Settings.UseScaleFromStyle
                                    ? AcadHelpers.GetAnnotationScaleByName(propertyFromStyle.Value.ToString())
                                    : AcadHelpers.Database.Cannoscale);
                        }
                        else if (attribute.Name == "LayerName")
                        {
                            var layerName = propertyFromStyle.Value.ToString();
                            if (string.IsNullOrEmpty(layerName))
                                layerName = Language.GetItem(MainFunction.LangItem, "defl");
                            AcadHelpers.SetLayerByName(entity.BlockId, layerName, style.LayerXmlData);
                        }
                        else if (attribute.Name == "LineType")
                        {
                            var lineType = propertyFromStyle.Value.ToString();
                            AcadHelpers.SetLineType(entity.BlockId, lineType);
                        }
                        else if (attribute.Name == "TextStyle")
                        {
                            if (MainStaticSettings.Settings.UseTextStyleFromStyle)
                            {
                                var textStyleName = propertyFromStyle.Value.ToString();
                                if (TextStyleHelper.HasTextStyle(textStyleName))
                                    propertyInfo.SetValue(entity, textStyleName);
                                else
                                {
                                    if (MainStaticSettings.Settings.IfNoTextStyle == 1 &&
                                        TextStyleHelper.CreateTextStyle(style.TextStyleXmlData))
                                        propertyInfo.SetValue(entity, textStyleName);
                                }
                            }
                        }
                        else propertyInfo.SetValue(entity, propertyFromStyle.Value);
                    }
                }
                else
                {
                    if (propertyInfo.Name == "StyleGuid")
                        propertyInfo.SetValue(entity, style.Guid);
                }
            }
        }

        #endregion
    }
}