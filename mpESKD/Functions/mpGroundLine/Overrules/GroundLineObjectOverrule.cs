// ReSharper disable InconsistentNaming
namespace mpESKD.Functions.mpGroundLine.Overrules
{
    using System.Diagnostics;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Helpers;
    using ModPlusAPI.Windows;

    public class GroundLineObjectOverrule : ObjectOverrule
    {
        protected static GroundLineObjectOverrule _groundLineObjectOverrule;

        public static GroundLineObjectOverrule Instance()
        {
            if (_groundLineObjectOverrule != null)
            {
                return _groundLineObjectOverrule;
            }

            _groundLineObjectOverrule = new GroundLineObjectOverrule();

            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _groundLineObjectOverrule.SetXDataFilter(GroundLineDescriptor.Instance.Name);
            return _groundLineObjectOverrule;
        }

        public override void Close(DBObject dbObject)
        {
            Debug.Print("GroundLineObjectOverrule");
            if (IsApplicable(dbObject))
            {
                try
                {
                    if (AcadHelpers.Document != null)
                    {
                        if (dbObject != null && dbObject.IsNewObject & dbObject.Database == AcadHelpers.Database ||
                            dbObject != null && dbObject.IsUndoing & dbObject.IsModifiedXData)
                        {
                            var groundLine = EntityReaderFactory.Instance.GetFromEntity<GroundLine>((Entity)dbObject);
                            if (groundLine != null)
                            {
                                groundLine.UpdateEntities();
                                groundLine.GetBlockTableRecordForUndo((BlockReference)dbObject).UpdateAnonymousBlocks();
                            }
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
            return ExtendedDataHelpers.IsApplicable(overruledSubject, GroundLineDescriptor.Instance.Name, true);
        }
    }
}