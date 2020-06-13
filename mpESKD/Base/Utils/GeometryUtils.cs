﻿namespace mpESKD.Base.Utils
{
    using Autodesk.AutoCAD.Geometry;

    /// <summary>
    /// Утилиты работы с геометрией
    /// </summary>
    public static class GeometryUtils
    {
        /// <summary>
        /// Возвращает среднюю точку между двумя указанными 3d точками
        /// </summary>
        /// <param name="firstPoint">Первая 3d точка</param>
        /// <param name="secondPoint">Вторая 3d точка</param>
        public static Point3d GetMiddlePoint3d(Point3d firstPoint, Point3d secondPoint)
        {
            return new Point3d(
                (firstPoint.X + secondPoint.X) / 2,
                (firstPoint.Y + secondPoint.Y) / 2,
                (firstPoint.Z + secondPoint.Z) / 2);
        }

        /// <summary>
        /// Возвращает среднюю точку между двумя указанными 2d точками
        /// </summary>
        /// <param name="firstPoint">Первая 2d точка</param>
        /// <param name="secondPoint">Вторая 2d точка</param>
        public static Point2d GetMiddlePoint2d(Point2d firstPoint, Point2d secondPoint)
        {
            return new Point2d(
                (firstPoint.X + secondPoint.X) / 2,
                (firstPoint.Y + secondPoint.Y) / 2);
        }

        /// <summary>
        /// Представляет точку в виде строки
        /// </summary>
        /// <param name="point3d">Точка</param>
        public static string AsString(this Point3d point3d)
        {
            return $"{point3d.X}${point3d.Y}${point3d.Z}";
        }

        /// <summary>
        /// Представляет вектор в виде строки
        /// </summary>
        /// <param name="vector3d">Вектор</param>
        public static string AsString(this Vector3d vector3d)
        {
            return $"{vector3d.X}${vector3d.Y}${vector3d.Z}";
        }

        /// <summary>
        /// Преобразует строку в 3d точку
        /// </summary>
        /// <param name="str">Строка</param>
        public static Point3d ParseToPoint3d(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                var splitted = str.Split('$');
                if (splitted.Length == 3)
                {
                    return new Point3d(
                        double.Parse(splitted[0]),
                        double.Parse(splitted[1]),
                        double.Parse(splitted[2]));
                }
            }

            return Point3d.Origin;
        }

        /// <summary>
        /// Конвертация Point3d в Point2d путем отбрасывания Z
        /// </summary>
        /// <param name="point3d">3d точка</param>
        public static Point2d ConvertPoint3dToPoint2d(this Point3d point3d)
        {
            return new Point2d(point3d.X, point3d.Y);
        }
    }
}
