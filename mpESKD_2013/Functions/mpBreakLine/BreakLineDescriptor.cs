namespace mpESKD.Functions.mpBreakLine
{
    using System.Collections.Generic;
    using Base;
    using ModPlusAPI;

    public class BreakLineDescriptor : IIntellectualEntityDescriptor
    {
        private static BreakLineDescriptor _instance;

        public static BreakLineDescriptor Instance => _instance ?? (_instance = new BreakLineDescriptor());

        /// <inheritdoc />
        public string Name => "mpBreakLine";
        
        /// <inheritdoc />
        // "Линия обрыва"
        public string LName => Language.GetItem(MainFunction.LangItem, "h48"); 
        
        /// <inheritdoc />
        // "Отрисовка линии обрыва по ГОСТ 2.303-68"
        public string Description => Language.GetItem(MainFunction.LangItem, "h56"); 
        
        /// <inheritdoc />
        // "Создание интеллектуального объекта на основе анонимного блока, описывающего линию обрыва по ГОСТ 2.303-68, путем указания двух точек"
        public string FullDescription => Language.GetItem(MainFunction.LangItem, "h57");
        
        /// <inheritdoc />
        public string ToolTipHelpImage => "Linear.png";
        
        /// <inheritdoc />
        public List<string> SubFunctionsNames => new List<string>
        {
            "mpBreakLineCurve", "mpBreakLineCylinder"
        };
        
        /// <inheritdoc />
        public List<string> SubFunctionsLNames => new List<string>
        {
            // "Криволинейный обрыв"
            Language.GetItem(MainFunction.LangItem, "h58"),
            // "Цилиндрический обрыв"
            Language.GetItem(MainFunction.LangItem, "h59") 
        };

        /// <inheritdoc />
        public List<string> SubDescriptions => new List<string>
        {
            // "Отрисовка криволинейного обрыва"
            Language.GetItem(MainFunction.LangItem, "h60"),
            //"Отрисовка цилиндрического обрыва"
            Language.GetItem(MainFunction.LangItem, "h61") 
        };

        /// <inheritdoc />
        public List<string> SubFullDescriptions => new List<string>
        {
            // "Создание интеллектуального объекта на основе анонимного блока, описывающего криволинейный обрыв, путем указания двух точек"
            Language.GetItem(MainFunction.LangItem, "h62"),
            // "Создание интеллектуального объекта на основе анонимного блока, описывающего цилиндрический обрыв, путем указания двух точек"
            Language.GetItem(MainFunction.LangItem, "h63") 
        };

        /// <inheritdoc />
        public List<string> SubHelpImages => new List<string> { "Curvilinear.png", "Cylindrical.png" };
    }
}
