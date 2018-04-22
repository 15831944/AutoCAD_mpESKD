using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using mpESKD.Base.Helpers;
using ModPlusAPI.Windows;
// ReSharper disable InconsistentNaming

namespace mpESKD.Functions.mpBreakLine.Overrules
{
    public class BreakLineObjectOverrule : ObjectOverrule
    {

        protected static BreakLineObjectOverrule _breakLineObjectOverrule;
        public static BreakLineObjectOverrule Instance()
        {
            if (_breakLineObjectOverrule != null) return _breakLineObjectOverrule;
            _breakLineObjectOverrule = new BreakLineObjectOverrule();
            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _breakLineObjectOverrule.SetXDataFilter(BreakLineFunction.MPCOEntName);
            return _breakLineObjectOverrule;
        }

        public override void Close(DBObject dbObject)
        {
            // Проверка дополнительных условий
            if (IsApplicable(dbObject))
            {
                try
                {
                    if(AcadHelpers.Document != null )
                        if (dbObject != null && dbObject.IsNewObject & dbObject.Database == AcadHelpers.Database ||
                            dbObject != null && dbObject.IsUndoing & dbObject.IsModifiedXData)
                    {
                        var breakLine = BreakLineXDataHelper.GetBreakLineFromEntity((Entity)dbObject);
                        if (breakLine != null)
                        {
                            breakLine.UpdateEntities();
                            breakLine.GetBlockTableRecordForUndo((BlockReference)dbObject).UpdateAnonymousBlocks();
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
            return ExtendedDataHelpers.IsApplicable(overruledSubject, BreakLineFunction.MPCOEntName);
        }
    }
}
