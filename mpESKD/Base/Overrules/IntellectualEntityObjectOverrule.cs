namespace mpESKD.Base.Overrules
{
    using System.Diagnostics;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Runtime;
    using ModPlusAPI.Windows;
    using Utils;

    /// <inheritdoc />
    public class IntellectualEntityObjectOverrule : ObjectOverrule
    {
        private static IntellectualEntityObjectOverrule _intellectualEntityObjectOverrule;

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static IntellectualEntityObjectOverrule Instance()
        {
            if (_intellectualEntityObjectOverrule != null)
            {
                return _intellectualEntityObjectOverrule;
            }

            _intellectualEntityObjectOverrule = new IntellectualEntityObjectOverrule();

            return _intellectualEntityObjectOverrule;
        }

        /// <inheritdoc />
        public override void Close(DBObject dbObject)
        {
            Debug.Print($"ObjectOverrule Close: {dbObject?.GetRXClass().Name}");
            if (IsApplicable(dbObject))
            {
                try
                {
                    if (AcadUtils.Document == null)
                        return;

                    if ((dbObject != null && dbObject.IsNewObject & dbObject.Database == AcadUtils.Database) ||
                        (dbObject != null && dbObject.IsUndoing & dbObject.IsModifiedXData))
                    {
                        var entity = EntityReaderService.Instance.GetFromEntity(dbObject);
                        if (entity == null) 
                            return;

                        entity.UpdateEntities();
                        entity.GetBlockTableRecordForUndo((BlockReference)dbObject).UpdateAnonymousBlocks();
                    }
                }
                catch (Exception exception)
                {
                    ExceptionBox.Show(exception);
                }
            }

            base.Close(dbObject);
        }

        /// <inheritdoc />
        public override bool IsApplicable(RXObject overruledSubject)
        {
            return ExtendedDataUtils.IsApplicable(overruledSubject, true);
        }
    }
}
