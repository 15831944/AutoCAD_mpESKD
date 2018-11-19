namespace mpESKD.Functions.mpSection
{
    using System.Collections.Generic;
    using ModPlusAPI;

    //todo translate
    public class SectionInterface
    {
        public static string Name => "mpSection";

        public static string LName => "";

        public static string Description => "";

        public static string FullDescription => "";

        public static string ToolTipHelpImage => string.Empty;

        public static List<string> SubFunctionsNames => new List<string>
        {
            "mpSectionSimply",
            "mpSectionFromPolyline"
        };

        public static List<string> SubFunctionsLNames => new List<string>
        {
            "",
            ""
        };

        public static List<string> SubDescriptions => new List<string>
        {
            "",
            ""
        };

        public static List<string> SubFullDescriptions => new List<string>
        {
            string.Empty,
            string.Empty
        };

        public static List<string> SubHelpImages => new List<string>
        {
            string.Empty,
            string.Empty
        };
    }
}
