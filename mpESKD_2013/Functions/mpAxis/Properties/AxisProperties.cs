using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using mpESKD.Base.Properties;
using ModPlusAPI;

namespace mpESKD.Functions.mpAxis.Properties
{
    public static class AxisProperties
    {
        /// <summary>Поле, описывающее свойство "Тип линии обрыва"</summary>
        public static MPCOTypeProperty<AxisMarkersPosition> MarkersPosition = new MPCOTypeProperty<AxisMarkersPosition>
        {
            Name = "MarkersPosition",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p8"), // Позиция маркеров:
            DefaultValue = AxisMarkersPosition.Bottom,
            Description = Language.GetItem(MainFunction.LangItem, "d8")
        };
        /// <summary>Поле, описывающее свойство "Излом"</summary>
        public static MPCOIntProperty Fracture = new MPCOIntProperty
        {
            Name = "Fracture",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p9"), // Излом:
            DefaultValue = 10,
            Minimum = 1,
            Maximum = 20,
            Description = Language.GetItem(MainFunction.LangItem, "d9")
        };
        /// <summary>Поле, описывающее свойство "Диаметр маркеров"</summary>
        public static MPCOIntProperty MarkersDiameter = new MPCOIntProperty
        {
            Name = "MarkersDiameter",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p10"), // Диаметр маркеров:
            DefaultValue = 10,
            Minimum = 6,
            Maximum = 12,
            Description = Language.GetItem(MainFunction.LangItem, "d10")
        };
        /// <summary>Поле, описывающее свойство "Количество маркеров"</summary>
        public static MPCOIntProperty MarkersCount = new MPCOIntProperty
        {
            Name = "MarkersCount",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p11"), // Количество маркеров:
            DefaultValue = 1,
            Minimum = 1,
            Maximum = 3,
            Description = Language.GetItem(MainFunction.LangItem, "d11")
        };
        public static MPCOIntProperty FirstMarkerType = new MPCOIntProperty
        {
            Name = "FirstMarkerType",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p12"), // Тип первого маркера:
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 1,
            Description = Language.GetItem(MainFunction.LangItem, "d12")
        };
        public static MPCOIntProperty SecondMarkerType = new MPCOIntProperty
        {
            Name = "SecondMarkerType",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p13"), // Тип второго маркера:
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 1,
            Description = Language.GetItem(MainFunction.LangItem, "d13")
        };
        public static MPCOIntProperty ThirdMarkerType = new MPCOIntProperty
        {
            Name = "ThirdMarkerType",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p14"), // Тип третьего маркера:
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 1,
            Description = Language.GetItem(MainFunction.LangItem, "d14")
        };
        /// <summary>Поле, описывающее свойство "Нижний отступ излома"</summary>
        public static MPCOIntProperty BottomFractureOffset = new MPCOIntProperty
        {
            Name = "BottomFractureOffset",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p15"), // Нижний отступ излома:
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 30,
            Description = Language.GetItem(MainFunction.LangItem, "d15")
        };
        /// <summary>Поле, описывающее свойство "Верхний отступ излома"</summary>
        public static MPCOIntProperty TopFractureOffset = new MPCOIntProperty
        {
            Name = "TopFractureOffset",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p16"), // Верхний отступ излома:
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 30,
            Description = Language.GetItem(MainFunction.LangItem, "d16")
        };
        public static MPCOStringProperty TextStyle = new MPCOStringProperty
        {
            Name = "TextStyle",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p17"), // Текстовый стиль:
            DefaultValue = "Standard",
            Description = Language.GetItem(MainFunction.LangItem, "d17")
        };
        public static MPCODoubleProperty TextHeight = new MPCODoubleProperty
        {
            Name = "TextHeight",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p18"), // Высота текста:
            DefaultValue = 3.5,
            Minimum = 0.000000001,
            Maximum = double.MaxValue,
            Description = Language.GetItem(MainFunction.LangItem, "d18")
        };
        // General
        /// <summary>Поле, описывающее свойство "Масштаб"</summary>
        public static MPCOTypeProperty<AnnotationScale> Scale = new MPCOTypeProperty<AnnotationScale>
        {
            Name = "Scale",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p5"), // Масштаб:
            DefaultValue = new AnnotationScale { Name = "1:1", DrawingUnits = 1.0, PaperUnits = 1.0 },
            Description = Language.GetItem(MainFunction.LangItem, "d5-1")
        };
        /// <summary>Поле, описывающее свойство "Масштаб типа линии"</summary>
        public static MPCODoubleProperty LineTypeScale = new MPCODoubleProperty
        {
            Name = "LineTypeScale",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p6"), // Масштаб типа линий:
            DefaultValue = 10.0,
            Minimum = 0,
            Maximum = double.MaxValue,
            Description = Language.GetItem(MainFunction.LangItem, "d6")
        };
        public static MPCOStringProperty LineType = new MPCOStringProperty
        {
            Name = "LineType",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p19"), // Тип линии:
            DefaultValue = "осевая",
            Description = Language.GetItem(MainFunction.LangItem, "d19")
        };
        /// <summary>Поле, описывающее слой</summary>
        public static MPCOStringProperty LayerName = new MPCOStringProperty
        {
            Name = "LayerName",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p7"), // Слой:
            DefaultValue = Language.GetItem(MainFunction.LangItem, "defl"), // "По умолчанию"
            Description = Language.GetItem(MainFunction.LangItem, "d7")
        };

