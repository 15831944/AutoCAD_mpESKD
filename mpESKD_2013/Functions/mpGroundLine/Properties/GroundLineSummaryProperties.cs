namespace mpESKD.Functions.mpGroundLine.Properties
{
    using System;
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Base.Properties;

    public class GroundLineSummaryProperties : BaseSummaryProperties<GroundLinePropertiesData>
    {
        public GroundLineSummaryProperties(IEnumerable<ObjectId> objectIds)
        {
            foreach (ObjectId objectId in objectIds)
            {
                GroundLinePropertiesData data = new GroundLinePropertiesData(objectId);
                if (data.IsValid)
                    Add(data);
            }
        }

        #region Properties

        //TODO Add Properties

        #endregion

        public new void Add(GroundLinePropertiesData data)
        {
            base.Add(data);
            data.AnyPropertyChanged += Data_AnyPropertyChanged;
        }

        private void Data_AnyPropertyChanged(object sender, EventArgs e)
        {
            AllPropertyChangedReise();
        }
    }
}
