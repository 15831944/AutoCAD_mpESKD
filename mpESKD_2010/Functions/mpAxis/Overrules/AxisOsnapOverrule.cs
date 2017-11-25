using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using mpESKD.Base.Helpers;
using mpESKD.Functions.mpAxis.Properties;
using ModPlusAPI.Windows;
// ReSharper disable InconsistentNaming

namespace mpESKD.Functions.mpAxis.Overrules
{
    public class AxisOsnapOverrule : OsnapOverrule
    {
        protected static AxisOsnapOverrule _axisOsnapOverrule;
        public static AxisOsnapOverrule Instance()
        {
            if (_axisOsnapOverrule != null) return _axisOsnapOverrule;
            _axisOsnapOverrule = new AxisOsnapOverrule();
            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _axisOsnapOverrule.SetXDataFilter(AxisFunction.MPCOEntName);
            return _axisOsnapOverrule;
        }

        public override void GetObjectSnapPoints(Entity entity, ObjectSnapModes snapMode, IntPtr gsSelectionMark, Point3d pickPoint,
            Point3d lastPoint, Matrix3d viewTransform, Point3dCollection snapPoints, IntegerCollection geometryIds)
        {
            // Проверка дополнительных условий
            if (IsApplicable(entity))
            {
                try
                {
                    var axis = AxisXDataHelper.GetAxisFromEntity(entity);
                    if (axis != null)
                    {
                        snapPoints.Add(axis.InsertionPoint);
                        snapPoints.Add(axis.EndPoint);
                        if (axis.MarkersPosition == AxisMarkersPosition.Both ||
                            axis.MarkersPosition == AxisMarkersPosition.Bottom)
                        {
                            snapPoints.Add(axis.BottomMarkerPoint);
                            if (axis.BottomOrientMarkerVisible)
                                snapPoints.Add(axis.BottomOrientPoint);
                        }
                        if (axis.MarkersPosition == AxisMarkersPosition.Both ||
                            axis.MarkersPosition == AxisMarkersPosition.Top)
                        {
                            snapPoints.Add(axis.TopMarkerPoint);
                            if (axis.TopOrientMarkerVisible)
                                snapPoints.Add(axis.TopOrientPoint);
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
            return ExtendedDataHelpers.IsApplicable(overruledSubject, AxisFunction.MPCOEntName);
        }
    }
}