        #region text
        // first
        public static MPCOStringProperty FirstTextPrefix = new MPCOStringProperty
        {
            Name = "FirstTextPrefix",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p20"), // Префикс первого значения:
            DefaultValue = string.Empty,
            Description = Language.GetItem(MainFunction.LangItem, "d20")
        };
        public static MPCOStringProperty FirstTextSuffix = new MPCOStringProperty
        {
            Name = "FirstTextSuffix",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p21"), // Суффикс первого значения:
            DefaultValue = string.Empty,
            Description = Language.GetItem(MainFunction.LangItem, "d21")
        };
        public static MPCOStringProperty FirstText = new MPCOStringProperty
        {
            Name = "FirstText",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p22"), // Первое значение:
            DefaultValue = string.Empty,
            Description = Language.GetItem(MainFunction.LangItem, "d22")
        };
        // second
        public static MPCOStringProperty SecondTextPrefix = new MPCOStringProperty
        {
            Name = "SecondTextPrefix",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p23"), // Префикс второго значения:
            DefaultValue = string.Empty,
            Description = Language.GetItem(MainFunction.LangItem, "d23")
        };
        public static MPCOStringProperty SecondTextSuffix = new MPCOStringProperty
        {
            Name = "SecondTextSuffix",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p24"), // Суффикс второго значения:
            DefaultValue = string.Empty,
            Description = Language.GetItem(MainFunction.LangItem, "d24")
        };
        public static MPCOStringProperty SecondText = new MPCOStringProperty
        {
            Name = "SecondText",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p25"), // Второе значение:
            DefaultValue = string.Empty,
            Description = Language.GetItem(MainFunction.LangItem, "d25")
        };
        // third
        public static MPCOStringProperty ThirdTextPrefix = new MPCOStringProperty
        {
            Name = "ThirdTextPrefix",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p26"), // Префикс третьего значения:
            DefaultValue = string.Empty,
            Description = Language.GetItem(MainFunction.LangItem, "d26")
        };
        public static MPCOStringProperty ThirdTextSuffix = new MPCOStringProperty
        {
            Name = "ThirdTextSuffix",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p27"), // Суффикс третьего значения:
            DefaultValue = string.Empty,
            Description = Language.GetItem(MainFunction.LangItem, "d27")
        };
        public static MPCOStringProperty ThirdText = new MPCOStringProperty
        {
            Name = "ThirdText",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p28"), // Третье значение:
            DefaultValue = string.Empty,
            Description = Language.GetItem(MainFunction.LangItem, "d28")
        };
        // Orient markers
        public static MPCOIntProperty ArrowsSize = new MPCOIntProperty
        {
            Name = "ArrowsSize",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p29"), // Размер стрелок:
            Minimum = 0,
            Maximum = 10,
            DefaultValue = 3,
            Description = Language.GetItem(MainFunction.LangItem, "d29")
        };
        public static MPCOStringProperty BottomOrientText = new MPCOStringProperty
        {
            Name = "BottomOrientText",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p30"), // Значение нижнего маркера-ориентира:
            DefaultValue = string.Empty,
            Description = Language.GetItem(MainFunction.LangItem, "d30")
        };
        public static MPCOStringProperty TopOrientText = new MPCOStringProperty
        {
            Name = "TopOrientText",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p31"), // Значение верхнего маркера-ориентира:
            DefaultValue = string.Empty,
            Description = Language.GetItem(MainFunction.LangItem, "d31")
        };
        public static MPCOBoolProperty BottomOrientMarkerVisible = new MPCOBoolProperty
        {
            Name = "BottomOrientMarkerVisible",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p32"), // Нижний маркер-ориентир:
            DefaultValue = false,
            Description = Language.GetItem(MainFunction.LangItem, "d32")
        };
        public static MPCOBoolProperty TopOrientMarkerVisible = new MPCOBoolProperty
        {
            Name = "TopOrientMarkerVisible",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p33"), // Верхний маркер-ориентир:
            DefaultValue = false,
            Description = Language.GetItem(MainFunction.LangItem, "d33")
        };
        public static MPCOIntProperty OrientMarkerType = new MPCOIntProperty
        {
            Name = "OrientMarkerType",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p34"), // Тип маркера-ориентира:
            DefaultValue = 0,
            Minimum = 0,
            Maximum = 1,
            Description = Language.GetItem(MainFunction.LangItem, "d34")
        };

        #endregion
    }

    public static class AxisPropertiesHelpers
    {
        public static List<string> AxisMarkersTypeLocalNames = new List<string>
        {
            Language.GetItem(MainFunction.LangItem, "amt1"),// "С двух сторон",
            Language.GetItem(MainFunction.LangItem, "amt2"),// "Сверху",
            Language.GetItem(MainFunction.LangItem, "amt3")//"Снизу"
        };

        #region Methods

        public static AxisMarkersPosition GetAxisMarkersPositionByLocalName(string local)
        {
            if (local == Language.GetItem(MainFunction.LangItem, "amt1")) return AxisMarkersPosition.Both;
            if (local == Language.GetItem(MainFunction.LangItem, "amt2")) return AxisMarkersPosition.Top;
            if (local == Language.GetItem(MainFunction.LangItem, "amt3")) return AxisMarkersPosition.Bottom;
            return AxisMarkersPosition.Bottom;
        }

        public static string GetLocalAxisMarkersPositionName(AxisMarkersPosition axisMarkersPosition)
        {
            if (axisMarkersPosition == AxisMarkersPosition.Both) return Language.GetItem(MainFunction.LangItem, "amt1");
            if (axisMarkersPosition == AxisMarkersPosition.Top) return Language.GetItem(MainFunction.LangItem, "amt2");
            if (axisMarkersPosition == AxisMarkersPosition.Bottom) return Language.GetItem(MainFunction.LangItem, "amt3");
            return Language.GetItem(MainFunction.LangItem, "amt1");
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
