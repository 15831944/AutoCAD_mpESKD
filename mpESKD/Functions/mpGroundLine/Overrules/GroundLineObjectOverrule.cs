namespace mpESKD.Functions.mpGroundLine.Overrules
{
    using System.Diagnostics;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Utils;

    /// <inheritdoc />
    public class GroundLineObjectOverrule : ObjectOverrule
    {
        private static GroundLineObjectOverrule _groundLineObjectOverrule;

        /// <summary>
        /// Singleton instance
        /// </summary>
        /// <returns></returns>
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

        /// <inheritdoc />
        public override void Close(DBObject dbObject)
        {
            Debug.Print("GroundLineObjectOverrule");
            if (IsApplicable(dbObject))
            {
                EntityUtils.ObjectOverruleProcess(
                    dbObject, () => EntityReaderService.Instance.GetFromEntity<GroundLine>(dbObject));
            }

            base.Close(dbObject);
        }

        /// <inheritdoc/>
        public override bool IsApplicable(RXObject overruledSubject)
        {
            return ExtendedDataUtils.IsApplicable(overruledSubject, GroundLineDescriptor.Instance.Name, true);
        }
    }
}