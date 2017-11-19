using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using mpESKD.Base.Properties;

namespace mpESKD.Functions.mpAxis.Properties
{
    public static class AxisProperties
    {
        /// <summary>Поле, описывающее свойство "Тип линии обрыва"</summary>
        public static MPCOTypeProperty<AxisMarkersPosition> MarkersPosition = new MPCOTypeProperty<AxisMarkersPosition>
        {
            Name = "MarkersPosition",
            DisplayName = "Позиция маркеров:",
            DefaultValue = AxisMarkersPosition.Bottom,
            Description = "Позиция маркеров: с двух сторон, сверху или снизу"
        };
        /// <summary>Поле, описывающее свойство "Излом"</summary>
        public static MPCOIntProperty Fracture = new MPCOIntProperty
        {
            Name = "Fracture",
            DisplayName = "Излом:",
            DefaultValue = 10,
            Minimum = 1,
            Maximum = 20,
            Description = "Высота излома оси"
        };
        /// <summary>Поле, описывающее свойство "Диаметр маркеров"</summary>
        public static MPCOIntProperty MarkersDiameter = new MPCOIntProperty
        {
            Name = "MarkersDiameter",
            DisplayName = "Диаметр маркеров:",
            DefaultValue = 10,
            Minimum = 6,
            Maximum = 12,
            Description = "Диаметр маркеров оси"
        };
        /// <summary>Поле, описывающее свойство "Количество маркеров"</summary>
        public static MPCOIntProperty MarkersCount = new MPCOIntProperty
        {
            Name = "MarkersCount",
            DisplayName = "Количество маркеров:",
            DefaultValue = 1,
            Minimum = 1,
            Maximum = 3,
            Description = "Количество маркеров оси"
        };
        public static MPCOIntProperty FirstMarkerType = new MPCOIntProperty
        {
            Name = "FirstMarkerType",
            DisplayName = "Тип первого маркера:",
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 1,
            Description = "Тип первого маркера"
        };
        public static MPCOIntProperty SecondMarkerType = new MPCOIntProperty
        {
            Name = "SecondMarkerType",
            DisplayName = "Тип второго маркера:",
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 1,
            Description = "Тип второго маркера"
        };
        public static MPCOIntProperty ThirdMarkerType = new MPCOIntProperty
        {
            Name = "ThirdMarkerType",
            DisplayName = "Тип третьего маркера:",
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 1,
            Description = "Тип третьего маркера"
        };
        /// <summary>Поле, описывающее свойство "Нижний отступ излома"</summary>
        public static MPCOIntProperty BottomFractureOffset = new MPCOIntProperty
        {
            Name = "BottomFractureOffset",
            DisplayName = "Нижний отступ излома:",
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 30,
            Description = "Нижний отступ излома"
        };
        /// <summary>Поле, описывающее свойство "Верхний отступ излома"</summary>
        public static MPCOIntProperty TopFractureOffset = new MPCOIntProperty
        {
            Name = "TopFractureOffset",
            DisplayName = "Верхний отступ излома:",
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 30,
            Description = "Верхний отступ излома"
        };
        public static MPCOStringProperty TextStyle = new MPCOStringProperty
        {
            Name = "TextStyle",
            DisplayName = "Текстовый стиль:",
            DefaultValue = "Standard",
            Description = "Текстовый стиль"
        };
        public static MPCODoubleProperty TextHeight = new MPCODoubleProperty
        {
            Name = "TextHeight",
            DisplayName = "Высота текста:",
            DefaultValue = 3.5,
            Minimum = 0.000000001,
            Maximum = double.MaxValue,
            Description = "Высота текста"
        };
        // General
        /// <summary>Поле, описывающее свойство "Масштаб"</summary>
        public static MPCOTypeProperty<AnnotationScale> Scale = new MPCOTypeProperty<AnnotationScale>
        {
            Name = "Scale",
            DisplayName = "Масштаб:",
            DefaultValue = new AnnotationScale { Name = "1:1", DrawingUnits = 1.0, PaperUnits = 1.0 },
            Description = "Масштаб линии обрыва"
        };
        /// <summary>Поле, описывающее свойство "Масштаб типа линии"</summary>
        public static MPCODoubleProperty LineTypeScale = new MPCODoubleProperty
        {
            Name = "LineTypeScale",
            DisplayName = "Масштаб типа линий:",
            DefaultValue = 10.0,
            Minimum = 0,
            Maximum = double.MaxValue,
            Description = "Масштаб типа линии для заданного в свойствах блока типа линии"
        };
        public static MPCOStringProperty LineType = new MPCOStringProperty
        {
            Name = "LineType",
            DisplayName = "Тип линии:",
            DefaultValue = "осевая",
            Description = "Тип линии оси"
        };
        /// <summary>Поле, описывающее слой</summary>
        public static MPCOStringProperty LayerName = new MPCOStringProperty
        {
            Name = "LayerName",
            DisplayName = "Слой",
            DefaultValue = "По умолчанию",
            Description = "Слой примитива"
        };

