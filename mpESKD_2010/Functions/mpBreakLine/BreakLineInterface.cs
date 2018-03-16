using System.Collections.Generic;
using ModPlusAPI;

namespace mpESKD.Functions.mpBreakLine
{
    public static class BreakLineInterface 
    {
        private const string LangItem = "mpESKD";
        
        public static string Name => "mpBreakLine";
        public static string LName => Language.GetItem(LangItem, "h48"); // "Линия обрыва";
        public static string Description => Language.GetItem(LangItem, "h56"); // "Отрисовка линии обрыва по ГОСТ 2.303-68";
        public static string FullDescription => Language.GetItem(LangItem, "h57"); // "Создание интеллектуального объекта на основе анонимного блока, описывающего линию обрыва по ГОСТ 2.303-68, путем указания двух точек";
        public static string ToolTipHelpImage => "Linear.png";
        public static List<string> SubFunctionsNames => new List<string>
        {
            "mpBreakLineCurve", "mpBreakLineCylinder"
        };
        public static List<string> SubFunctionsLNames => new List<string>
        {
            Language.GetItem(LangItem, "h58"), // "Криволинейный обрыв",
            Language.GetItem(LangItem, "h59") // "Цилиндрический обрыв"
        };

        public static List<string> SubDescriptions => new List<string>
        {
            Language.GetItem(LangItem, "h60"), // "Отрисовка криволинейного обрыва",
            Language.GetItem(LangItem, "h61") //"Отрисовка цилиндрического обрыва"
        };

        public static List<string> SubFullDescriptions => new List<string>
        {
            Language.GetItem(LangItem, "h62"), // "Создание интеллектуального объекта на основе анонимного блока, описывающего криволинейный обрыв, путем указания двух точек",
            Language.GetItem(LangItem, "h63") // "Создание интеллектуального объекта на основе анонимного блока, описывающего цилиндрический обрыв, путем указания двух точек"
        };

        public static List<string> SubHelpImages => new List<string> { "Curvilinear.png", "Cylindrical.png" };
    }
}
