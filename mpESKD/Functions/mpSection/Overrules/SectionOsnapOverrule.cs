namespace mpESKD.Functions.mpSection.Overrules
{
    using System;
    using System.Diagnostics;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Helpers;
    using ModPlusAPI.Windows;

    public class SectionOsnapOverrule : OsnapOverrule
    {
        private static SectionOsnapOverrule _instance;

        public static SectionOsnapOverrule Instance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = new SectionOsnapOverrule();

            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _instance.SetXDataFilter(SectionDescriptor.Instance.Name);
            return _instance;
        }

        public override void GetObjectSnapPoints(Entity entity, ObjectSnapModes snapMode, IntPtr gsSelectionMark, Point3d pickPoint,
            Point3d lastPoint, Matrix3d viewTransform, Point3dCollection snapPoints, IntegerCollection geometryIds)
        {
            Debug.Print("SectionOsnapOverrule");
            if (IsApplicable(entity))
            {
                try
                {
                    var groundLine = EntityReaderFactory.Instance.GetFromEntity<mpSection.Section>(entity);
                    if (groundLine != null)
                    {
                        snapPoints.Add(groundLine.InsertionPoint);
                        groundLine.MiddlePoints.ForEach(p => snapPoints.Add(p));
                        snapPoints.Add(groundLine.EndPoint);
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

        public override bool IsApplicable(RXObject overruledSubject)
        {
            return ExtendedDataHelpers.IsApplicable(overruledSubject, SectionDescriptor.Instance.Name);
        }
    }
}