        #region text
        // first
        public static MPCOStringProperty FirstTextPrefix = new MPCOStringProperty
        {
            Name = "FirstTextPrefix",
            DisplayName = "Префикс первого значения:",
            DefaultValue = string.Empty,
            Description = "Префикс первого значения"
        };
        public static MPCOStringProperty FirstTextSuffix = new MPCOStringProperty
        {
            Name = "FirstTextSuffix",
            DisplayName = "Суффикс первого значения:",
            DefaultValue = string.Empty,
            Description = "Суффикс первого значения"
        };
        public static MPCOStringProperty FirstText = new MPCOStringProperty
        {
            Name = "FirstText",
            DisplayName = "Первое значение:",
            DefaultValue = string.Empty,
            Description = "Первое значение"
        };
        // second
        public static MPCOStringProperty SecondTextPrefix = new MPCOStringProperty
        {
            Name = "SecondTextPrefix",
            DisplayName = "Префикс второго значения:",
            DefaultValue = string.Empty,
            Description = "Префикс второго значения"
        };
        public static MPCOStringProperty SecondTextSuffix = new MPCOStringProperty
        {
            Name = "SecondTextSuffix",
            DisplayName = "Суффикс второго значения:",
            DefaultValue = string.Empty,
            Description = "Суффикс второго значения"
        };
        public static MPCOStringProperty SecondText = new MPCOStringProperty
        {
            Name = "SecondText",
            DisplayName = "Второе значение:",
            DefaultValue = string.Empty,
            Description = "Второе значение"
        };
        // third
        public static MPCOStringProperty ThirdTextPrefix = new MPCOStringProperty
        {
            Name = "ThirdTextPrefix",
            DisplayName = "Префикс третьего значения:",
            DefaultValue = string.Empty,
            Description = "Префикс третьего значения"
        };
        public static MPCOStringProperty ThirdTextSuffix = new MPCOStringProperty
        {
            Name = "ThirdTextSuffix",
            DisplayName = "Суффикс третьего значения:",
            DefaultValue = string.Empty,
            Description = "Суффикс третьего значения"
        };
        public static MPCOStringProperty ThirdText = new MPCOStringProperty
        {
            Name = "ThirdText",
            DisplayName = "Третье значение:",
            DefaultValue = string.Empty,
            Description = "Третье значение"
        };
        // Orient markers
        public static MPCOIntProperty ArrowsSize = new MPCOIntProperty
        {
            Name = "ArrowsSize",
            DisplayName = "Размер стрелок:",
            Minimum = 0,
            Maximum = 5,
            DefaultValue = 1,
            Description = "Размер стрелок"
        };
        public static MPCOStringProperty BottomOrientText = new MPCOStringProperty
        {
            Name = "BottomOrientText",
            DisplayName = "Значение нижнего маркера-ориентира:",
            DefaultValue = string.Empty,
            Description = "Значение нижнего маркера-ориентира"
        };
        public static MPCOStringProperty TopOrientText = new MPCOStringProperty
        {
            Name = "TopOrientText",
            DisplayName = "Значение верхнего маркера-ориентира:",
            DefaultValue = string.Empty,
            Description = "Значение верхнего маркера-ориентира"
        };
        public static MPCOBoolProperty BottomOrientMarkerVisible = new MPCOBoolProperty
        {
            Name = "BottomOrientMarkerVisible",
            DisplayName = "Нижний маркер-ориентир:",
            DefaultValue = false,
            Description = "Видимость нижнего маркера-ориентира"
        };
        public static MPCOBoolProperty TopOrientMarkerVisible = new MPCOBoolProperty
        {
            Name = "TopOrientMarkerVisible",
            DisplayName = "Верхний маркер-ориентир:",
            DefaultValue = false,
            Description = "Видимость верхнего маркера-ориентира"
        };
        public static MPCOIntProperty OrientMarkerType = new MPCOIntProperty
        {
            Name = nameof(OrientMarkerType),
            DisplayName = "Тип маркера-ориентира:",
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 1,
            Description = "Тип маркера-ориентира"
        };

        #endregion
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
