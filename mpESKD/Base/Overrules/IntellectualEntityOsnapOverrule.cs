namespace mpESKD.Base.Overrules
{
    using System;
    using System.Diagnostics;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using ModPlusAPI.Windows;
    using Utils;

    /// <inheritdoc />
    public class IntellectualEntityOsnapOverrule : OsnapOverrule
    {
        private static IntellectualEntityOsnapOverrule _axisOsnapOverrule;

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static IntellectualEntityOsnapOverrule Instance()
        {
            if (_axisOsnapOverrule != null)
            {
                return _axisOsnapOverrule;
            }

            _axisOsnapOverrule = new IntellectualEntityOsnapOverrule();
            
            return _axisOsnapOverrule;
        }

        /// <inheritdoc/>
        public override void GetObjectSnapPoints(Entity entity, ObjectSnapModes snapMode, IntPtr gsSelectionMark, Point3d pickPoint,
            Point3d lastPoint, Matrix3d viewTransform, Point3dCollection snapPoints, IntegerCollection geometryIds)
        {
            Debug.Print("IntellectualEntityOsnapOverrule");
            if (IsApplicable(entity))
            {
                try
                {
                    var intellectualEntity = EntityReaderService.Instance.GetFromEntity(entity);
                    if (intellectualEntity != null)
                    {
                        foreach (var point3d in intellectualEntity.GetPointsForOsnap())
                        {
                            snapPoints.Add(point3d);
                        }
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception exception)
                {
                    ExceptionBox.Show(exception);
                }
            }
            else
            {
                base.GetObjectSnapPoints(entity, snapMode, gsSelectionMark, pickPoint, lastPoint, viewTransform, snapPoints, geometryIds);
            }
        }

        /// <inheritdoc />
        public override bool IsApplicable(RXObject overruledSubject)
        {
            return ExtendedDataUtils.IsApplicable(overruledSubject, true);
        }
    }
}
