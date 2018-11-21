namespace mpESKD.Functions.mpGroundLine
{
    using System.Collections.Generic;
    using ModPlusAPI;

    public class GroundLineDescriptor
    {
        public static string Name => "mpGroundLine";

        public static string LName => Language.GetItem(MainFunction.LangItem, "h73"); // "Линия грунта";

        public static string Description => Language.GetItem(MainFunction.LangItem, "h74"); // "Линия обозначения грунта";

        public static string FullDescription => Language.GetItem(MainFunction.LangItem, "h75"); // "Создание интеллектуального объекта на основе анонимного блока, описывающего линию грунта";

        public static string ToolTipHelpImage => string.Empty;

        public static List<string> SubFunctionsNames => new List<string>
        {
            "mpGroundLineFromPolyline"
        };

        public static List<string> SubFunctionsLNames => new List<string>
        {
            Language.GetItem(MainFunction.LangItem, "h76") // "Линия грунта из полилинии"
        };

        public static List<string> SubDescriptions => new List<string>
        {
            Language.GetItem(MainFunction.LangItem, "h77") // "Конвертирование выбранной полилинии в линию обозначения грунта"
        };

        public static List<string> SubFullDescriptions => new List<string>
        {
            string.Empty
        };

        public static List<string> SubHelpImages => new List<string>
        {
            string.Empty
        };
    }
}
