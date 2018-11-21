namespace mpESKD.Functions.mpSection
{
    using System.Collections.Generic;
    using Base;

    //todo translate
    public class SectionDescriptor : IIntellectualEntityDescriptor
    {
        private static SectionDescriptor _instance;

        public static SectionDescriptor Instance => _instance ?? (_instance = new SectionDescriptor());

        /// <inheritdoc />
        public string Name => "mpSection";

        /// <inheritdoc />
        public string LName => "";

        /// <inheritdoc />
        public string Description => "";

        /// <inheritdoc />
        public string FullDescription => "";

        /// <inheritdoc />
        public string ToolTipHelpImage => string.Empty;

        /// <inheritdoc />
        public List<string> SubFunctionsNames => new List<string>
        {
            "mpSectionSimply",
            "mpSectionFromPolyline"
        };

        /// <inheritdoc />
        public List<string> SubFunctionsLNames => new List<string>
        {
            "",
            ""
        };

        /// <inheritdoc />
        public List<string> SubDescriptions => new List<string>
        {
            "",
            ""
        };

        /// <inheritdoc />
        public List<string> SubFullDescriptions => new List<string>
        {
            string.Empty,
            string.Empty
        };

        /// <inheritdoc />
        public List<string> SubHelpImages => new List<string>
        {
            string.Empty,
            string.Empty
        };
    }
}
