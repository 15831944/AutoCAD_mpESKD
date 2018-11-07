namespace mpESKD.Functions.mpGroundLine.Properties
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Base.Properties;
    using ModPlusAPI;

    public static class GroundLineProperties
    {
        /// <summary>Поле, описывающее свойство "Масштаб"</summary>
        public static MPCOTypeProperty<AnnotationScale> Scale = new MPCOTypeProperty<AnnotationScale>
        {
            Name = "Scale",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p5"), // Масштаб:
            DefaultValue = new AnnotationScale { Name = "1:1", DrawingUnits = 1.0, PaperUnits = 1.0 },
            Description = Language.GetItem(MainFunction.LangItem, "d5")
        };
        public static MPCOStringProperty LineType = new MPCOStringProperty
        {
            Name = "LineType",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p19"), // Тип линии:
            DefaultValue = "Continuous",
            Description = Language.GetItem(MainFunction.LangItem, "d19")
        };
        /// <summary>Поле, описывающее свойство "Масштаб типа линии"</summary>
        public static MPCODoubleProperty LineTypeScale = new MPCODoubleProperty
        {
            Name = "LineTypeScale",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p6"), // Масштаб типа линий
            DefaultValue = 1.0, Minimum = 0, Maximum = double.MaxValue,
            Description = Language.GetItem(MainFunction.LangItem, "d6")
        };
        public static MPCOStringProperty LayerName = new MPCOStringProperty
        {
            Name = "LayerName",
            DisplayName = Language.GetItem(MainFunction.LangItem, "p7"), // Слой
            DefaultValue = Language.GetItem(MainFunction.LangItem, "defl"), // По умолчанию
            Description = Language.GetItem(MainFunction.LangItem, "d7") // Слой примитива
        };
    }
}
