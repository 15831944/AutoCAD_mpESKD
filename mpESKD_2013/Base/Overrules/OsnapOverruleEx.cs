namespace mpESKD.Base.Overrules
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Helpers;
    using ModPlusAPI.Windows;

    public class OsnapOverruleEx : OsnapOverrule
    {
        protected static OsnapOverruleEx OsnapOverruleInstance;

        public static OsnapOverruleEx Instance()
        {
            if (OsnapOverruleInstance != null) 
                return OsnapOverruleInstance;
            OsnapOverruleInstance = new OsnapOverruleEx();
            
            return OsnapOverruleInstance;
        }

        public override void GetObjectSnapPoints(Entity entity, ObjectSnapModes snapMode, IntPtr gsSelectionMark, Point3d pickPoint,
            Point3d lastPoint, Matrix3d viewTransform, Point3dCollection snapPoints, IntegerCollection geometryIds)
        {
            // Проверка дополнительных условий
            if (IsApplicable(entity))
            {
                try
                {
                    var intellectualEntity = EntityReaderFactory.Instance.GetFromEntity(entity); 
                    if (intellectualEntity != null)
                    {
                        var type = intellectualEntity.GetType();
                        foreach (PropertyInfo propertyInfo in type.GetProperties())
                        {
                            var pointForOsnapAttribute = propertyInfo.GetCustomAttribute<PointForOsnapAttribute>();
                            if (pointForOsnapAttribute != null)
                            {
                                var value = propertyInfo.GetValue(intellectualEntity);
                                if (value is Point3d point)
                                    snapPoints.Add(point);
                            }
                            else
                            {
                                var pointsForOsnapAttribute = propertyInfo.GetCustomAttribute<ListOfPointsForOsnapAttribute>();
                                if (pointsForOsnapAttribute != null)
                                {
                                    var value = propertyInfo.GetValue(intellectualEntity);
                                    if (value is List<Point3d> points)
                                        points.ForEach(p => snapPoints.Add(p));
                                }
                            }
                        }
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception exception)
                {
                    ExceptionBox.Show(exception);
                }
            }
            else base.GetObjectSnapPoints(entity, snapMode, gsSelectionMark, pickPoint, lastPoint, viewTransform, snapPoints, geometryIds);
        }

        public override bool IsApplicable(RXObject overruledSubject)
        {
            return ExtendedDataHelpers.IsApplicable(overruledSubject);
        }
    }
}
