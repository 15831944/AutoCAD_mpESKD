namespace mpESKD.Functions.mpSection
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Base;

    public class Section : IntellectualEntity
    {
        public override IEnumerable<Entity> Entities { get; }
        public override string LineType { get; set; }
        public override double LineTypeScale { get; set; }
        public override string TextStyle { get; set; }
        public override void UpdateEntities()
        {
            throw new System.NotImplementedException();
        }

        public override ResultBuffer GetParametersForXData()
        {
            throw new System.NotImplementedException();
            // При сохранении свойств типа Enum, лучше сохранять их как int
        }

        public override void GetParametersFromResBuf(ResultBuffer resBuf)
        {
            throw new System.NotImplementedException();
        }
    }
}
