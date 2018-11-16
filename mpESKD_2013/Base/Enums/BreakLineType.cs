namespace mpESKD.Base.Enums
{
    /// <summary>
    /// Тип линии: линейный, криволинейный, цилиндрический
    /// </summary>
    public enum BreakLineType
    {
        /// <summary>
        /// Линейный
        /// </summary>
        [EnumPropertyDisplayValueKeyAttribute("blt1")]
        Linear = 1,

        /// <summary>
        /// Криволинейный
        /// </summary>
        [EnumPropertyDisplayValueKeyAttribute("blt2")]
        Curvilinear = 2,

        /// <summary>
        /// Цилиндрический
        /// </summary>
        [EnumPropertyDisplayValueKeyAttribute("blt3")]
        Cylindrical = 3
    }
}