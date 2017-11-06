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
        public static MPCOIntProperty FirstMarkerTypePropertyDescriptive = new MPCOIntProperty
        {
            PropertyType = MPCOPropertyType.Int,
            Name = "FirstMarkerType",
            DisplayName = "Тип первого маркера:",
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 1,
            Description = "Тип первого маркера"
        };
        public static MPCOIntProperty SecondMarkerTypePropertyDescriptive = new MPCOIntProperty
        {
            PropertyType = MPCOPropertyType.Int,
            Name = "SecondMarkerType",
            DisplayName = "Тип второго маркера:",
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 1,
            Description = "Тип второго маркера"
        };
        public static MPCOIntProperty ThirdMarkerTypePropertyDescriptive = new MPCOIntProperty
        {
            PropertyType = MPCOPropertyType.Int,
            Name = "ThirdMarkerType",
            DisplayName = "Тип третьего маркера:",
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 1,
            Description = "Тип третьего маркера"
        };
        /// <summary>Поле, описывающее свойство "Нижний отступ излома"</summary>
        public static MPCOIntProperty BottomFractureOffsetPropertyDescriptive = new MPCOIntProperty
        {
            PropertyType = MPCOPropertyType.Int,
            Name = "BottomFractureOffset",
            DisplayName = "Нижний отступ излома:",
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 30,
            Description = "Нижний отступ излома"
        };
        /// <summary>Поле, описывающее свойство "Верхний отступ излома"</summary>
        public static MPCOIntProperty TopFractureOffsetPropertyDescriptive = new MPCOIntProperty
        {
            PropertyType = MPCOPropertyType.Int,
            Name = "TopFractureOffset",
            DisplayName = "Верхний отступ излома:",
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 30,
            Description = "Верхний отступ излома"
        };
        public static MPCOStringProperty TextStylePropertyDescriptive = new MPCOStringProperty
        {
            PropertyType = MPCOPropertyType.String,
            Name = "TextStyle",
            DisplayName = "Текстовый стиль:",
            DefaultValue = "Standard",
            Description = "Текстовый стиль"
        };
        public static MPCODoubleProperty TextHeightPropertyDescriptive = new MPCODoubleProperty
        {
            PropertyType = MPCOPropertyType.Double,
            Name = "TextHeight",
            DisplayName = "Высота текста:",
            DefaultValue = 3.5,
            Minimum = 0.000000001,
            Maximum = double.MaxValue,
            Description = "Высота текста"
        };
        // General
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
            DefaultValue = 10.0,
            Minimum = 0,
            Maximum = double.MaxValue,
            Description = "Масштаб типа линии для заданного в свойствах блока типа линии"
        };
        public static MPCOStringProperty LineTypePropertyDescriptive = new MPCOStringProperty
        {
            PropertyType = MPCOPropertyType.String,
            Name = "LineType",
            DisplayName = "Тип линии:",
            DefaultValue = "осевая",
            Description = "Тип линии оси"
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
