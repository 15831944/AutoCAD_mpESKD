// ReSharper disable InconsistentNaming
namespace mpESKD.Base.Overrules
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.GraphicsInterface;
    using Enums;

    public abstract class IntellectualEntityGripData : GripData
    {
        /// <summary>
        /// Тип ручки примитива
        /// </summary>
        public GripType GripType { get; set; }

        public override bool ViewportDraw(ViewportDraw worldDraw, ObjectId entityId, DrawType type, Point3d? imageGripPoint,
            int gripSizeInPixels)
        {
            CoordinateSystem3d eCS = GetECS(entityId);
            Point2d numPixelsInUnitSquare = worldDraw.Viewport.GetNumPixelsInUnitSquare(GripPoint);
            double num = gripSizeInPixels / numPixelsInUnitSquare.X;

            Point3dCollection point3dCollections = new Point3dCollection();
            switch (GripType)
            {
                case GripType.Point:
                    point3dCollections = PointsForSquareGrip(num, eCS);
                    break;
                case GripType.Plus:
                    point3dCollections = PointsForPlusGrip(num, eCS);
                    break;
                case GripType.Minus:
                    point3dCollections = PointsForMinusGrip(num, eCS);
                    break;
            }
            short backupColor = worldDraw.SubEntityTraits.Color;
            FillType backupFillType = worldDraw.SubEntityTraits.FillType;

            worldDraw.SubEntityTraits.FillType = FillType.FillAlways;
            worldDraw.SubEntityTraits.Color = GetGripColor();
            if (GripType != GripType.Mirror)
                worldDraw.Geometry.Polygon(point3dCollections);
            else
            {
                worldDraw.Geometry.Polygon(PointsForReverseGripFirstArrow(num, eCS));
                worldDraw.Geometry.Polygon(PointsForReverseGripSecondArrow(num, eCS));
            }
            worldDraw.SubEntityTraits.FillType = FillType.FillNever;
            // обводка
            worldDraw.SubEntityTraits.Color = 250;
            if (GripType != GripType.Mirror)
                worldDraw.Geometry.Polygon(point3dCollections);
            else
            {
                worldDraw.Geometry.Polygon(PointsForReverseGripFirstArrow(num, eCS));
                worldDraw.Geometry.Polygon(PointsForReverseGripSecondArrow(num, eCS));
            }
            // restore
            worldDraw.SubEntityTraits.Color = backupColor;
            worldDraw.SubEntityTraits.FillType = backupFillType;

            return true;
        }

        protected CoordinateSystem3d GetECS(ObjectId entityId)
        {
            CoordinateSystem3d coordinateSystem3D = new CoordinateSystem3d(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis);
            if (!entityId.IsNull)
            {
                using (OpenCloseTransaction openCloseTransaction = new OpenCloseTransaction())
                {
                    Entity obj = (Entity)openCloseTransaction.GetObject(entityId, OpenMode.ForRead);
                    if (obj != null)
                    {
                        Plane plane = obj.GetPlane();
                        Plane plane1 = new Plane(plane.PointOnPlane, plane.Normal);
                        coordinateSystem3D = plane1.GetCoordinateSystem();
                    }
                    openCloseTransaction.Commit();
                }
            }
            return coordinateSystem3D;
        }

        private Point3dCollection PointsForSquareGrip(double num, CoordinateSystem3d eCS)
        {
            Point3dCollection point3dCollections = new Point3dCollection();
            point3dCollections.Add((GripPoint - (num * eCS.Xaxis)) - (num * eCS.Yaxis));
            point3dCollections.Add((GripPoint - (num * eCS.Xaxis)) + (num * eCS.Yaxis));
            point3dCollections.Add((GripPoint + (num * eCS.Xaxis)) + (num * eCS.Yaxis));
            point3dCollections.Add((GripPoint + (num * eCS.Xaxis)) - (num * eCS.Yaxis));

            return point3dCollections;
        }

        private Point3dCollection PointsForPlusGrip(double num, CoordinateSystem3d eCS)
        {
            var num2 = num / 3;
            Point3dCollection point3dCollection = new Point3dCollection();

            point3dCollection.Add(GripPoint + num * eCS.Xaxis + num2 * eCS.Yaxis);
            point3dCollection.Add(GripPoint + num2 * eCS.Xaxis + num2 * eCS.Yaxis);
            point3dCollection.Add(GripPoint + num2 * eCS.Xaxis + num * eCS.Yaxis);
            point3dCollection.Add(GripPoint - num2 * eCS.Xaxis + num * eCS.Yaxis);
            point3dCollection.Add(GripPoint - num2 * eCS.Xaxis + num2 * eCS.Yaxis);
            point3dCollection.Add(GripPoint - num * eCS.Xaxis + num2 * eCS.Yaxis);
            point3dCollection.Add(GripPoint - num * eCS.Xaxis - num2 * eCS.Yaxis);
            point3dCollection.Add(GripPoint - num2 * eCS.Xaxis - num2 * eCS.Yaxis);
            point3dCollection.Add(GripPoint - num2 * eCS.Xaxis - num * eCS.Yaxis);
            point3dCollection.Add(GripPoint + num2 * eCS.Xaxis - num * eCS.Yaxis);
            point3dCollection.Add(GripPoint + num2 * eCS.Xaxis - num2 * eCS.Yaxis);
            point3dCollection.Add(GripPoint + num * eCS.Xaxis - num2 * eCS.Yaxis);

            return point3dCollection;
        }

        private Point3dCollection PointsForMinusGrip(double num, CoordinateSystem3d eCS)
        {
            var num2 = num / 3;
            Point3dCollection point3dCollection = new Point3dCollection();

            point3dCollection.Add(GripPoint - num * eCS.Xaxis + num2 * eCS.Yaxis);
            point3dCollection.Add(GripPoint + num * eCS.Xaxis + num2 * eCS.Yaxis);
            point3dCollection.Add(GripPoint + num * eCS.Xaxis - num2 * eCS.Yaxis);
            point3dCollection.Add(GripPoint - num * eCS.Xaxis - num2 * eCS.Yaxis);

            return point3dCollection;
        }

        private Point3dCollection PointsForReverseGripFirstArrow(double num, CoordinateSystem3d eCS)
        {
            Point3dCollection point3dCollection = new Point3dCollection();

            point3dCollection.Add(GripPoint - num * eCS.Xaxis * 0.0 - num * eCS.Yaxis * 0.25);
            point3dCollection.Add(GripPoint - num * eCS.Xaxis * 0.75 + num * eCS.Yaxis * 1.25);
            point3dCollection.Add(GripPoint - num * eCS.Xaxis * 1.5 - num * eCS.Yaxis * 0.25);
            point3dCollection.Add(GripPoint - num * eCS.Xaxis * 1.0 - num * eCS.Yaxis * 0.25);
            point3dCollection.Add(GripPoint - num * eCS.Xaxis * 1.0 - num * eCS.Yaxis * 1.25);
            point3dCollection.Add(GripPoint - num * eCS.Xaxis * 0.5 - num * eCS.Yaxis * 1.25);
            point3dCollection.Add(GripPoint - num * eCS.Xaxis * 0.5 - num * eCS.Yaxis * 0.25);

            return point3dCollection;
        }

        private Point3dCollection PointsForReverseGripSecondArrow(double num, CoordinateSystem3d eCS)
        {
            Point3dCollection point3dCollection = new Point3dCollection();

            point3dCollection.Add(GripPoint + num * eCS.Xaxis * 0.0 + num * eCS.Yaxis * 0.25);
            point3dCollection.Add(GripPoint + num * eCS.Xaxis * 0.75 - num * eCS.Yaxis * 1.25);
            point3dCollection.Add(GripPoint + num * eCS.Xaxis * 1.5 + num * eCS.Yaxis * 0.25);
            point3dCollection.Add(GripPoint + num * eCS.Xaxis * 1.0 + num * eCS.Yaxis * 0.25);
            point3dCollection.Add(GripPoint + num * eCS.Xaxis * 1.0 + num * eCS.Yaxis * 1.25);
            point3dCollection.Add(GripPoint + num * eCS.Xaxis * 0.5 + num * eCS.Yaxis * 1.25);
            point3dCollection.Add(GripPoint + num * eCS.Xaxis * 0.5 + num * eCS.Yaxis * 0.25);

            return point3dCollection;
        }

        private short GetGripColor()
        {
            switch (GripType)
            {
                case GripType.Plus:
                    return 110;
                case GripType.Minus:
                    return 20;
            }

            return 150;
        }
    }
}
