// ReSharper disable InconsistentNaming
//todo remove this all
namespace mpESKD.Base.Properties
{
    using System;

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

    public abstract class MPCOBaseProperty
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }
        
        public string Description { get; set; }
        
        public MPCOPropertyType PropertyType { get; set; }
    }

    public class MPCOStringProperty : MPCOBaseProperty
    {
        public MPCOStringProperty()
        {
            PropertyType = MPCOPropertyType.String;
        }

        public string Value { get; set; }
        
        public string DefaultValue { get; set; }

        public MPCOStringProperty Clone(bool useDefaultValue)
        {
            return new MPCOStringProperty
            {
                Name = Name,
                DisplayName = DisplayName,
                Description = Description,
                Value = useDefaultValue ? DefaultValue : Value,
                DefaultValue = DefaultValue
            };
        }
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

        public MPCOIntProperty Clone(bool useDefaultValue)
        {
            return new MPCOIntProperty
            {
                Name = Name,
                DisplayName = DisplayName,
                Description = Description,
                Value = useDefaultValue ? DefaultValue : Value,
                DefaultValue = DefaultValue,
                Minimum = Minimum,
                Maximum = Maximum
            };
        }
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

        public MPCODoubleProperty Clone(bool useDefaultValue)
        {
            return new MPCODoubleProperty
            {
                Name = Name,
                DisplayName = DisplayName,
                Description = Description,
                Value = useDefaultValue ? DefaultValue : Value,
                DefaultValue = DefaultValue,
                Minimum = Minimum,
                Maximum = Maximum
            };
        }
    }

    public class MPCOBoolProperty : MPCOBaseProperty
    {
        public MPCOBoolProperty()
        {
            PropertyType = MPCOPropertyType.Bool;
        }

        public bool Value { get; set; }
        
        public bool DefaultValue { get; set; }

        public MPCOBoolProperty Clone(bool useDefaultValue)
        {
            return new MPCOBoolProperty
            {
                Name = Name,
                DisplayName = DisplayName,
                Description = Description,
                Value = useDefaultValue ? DefaultValue : Value,
                DefaultValue = DefaultValue
            };
        }
    }

    public class MPCOTypeProperty<T> : MPCOBaseProperty
    {
        public MPCOTypeProperty()
        {
            PropertyType = MPCOPropertyType.Type;
        }

        public T Value { get; set; }
        
        public T DefaultValue { get; set; }

        public MPCOTypeProperty<T> Clone(bool useDefaultValue)
        {
            return new MPCOTypeProperty<T>
            {
                Name = Name,
                DisplayName = DisplayName,
                Description = Description,
                Value = useDefaultValue ? DefaultValue : Value,
                DefaultValue = DefaultValue
            };
        }
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
