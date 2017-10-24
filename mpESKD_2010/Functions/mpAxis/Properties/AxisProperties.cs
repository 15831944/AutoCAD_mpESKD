using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using mpESKD.Base.Properties;

namespace mpESKD.Functions.mpAxis.Properties
{
    public static class AxisProperties
    {
        /// <summary>Поле, описывающее свойство "Тип линии обрыва"</summary>
        public static MPCOTypeProperty<AxisMarkersPosition> MarkersPositionPropertyDescriptive = new MPCOTypeProperty<AxisMarkersPosition>
        {
            PropertyType = MPCOPropertyType.Type,
            Name = "MarkersPosition",
            DisplayName = "Позиция маркеров:",
            DefaultValue = AxisMarkersPosition.Bottom,
            Description = "Позиция маркеров: с двух сторон, сверху или снизу"
        };
        /// <summary>Поле, описывающее свойство "Излом"</summary>
        public static MPCOIntProperty FracturePropertyDescriptive = new MPCOIntProperty
        {
            PropertyType = MPCOPropertyType.Int,
            Name = "Fracture",
            DisplayName = "Излом:",
            DefaultValue = 10,
            Minimum = 1,
            Maximum = 20,
            Description = "Высота излома оси"
        };
        /// <summary>Поле, описывающее свойство "Диаметр маркеров"</summary>
        public static MPCOIntProperty MarkersDiameterPropertyDescriptive = new MPCOIntProperty
        {
            PropertyType = MPCOPropertyType.Int,
            Name = "MarkersDiameter",
            DisplayName = "Диаметр маркеров:",
            DefaultValue = 10,
            Minimum = 6,
            Maximum = 12,
            Description = "Диаметр маркеров оси"
        };
        /// <summary>Поле, описывающее свойство "Количество маркеров"</summary>
        public static MPCOIntProperty MarkersCountPropertyDescriptive = new MPCOIntProperty
        {
            PropertyType = MPCOPropertyType.Int,
            Name = "MarkersCount",
            DisplayName = "Количество маркеров:",
            DefaultValue = 1,
            Minimum = 1,
            Maximum = 3,
            Description = "Количество маркеров оси"
        };
        /// <summary>Поле, описывающее свойство "Масштаб"</summary>
        public static MPCOTypeProperty<AnnotationScale> ScalePropertyDescriptive = new MPCOTypeProperty<AnnotationScale>
        {
            PropertyType = MPCOPropertyType.Type,
            Name = "Scale",
            DisplayName = "Масштаб:",
            DefaultValue = new AnnotationScale { Name = "1:1", DrawingUnits = 1.0, PaperUnits = 1.0 },
            Description = "Масштаб линии обрыва"
        };
        /// <summary>Поле, описывающее свойство "Масштаб типа линии"</summary>
        public static MPCODoubleProperty LineTypeScalePropertyDescriptive = new MPCODoubleProperty
        {
            PropertyType = MPCOPropertyType.Double,
            Name = "LineTypeScale",
            DisplayName = "Масштаб типа линий:",
            DefaultValue = 1.0,
            Minimum = 0,
            Maximum = double.MaxValue,
            Description = "Масштаб типа линии для заданного в свойствах блока типа линии"
        };
        /// <summary>Поле, описывающее слой</summary>
        public static MPCOStringProperty LayerName = new MPCOStringProperty
        {
            PropertyType = MPCOPropertyType.String,
            Name = "LayerName",
            DisplayName = "Слой",
            DefaultValue = "По умолчанию",
            Description = "Слой примитива"
        };
    }
    public static class AxisPropertiesHelpers
    {
        public static List<string> BreakLineTypeLocalNames = new List<string> { "С двух сторон", "Сверху", "Снизу" };
        #region Methods

        public static AxisMarkersPosition GetAxisMarkersPositionByLocalName(string local)
        {
            if (local == "С двух сторон") return AxisMarkersPosition.Both;
            if (local == "Сверху") return AxisMarkersPosition.Top;
            if (local == "Снизу") return AxisMarkersPosition.Bottom;
            return AxisMarkersPosition.Bottom;
        }

        public static string GetLocalAxisMarkersPositionName(AxisMarkersPosition breakLineType)
        {
            if (breakLineType == AxisMarkersPosition.Both) return "С двух сторон";
            if (breakLineType == AxisMarkersPosition.Top) return "Сверху";
            if (breakLineType == AxisMarkersPosition.Bottom) return "Снизу";
            return "Снизу";
        }

        public static AxisMarkersPosition GetAxisMarkersPositionFromString(string str)
        {
            if (str == "Both") return AxisMarkersPosition.Both;
            if (str == "Top") return AxisMarkersPosition.Top;
            if (str == "Bottom") return AxisMarkersPosition.Bottom;
            return AxisMarkersPosition.Bottom;
        }
        #endregion
    }
    /// <summary>Вариант позиции маркеров оси</summary>
    public enum AxisMarkersPosition
    {
        /// <summary>С обеих сторон</summary>
        Both,
        /// <summary>Сверху</summary>
        Top,
        /// <summary>Снизу</summary>
        Bottom
    }
}
