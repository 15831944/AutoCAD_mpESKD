using System.Collections.Generic;
using ModPlusAPI;

namespace mpESKD.Functions.mpAxis
{
    public static class AxisInterface
    {
        private const string LangItem = "mpESKD";

        public static string Name => "mpAxis";
        public static string LName => Language.GetItem(LangItem, "h41"); // "Прямая ось";
        public static string Description => Language.GetItem(LangItem, "h65");// "Отрисовка прямой оси по ГОСТ 21.101-97";
        public static string FullDescription => Language.GetItem(LangItem, "h66");//"Создание интеллектуального объекта на основе анонимного блока, описывающего прямую ось по ГОСТ 21.101-97, путем указания двух точек";
        public static string ToolTipHelpImage => string.Empty;
        public static List<string> SubFunctionsNames => new List<string>();
        public static List<string> SubFunctionsLNames => new List<string>();

        public static List<string> SubDescriptions => new List<string>();

        public static List<string> SubFullDescriptions => new List<string>();

        public static List<string> SubHelpImages => new List<string>();
    }
}
