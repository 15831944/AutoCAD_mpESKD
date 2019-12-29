namespace mpESKD.Base.Helpers
{
    using System;

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
