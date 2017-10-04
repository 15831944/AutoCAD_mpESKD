using System;
using System.Collections.Generic;
using ModPlusAPI.Interfaces;

namespace mpESKD
{
    public class Interface : IModPlusFunctionInterface
    {
        public SupportedProduct SupportedProduct => SupportedProduct.AutoCAD;
        public string Name => "mpESKD";
        public string AvailProductExternalVersion => "2014";
        public string FullClassName => string.Empty;
        public string AppFullClassName => string.Empty;
        public Guid AddInId => Guid.Empty;
        public string ClassName => string.Empty;
        public string LName => "ModPlus ЕСКД";
        public string Description => "Оформление чертежей по нормам ЕСКД";
        public string Author => "Пекшев Александр aka Modis";
        public string Price => "0";
        public bool CanAddToRibbon => false;
        public string FullDescription => "Сборник функций, создающий интеллектуальные объекты для оформления чертежей по нормам ЕСКД";
        public string ToolTipHelpImage => string.Empty;
        public List<string> SubFunctionsNames => new List<string>();
        public List<string> SubFunctionsLames => new List<string>();
        public List<string> SubDescriptions => new List<string>();
        public List<string> SubFullDescriptions => new List<string>();
        public List<string> SubHelpImages => new List<string>();
        public List<string> SubClassNames => new List<string>();
    }
    public class MpVersionData
    {
        public const string CurCadVers = "2014";
        public const string CurCadInternalVersion = "19.1";
    }
}
