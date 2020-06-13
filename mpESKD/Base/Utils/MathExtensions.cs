namespace mpESKD.Base.Utils
{
    using System;

    /// <summary>
    /// Математические вспомогательные методы расширения
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Перевод градусов в радианы
        /// </summary>
        /// <param name="degree">Угол в градусах</param>
        public static double DegreeToRadian(this double degree)
        {
            return degree * Math.PI / 180;
        }

        /// <summary>
        /// Перевод градусов в радианы
        /// </summary>
        /// <param name="degree">Угол в градусах</param>
        public static double DegreeToRadian(this int degree)
        {
            return degree * Math.PI / 180;
        }
    }
}
