namespace mpESKD.Base
{
    using System;
    using Enums;
    using ModPlusAPI.Annotations;

    public class EntityPropertyAttribute : Attribute
    {
        public EntityPropertyAttribute(
            PropertiesCategory category, 
            string name,
            string displayNameLocalizationKey,
            string descriptionLocalizationKey,
            Type valueType,
            object defaultValue,
            object minimum, 
            object maximum)
        {
            Category = category;
            Name = name;
            DisplayNameLocalizationKey = displayNameLocalizationKey;
            DescriptionLocalizationKey = descriptionLocalizationKey;
            ValueType = valueType;
            DefaultValue = defaultValue;
            Minimum = minimum;
            Maximum = maximum;
        }
        
        public PropertiesCategory Category { get; }

        public string Name { get;  }

        public string DisplayNameLocalizationKey { get; }

        public string DescriptionLocalizationKey { get;  }

        //todo need?
        public Type ValueType { get;  }
        
        public object DefaultValue { get;  }

        [CanBeNull]
        public object Minimum { get;  }

        [CanBeNull]
        public object Maximum { get;  }
    }
}
