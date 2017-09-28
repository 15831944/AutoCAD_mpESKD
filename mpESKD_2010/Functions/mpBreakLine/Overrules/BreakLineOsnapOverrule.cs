using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using mpESKD.Base.Helpers;
using ModPlusAPI.Windows;

namespace mpESKD.Functions.mpBreakLine.Overrules
{
    public class BreakLineOsnapOverrule : OsnapOverrule
    {
        protected static BreakLineOsnapOverrule _breakLineOsnapOverrule;
        public static BreakLineOsnapOverrule Instance()
        {
            return _breakLineOsnapOverrule ?? (_breakLineOsnapOverrule = new BreakLineOsnapOverrule());
        }

        public override void GetObjectSnapPoints(Autodesk.AutoCAD.DatabaseServices.Entity entity, ObjectSnapModes snapMode, IntPtr gsSelectionMark, Point3d pickPoint,
            Point3d lastPoint, Matrix3d viewTransform, Point3dCollection snapPoints, IntegerCollection geometryIds)
        {
            if (IsApplicable(entity))
            {
                try
                {
                    var breakLine = BreakLineXDataHelper.GetBreakLineFromEntity(entity);
                    if (breakLine != null)
                    {
                        snapPoints.Add(breakLine.InsertionPoint);
                        snapPoints.Add(breakLine.EndPoint);
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
            return ExtendedDataHelpers.IsApplicable(overruledSubject, BreakLineFunction.MPCOEntName);
        }
    }
}
