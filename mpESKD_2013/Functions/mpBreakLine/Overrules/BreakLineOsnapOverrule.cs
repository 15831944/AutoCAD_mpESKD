using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using mpESKD.Base.Helpers;
using ModPlusAPI.Windows;
// ReSharper disable InconsistentNaming

namespace mpESKD.Functions.mpBreakLine.Overrules
{
    using Base;

    public class BreakLineOsnapOverrule : OsnapOverrule
    {
        protected static BreakLineOsnapOverrule _breakLineOsnapOverrule;
        public static BreakLineOsnapOverrule Instance()
        {
            if (_breakLineOsnapOverrule != null) return _breakLineOsnapOverrule;
            _breakLineOsnapOverrule = new BreakLineOsnapOverrule();
            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _breakLineOsnapOverrule.SetXDataFilter(BreakLineFunction.MPCOEntName);
            return _breakLineOsnapOverrule;
        }

        public override void GetObjectSnapPoints(Entity entity, ObjectSnapModes snapMode, IntPtr gsSelectionMark, Point3d pickPoint,
            Point3d lastPoint, Matrix3d viewTransform, Point3dCollection snapPoints, IntegerCollection geometryIds)
        {
            // Проверка дополнительных условий
            if (IsApplicable(entity))
            {
                try
                {
                    var breakLine = EntityReaderFactory.Instance.GetFromEntity<BreakLine>(entity);
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
