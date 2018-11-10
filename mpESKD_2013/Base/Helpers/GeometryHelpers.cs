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
    }
}
