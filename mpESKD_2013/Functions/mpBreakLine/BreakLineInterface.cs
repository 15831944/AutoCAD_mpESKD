namespace mpESKD.Functions.mpBreakLine
{
    using System.Collections.Generic;
    using ModPlusAPI;

    public static class BreakLineInterface 
    {
        public static string Name => "mpBreakLine";
        
        public static string LName => Language.GetItem(MainFunction.LangItem, "h48"); // "Линия обрыва";
        
        public static string Description => Language.GetItem(MainFunction.LangItem, "h56"); // "Отрисовка линии обрыва по ГОСТ 2.303-68";
        
        public static string FullDescription => Language.GetItem(MainFunction.LangItem, "h57"); // "Создание интеллектуального объекта на основе анонимного блока, описывающего линию обрыва по ГОСТ 2.303-68, путем указания двух точек";
        
        public static string ToolTipHelpImage => "Linear.png";
        
        public static List<string> SubFunctionsNames => new List<string>
        {
            "mpBreakLineCurve", "mpBreakLineCylinder"
        };
        
        public static List<string> SubFunctionsLNames => new List<string>
        {
            Language.GetItem(MainFunction.LangItem, "h58"), // "Криволинейный обрыв",
            Language.GetItem(MainFunction.LangItem, "h59") // "Цилиндрический обрыв"
        };

        public static List<string> SubDescriptions => new List<string>
        {
            Language.GetItem(MainFunction.LangItem, "h60"), // "Отрисовка криволинейного обрыва",
            Language.GetItem(MainFunction.LangItem, "h61") //"Отрисовка цилиндрического обрыва"
        };

        public static List<string> SubFullDescriptions => new List<string>
        {
            Language.GetItem(MainFunction.LangItem, "h62"), // "Создание интеллектуального объекта на основе анонимного блока, описывающего криволинейный обрыв, путем указания двух точек",
            Language.GetItem(MainFunction.LangItem, "h63") // "Создание интеллектуального объекта на основе анонимного блока, описывающего цилиндрический обрыв, путем указания двух точек"
        };

        public static List<string> SubHelpImages => new List<string> { "Curvilinear.png", "Cylindrical.png" };
    }
}
