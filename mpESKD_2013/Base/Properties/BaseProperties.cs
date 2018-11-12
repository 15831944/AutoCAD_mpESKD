// ReSharper disable InconsistentNaming
namespace mpESKD.Base.Properties
{
    public interface IGeneralProperties
    {
        /// <summary>
        /// Стиль интеллектуального примитива
        /// </summary>
        string Style { get; set; }

        /// <summary>
        /// Масштаб интеллектуального примитива
        /// </summary>
        string Scale { get; set; }

        /// <summary>
        /// Масштаб типа линии
        /// </summary>
        double LineTypeScale { get; set; }

        /// <summary>
        /// Тип линии
        /// </summary>
        string LineType { get; set; }

        /// <summary>
        /// Имя слоя
        /// </summary>
        string LayerName { get; set; }
    }

    public class MPCOBaseProperty
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public MPCOPropertyType PropertyType { get; set; }

        public MPCOPropertyType GePropertyTypeByString(string type)
        {
            if (type == "Int")
                return MPCOPropertyType.Int;
            if (type == "Double")
                return MPCOPropertyType.Double;
            if (type == "Type")
                return MPCOPropertyType.Type;
            if (type == "Bool")
                return MPCOPropertyType.Bool;
            // or string
            return MPCOPropertyType.String;
        }
    }

    public class MPCOStringProperty : MPCOBaseProperty
    {
        public MPCOStringProperty()
        {
            PropertyType = MPCOPropertyType.String;
        }
        public string Value { get; set; }
        public string DefaultValue { get; set; }
    }

    public class MPCOIntProperty : MPCOBaseProperty
    {
        public MPCOIntProperty()
        {
            PropertyType = MPCOPropertyType.Int;
        }
        public int Value { get; set; }
        public int DefaultValue { get; set; }
        public int Minimum { get; set; }
        public int Maximum { get; set; }
    }

    public class MPCODoubleProperty : MPCOBaseProperty
    {
        public MPCODoubleProperty()
        {
            PropertyType = MPCOPropertyType.Double;
        }
        public double Value { get; set; }
        public double DefaultValue { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
    }

    public class MPCOBoolProperty : MPCOBaseProperty
    {
        public MPCOBoolProperty()
        {
            PropertyType = MPCOPropertyType.Bool;
        }
        public bool Value { get; set; }
        public bool DefaultValue { get; set; }
    }

    public class MPCOTypeProperty<T> : MPCOBaseProperty
    {
        public MPCOTypeProperty()
        {
            PropertyType = MPCOPropertyType.Type;
        }
        public T Value { get; set; }
        public T DefaultValue { get; set; }
    }

    public class MPCOScaleProperty : MPCOBaseProperty
    {
        public string ScaleName { get; set; }
    }

    public enum MPCOPropertyType
    {
        String = 1,
        Int = 2,
        Double = 3,
        Type = 4,
        Bool = 5
    }
}
