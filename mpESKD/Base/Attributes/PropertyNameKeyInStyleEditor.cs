namespace mpESKD.Base.Attributes
{
    using System;

    /// <summary>
    /// Атрибут, указывающий ключ локализации для имени свойства примитива, отображаемое в
    /// редакторе стилей. Атрибут нужно задавать для тех свойств, отображаемое имя которых отличается
    /// в палитре свойств и в редакторе (обычно, когда в скобках нужно указать значение с картинки)
    /// </summary>
    public class PropertyNameKeyInStyleEditor : Attribute
    {
        public PropertyNameKeyInStyleEditor(string localizationKey)
        {
            LocalizationKey = localizationKey;
        }

        /// <summary>
        /// Ключ получения локализованного значения
        /// </summary>
        public string LocalizationKey { get; }
    }
}