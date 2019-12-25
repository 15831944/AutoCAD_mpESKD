﻿namespace mpESKD
{
    using System;
    using System.Collections.Generic;
    using ModPlusAPI.Interfaces;

    public class ModPlusConnector : IModPlusFunctionInterface
    {
        private static ModPlusConnector _instance;

        public static ModPlusConnector Instance => _instance ?? (_instance = new ModPlusConnector());

        public SupportedProduct SupportedProduct => SupportedProduct.AutoCAD;

        public string Name => "mpESKD";

#if A2013
        public string AvailProductExternalVersion => "2013";
#elif A2014
        public string AvailProductExternalVersion => "2014";
#elif A2015
        public string AvailProductExternalVersion => "2015";
#elif A2016
        public string AvailProductExternalVersion => "2016";
#elif A2017
        public string AvailProductExternalVersion => "2017";
#elif A2018
        public string AvailProductExternalVersion => "2018";
#elif A2019
        public string AvailProductExternalVersion => "2019";
#elif A2020
        public string AvailProductExternalVersion => "2020";
#endif

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
#if A2013
        public const string CurCadVers = "2013";
        public const string CurCadInternalVersion = "19.0";
#elif A2014
        public const string CurCadVers = "2014";
        public const string CurCadInternalVersion = "19.1";
#elif A2015
        public const string CurCadVers = "2015";
        public const string CurCadInternalVersion = "20.0";
#elif A2016
        public const string CurCadVers = "2016";
        public const string CurCadInternalVersion = "20.1";
#elif A2017
        public const string CurCadVers = "2017";
        public const string CurCadInternalVersion = "21.0";
#elif A2018
        public const string CurCadVers = "2018";
        public const string CurCadInternalVersion = "22.0";
#elif A2019
        public const string CurCadVers = "2019";
        public const string CurCadInternalVersion = "23.0";
#endif
    }
}
