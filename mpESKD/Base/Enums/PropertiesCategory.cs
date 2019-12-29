namespace mpESKD.Base.Enums
{
    using Attributes;

    public enum PropertiesCategory
    {
        Undefined = -1,

        [EnumPropertyDisplayValueKey("h49")]
        General = 1,

        [EnumPropertyDisplayValueKey("h51")]
        Geometry = 2,

        [EnumPropertyDisplayValueKey("h64")]
        Content = 3
    }
}
