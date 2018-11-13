using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using mpESKD.Base.Helpers;
using ModPlusAPI.Windows;

// ReSharper disable InconsistentNaming

namespace mpESKD.Functions.mpAxis.Overrules
{
    using Base;

    public class AxisObjectOverrule : ObjectOverrule
    {
        protected static AxisObjectOverrule _axisObjectOverrule;
        public static AxisObjectOverrule Instance()
        {
            if (_axisObjectOverrule != null) return _axisObjectOverrule;
            _axisObjectOverrule = new AxisObjectOverrule();
            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _axisObjectOverrule.SetXDataFilter(AxisFunction.MPCOEntName);
            return _axisObjectOverrule;
        }
        public override void Close(DBObject dbObject)
        {
            // Проверка дополнительных условий
            if (IsApplicable(dbObject))
            {
                try
                {
                    if (AcadHelpers.Document != null)
                        if (dbObject != null && dbObject.IsNewObject & dbObject.Database == AcadHelpers.Database ||
                            dbObject != null && dbObject.IsUndoing & dbObject.IsModifiedXData)
                        {
                            var axis = EntityReaderFactory.Instance.GetFromEntity<Axis>((Entity)dbObject);
                            if (axis != null)
                            {
                                axis.UpdateEntities();
                                axis.GetBlockTableRecordForUndo((BlockReference)dbObject).UpdateAnonymousBlocks();
                            }
                        }
                }
                catch (Exception exception)
                {
                    ExceptionBox.Show(exception);
                }
            }
            base.Close(dbObject);
        }

        public override bool IsApplicable(RXObject overruledSubject)
        {
            return ExtendedDataHelpers.IsApplicable(overruledSubject, AxisFunction.MPCOEntName);
        }
    }
}
