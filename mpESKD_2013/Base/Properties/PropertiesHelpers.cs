using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace mpESKD.Base.Properties
{
    /// <inheritdoc />
    /// <summary>
    /// Объект для сравнения значений типа Double
    /// </summary>
    public class DoubleEqComparer : IEqualityComparer<double>
    {
        readonly double _precission;

        /// <summary>
        /// Создание объекта с точностью сравнения 0,000001
        /// </summary>
        public DoubleEqComparer()
        {
            _precission = 0.000001;
        }

        /// <summary>
        /// Создание объекта с заданной точностью сравнения
        /// </summary>
        /// <param name="precission">Точность сравнения</param>
        public DoubleEqComparer(double precission)
        {
            _precission = precission;
        }

        /// <inheritdoc />
        /// <summary>
        /// Сравнение объектов типа Double
        /// </summary>
        /// <param name="x">Первый объект</param>
        /// <param name="y">Второй объект</param>
        /// <returns>
        /// True - объекты идентичны,
        /// false - объекты различаются
        /// </returns>
        public bool Equals(double x, double y)
        {
            return Math.Abs(x - y) <= _precission;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(double obj)
        {
            return obj.GetHashCode();
        }
    }

    public static class Parsers
    {
        /// <summary>Парсинг аннотативного масштаба из строки</summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static AnnotationScale AnnotationScaleFromString(string str)
        {
            var defaultScale = new AnnotationScale {Name = "1:1", DrawingUnits = 1.0, PaperUnits = 1.0};
            var splitted = str.Split(':');
            if (splitted.Length == 2)
            {
                var scale = new AnnotationScale
                {
                    Name = str,
                    PaperUnits = double.TryParse(splitted[0], out var d) ? d : 1.0,
                    DrawingUnits = double.TryParse(splitted[1], out d) ? d : 1.0
                };
                return scale;
            }
            return defaultScale;
        }
    }
}
