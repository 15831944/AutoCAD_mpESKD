using System.Collections.Generic;

namespace mpESKD.Functions.mpBreakLine
{
    public static class BreakLineInterface 
    {
        public static string Name => "mpBreakLine";
        public static string LName => "Линия обрыва";
        public static string Description => "Отрисовка линии обрыва по ГОСТ 2.303-68";
        public static string FullDescription => "Создание интеллектуального объекта на основе анонимного блока, описывающего линию обрыва по ГОСТ 2.303-68, путем указания двух точек";
        public static string ToolTipHelpImage => "Linear.png";
        public static List<string> SubFunctionsNames => new List<string>
        {
            "mpBreakLineCurve","mpBreakLineCylinder"
        };
        public static List<string> SubFunctionsLNames => new List<string>
        {
            "Криволинейный обрыв", "Цилиндрический обрыв"
        };

        public static List<string> SubDescriptions => new List<string>
        {
            "Отрисовка криволинейного обрыва", "Отрисовка цилиндрического обрыва"
        };

        public static List<string> SubFullDescriptions => new List<string>
        {
            "Создание интеллектуального объекта на основе анонимного блока, описывающего криволинейный обрыв, путем указания двух точек",
            "Создание интеллектуального объекта на основе анонимного блока, описывающего цилиндрический обрыв, путем указания двух точек"
        };

        public static List<string> SubHelpImages => new List<string> { "Curvilinear.png", "Cylindrical.png" };
    }
}
