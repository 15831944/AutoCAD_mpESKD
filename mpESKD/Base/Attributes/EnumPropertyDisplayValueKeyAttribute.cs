namespace mpESKD.Base.Attributes
{
    using System;

    /// <summary>
    /// Атрибут, указывающий ключ локализации для поля перечислителя, используемого в свойствах примитива
    /// </summary>
    public class EnumPropertyDisplayValueKeyAttribute : Attribute
    {
        public EnumPropertyDisplayValueKeyAttribute(string localizationKey)
        {
            LocalizationKey = localizationKey;
        }

        /// <summary>
        /// Ключ получения локализованного значения
        /// </summary>
        public string LocalizationKey { get; }
    }
}