namespace mpESKD.Functions.mpGroundLine.Properties
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Base.Enums;
    using Base.Properties;
    using ModPlusAPI;

    public static class GroundLineProperties
    {
        ////TODO Localization!

        /// <summary>Поле, описывающее свойство "Масштаб"</summary>
        public static MPCOTypeProperty<AnnotationScale> Scale = new MPCOTypeProperty<AnnotationScale>
        {
            Name = nameof(Scale),
            DisplayName = Language.GetItem(MainFunction.LangItem, "p5"), // Масштаб:
            DefaultValue = new AnnotationScale { Name = "1:1", DrawingUnits = 1.0, PaperUnits = 1.0 },
            Description = Language.GetItem(MainFunction.LangItem, "d5")
        };
        public static MPCOStringProperty LineType = new MPCOStringProperty
        {
            Name = nameof(LineType),
            DisplayName = "Тип линии:",
            DefaultValue = "Continuous",
            Description = Language.GetItem(MainFunction.LangItem, "d19")
        };
        /// <summary>Поле, описывающее свойство "Масштаб типа линии"</summary>
        public static MPCODoubleProperty LineTypeScale = new MPCODoubleProperty
        {
            Name = nameof(LineTypeScale),
            DisplayName = Language.GetItem(MainFunction.LangItem, "p6"), // Масштаб типа линий
            DefaultValue = 1.0,
            Minimum = 0,
            Maximum = double.MaxValue,
            Description = Language.GetItem(MainFunction.LangItem, "d6")
        };
        public static MPCOStringProperty LayerName = new MPCOStringProperty
        {
            Name = nameof(LayerName),
            DisplayName = Language.GetItem(MainFunction.LangItem, "p7"), // Слой
            DefaultValue = Language.GetItem(MainFunction.LangItem, "defl"), // По умолчанию
            Description = Language.GetItem(MainFunction.LangItem, "d7") // Слой примитива
        };

        public static MPCOTypeProperty<GroundLineFirstStrokeOffset> FirstStrokeOffset = new MPCOTypeProperty<GroundLineFirstStrokeOffset>
        {
            Name = nameof(FirstStrokeOffset),
            DisplayName = "Отступ первого штриха",
            Description = "Отступ первого штриха в каждом сегменте линии грунта",
            DefaultValue = GroundLineFirstStrokeOffset.ByHalfSpace
        };

        /// <summary>
        /// Длина штриха
        /// </summary>
        public static MPCOIntProperty StrokeLength = new MPCOIntProperty
        {
            Name = nameof(StrokeLength),
            DisplayName = "Длина штриха",
            Description = "Длина штриха",
            DefaultValue = 8,
            Minimum = 1,
            Maximum = 10
        };

        /// <summary>
        /// Расстояние между штрихами
        /// </summary>
        public static MPCOIntProperty StrokeOffset = new MPCOIntProperty
        {
            Name = nameof(StrokeOffset),
            DisplayName = "Расстояние между штрихами",
            Description = "Расстояние между штрихами",
            DefaultValue = 4,
            Minimum = 1,
            Maximum = 10
        };

        /// <summary>
        /// Угол наклона штриха
        /// </summary>
        public static MPCOIntProperty StrokeAngle = new MPCOIntProperty
        {
            Name = nameof(StrokeAngle),
            DisplayName = "Угол наклона штриха",
            Description = "Угол наклона штриха",
            DefaultValue = 60,
            Minimum = 30,
            Maximum = 90
        };

        /// <summary>
        /// Отступ группы штрихов
        /// </summary>
        public static MPCOIntProperty Space = new MPCOIntProperty
        {
            Name = nameof(Space),
            DisplayName = "Отступ группы штрихов",
            Description = "Отступ группы штрихов",
            DefaultValue = 10,
            Minimum = 1,
            Maximum = 20
        };
    }

    //TODO Localization
    public static class GroundLinePropertiesHelpers
    {
        public static List<string> FirstStrokeOffsetNames = new List<string>
        {
            "Расстояние между штрихами",
            "Половина расстояния между группами штрихов",
            "Расстояние между группами штрихов"
        };

        public static GroundLineFirstStrokeOffset GetFirstStrokeOffsetByLocalName(string local)
        {
            if (local == FirstStrokeOffsetNames[0]) return GroundLineFirstStrokeOffset.ByStrokeOffset;
            if (local == FirstStrokeOffsetNames[2]) return GroundLineFirstStrokeOffset.BySpace;
            return GroundLineFirstStrokeOffset.ByHalfSpace;
        }

        public static string GetLocalFirstStrokeOffsetName(GroundLineFirstStrokeOffset firstStrokeOffset)
        {
            if (firstStrokeOffset == GroundLineFirstStrokeOffset.ByStrokeOffset)
                return FirstStrokeOffsetNames[0];
            if (firstStrokeOffset == GroundLineFirstStrokeOffset.BySpace)
                return FirstStrokeOffsetNames[2];
            return FirstStrokeOffsetNames[1];
        }

        public static GroundLineFirstStrokeOffset GetFirstStrokeOffsetFromString(string str)
        {
            if (str == "ByStrokeOffset") return GroundLineFirstStrokeOffset.ByStrokeOffset;
            if (str == "BySpace") return GroundLineFirstStrokeOffset.BySpace;
            return GroundLineFirstStrokeOffset.ByHalfSpace;
        }
    }
}
