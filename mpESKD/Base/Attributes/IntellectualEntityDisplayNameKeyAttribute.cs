namespace mpESKD.Base.Attributes
{
    using System;

    /// <summary>
    /// Атрибут, указывающий ключ локализации для имени интеллектуального примитива
    /// </summary>
    public class IntellectualEntityDisplayNameKeyAttribute : Attribute
    {
        public IntellectualEntityDisplayNameKeyAttribute(string localizationKey)
        {
            LocalizationKey = localizationKey;
        }

        public string LocalizationKey { get; }
    }
}