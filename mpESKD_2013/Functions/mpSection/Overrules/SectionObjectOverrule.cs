namespace mpESKD.Functions.mpSection.Overrules
{
    using System.Diagnostics;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Helpers;
    using ModPlusAPI.Windows;

    public class SectionObjectOverrule : ObjectOverrule
    {
        private static SectionObjectOverrule _instance;

        public static SectionObjectOverrule Instance()
        {
            if (_instance != null) 
                return _instance;
            _instance = new SectionObjectOverrule();

            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _instance.SetXDataFilter(SectionDescriptor.Instance.Name);
            return _instance;
        }

        public override void Close(DBObject dbObject)
        {
            Debug.Print("SectionObjectOverrule");
            if (IsApplicable(dbObject))
            {
                try
                {
                    if (AcadHelpers.Document != null)
                        if (dbObject != null && dbObject.IsNewObject & dbObject.Database == AcadHelpers.Database ||
                            dbObject != null && dbObject.IsUndoing & dbObject.IsModifiedXData)
                        {
                            var axis = EntityReaderFactory.Instance.GetFromEntity<mpSection.Section>((Entity)dbObject);
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
            return ExtendedDataHelpers.IsApplicable(overruledSubject, SectionDescriptor.Instance.Name, true);
        }
    }
}
