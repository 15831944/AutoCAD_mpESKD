namespace mpESKD.Functions.mpAxis.Overrules
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Runtime;
    using Base.Helpers;
    using ModPlusAPI.Windows;
    using System.Diagnostics;
    using Base;

    public class AxisObjectOverrule : ObjectOverrule
    {
        private static AxisObjectOverrule _instance;

        public static AxisObjectOverrule Instance()
        {
            if (_instance != null) return _instance;
            _instance = new AxisObjectOverrule();
            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _instance.SetXDataFilter(AxisDescriptor.Instance.Name);
            return _instance;
        }

        public override void Close(DBObject dbObject)
        {
            Debug.Print("AxisObjectOverrule");
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
            return ExtendedDataHelpers.IsApplicable(overruledSubject, AxisDescriptor.Instance.Name, true);
        }
    }
}
