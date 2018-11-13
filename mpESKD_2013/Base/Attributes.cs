namespace mpESKD.Base
{
    using System;
    using ModPlusAPI.Annotations;

    public class EntityPropertyAttribute : Attribute
    {
        public EntityPropertyAttribute(string category, string name, string displayName, string description, Type valueType, object defaultValue, object minimum, object maximum)
        {
            Category = category;
            Name = name;
            DisplayName = displayName;
            Description = description;
            ValueType = valueType;
            DefaultValue = defaultValue;
            Minimum = minimum;
            Maximum = maximum;
        }
        //todo enum?
        public string Category { get; }

        public string Name { get;  }

        public string DisplayName { get; }

        public string Description { get;  }

        public Type ValueType { get;  }
        
        public object DefaultValue { get;  }

        [CanBeNull]
        public object Minimum { get;  }

        [CanBeNull]
        public object Maximum { get;  }
    }
}
