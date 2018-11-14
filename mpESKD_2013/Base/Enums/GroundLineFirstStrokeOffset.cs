namespace mpESKD.Base.Enums
{
    /// <summary>
    /// Отступ первого штриха в объекте GroundLine
    /// </summary>
    public enum GroundLineFirstStrokeOffset
    {
        /// <summary>
        /// По расстоянию между штрихами
        /// </summary>
        [EnumPropertyDisplayValueKeyAttribute("glfst1")]
        ByStrokeOffset,

        /// <summary>
        /// Половина расстояния между группами штрихов
        /// </summary>
        [EnumPropertyDisplayValueKeyAttribute("glfst2")]
        ByHalfSpace,

        /// <summary>
        /// Расстояние между группами штрихов
        /// </summary>
        [EnumPropertyDisplayValueKeyAttribute("glfst3")]
        BySpace
    }
}
