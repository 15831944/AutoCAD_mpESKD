namespace mpESKD.Base.Attributes
{
    using System;

    /// <summary>
    /// Атрибут, указывающий зависимость видимости свойства от другого свойства.
    /// Принцип работы: в классе примитива нужно создать специальное свойство типа bool к которому
    /// нужно указать данный атрибут. 
    /// В список зависимых свойств DependencyProperties нужно внести имена тех свойств, видимость которых
    /// зависит от свойства, для которого установлен атрибут
    /// Самой свойство, для которого установлен атрибут, должно менять внутри класса примитива
    /// </summary>
    public class PropertyVisibilityDependencyAttribute : Attribute
    {
        public PropertyVisibilityDependencyAttribute(string[] dependencyProperties)
        {
            DependencyProperties = dependencyProperties;
        }

        /// <summary>
        /// Свойства, видимость которых зависит от свойства, для которого установлен атрибут
        /// </summary>
        public string[] DependencyProperties { get; }
    }
}