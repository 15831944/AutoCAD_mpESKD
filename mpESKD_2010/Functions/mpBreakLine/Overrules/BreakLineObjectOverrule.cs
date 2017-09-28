using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using mpESKD.Base.Helpers;
using ModPlusAPI.Windows;

namespace mpESKD.Functions.mpBreakLine.Overrules
{
    public class BreakLineObjectOverrule : ObjectOverrule
    {

        protected static BreakLineObjectOverrule _breakLineObjectOverrule;
        public static BreakLineObjectOverrule Instance()
        {
            return _breakLineObjectOverrule ?? (_breakLineObjectOverrule = new BreakLineObjectOverrule());
        }

        public override void Close(DBObject dbObject)
        {
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
