// ReSharper disable InconsistentNaming
namespace mpESKD.Base.Helpers
{
    using Autodesk.AutoCAD.Geometry;

    public static class GeometryHelpers
    {
        public static Point3d GetMiddlePoint3d(Point3d firstPoint, Point3d secondPoint)
        {
            return new Point3d(
                (firstPoint.X + secondPoint.X) / 2,
                (firstPoint.Y + secondPoint.Y) / 2,
                (firstPoint.Z + secondPoint.Z) / 2);
        }

        public static Point2d GetMiddlePoint2d(Point2d firstPoint, Point2d secondPoint)
        {
            return new Point2d(
                (firstPoint.X + secondPoint.X) / 2,
                (firstPoint.Y + secondPoint.Y) / 2);
        }

        public static string AsString(this Point3d point)
        {
            return $"{point.X}${point.Y}${point.Z}";
        }

        public static string AsString(this Vector3d point)
        {
            return $"{point.X}${point.Y}${point.Z}";
        }

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
        public static Point2d ConvertPoint3dToPoint2d(this Point3d point3d)
        {
            return new Point2d(point3d.X, point3d.Y);
        }
    }
}
