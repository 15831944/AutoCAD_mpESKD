namespace mpESKD.Functions.mpGroundLine
{
    using System.Collections.Generic;

    public class GroundLineInterface
    {
        //// TODO Translate
        
        public static string Name => "mpGroundLine";

        public static string LName => "Линия грунта";

        public static string Description => "Линия обозначения грунта";

        public static string FullDescription => "Создание интеллектуального объекта на основе анонимного блока, описывающего линию грунта";

        public static string ToolTipHelpImage => string.Empty;

        public static List<string> SubFunctionsNames => new List<string>
        {
            "mpGroundLineFromPolyline"
        };

        public static List<string> SubFunctionsLNames => new List<string>
        {
            "Линия грунта из полилинии"
        };

        public static List<string> SubDescriptions => new List<string>
        {
            "Конвертирование выбранной полилинии в линию обозначения грунта"
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
