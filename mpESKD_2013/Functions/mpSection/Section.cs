namespace mpESKD.Functions.mpSection
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Base.Enums;

    public class Section : IntellectualEntity
    {
        #region Constructors

        public Section()
        {
            var blockTableRecord = new BlockTableRecord
            {
                Name = "*U",
                BlockScaling = BlockScaling.Uniform
            };
            BlockRecord = blockTableRecord;
        }

        /// <summary>Инициализация экземпляра класса для Section без заполнения данными
        /// В данном случае уже все данные получены и нужно только "построить" 
        /// базовые примитивы</summary>
        public Section(ObjectId objectId)
        {
            BlockId = objectId;
        }

        #endregion

        #region Points and Grips

        /// <summary>
        /// Промежуточные точки
        /// </summary>
        public List<Point3d> MiddlePoints { get; set; } = new List<Point3d>();
        
        private List<Point3d> MiddlePointsOCS
        {
            get
            {
                List<Point3d> points = new List<Point3d>();
                MiddlePoints.ForEach(p => points.Add(p.TransformBy(BlockTransform.Inverse())));
                return points;
            }
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        /// В примитиве не используется!
        public override string LineType { get; set; }

        /// <inheritdoc />
        /// В примитиве не используется!
        public override double LineTypeScale { get; set; }

        /// <inheritdoc />
        /// todo translate
        [EntityProperty(PropertiesCategory.Content, 1, "", "", "Standard", null, null)]
        public override string TextStyle { get; set; }

        /// <summary>
        /// Минимальная длина
        /// </summary>
        public double SectionMinLength => 16.2;

        #endregion

        #region Geometry

        /// <summary>
        /// Средние штрихи - штрихи, создаваемые в средних точках
        /// </summary>
        public List<Polyline> MiddleStrokes { get; } = new List<Polyline>();

        /// <inheritdoc />
        public override IEnumerable<Entity> Entities
        {
            get
            {
                foreach (var s in MiddleStrokes)
                {
                    yield return s;
                }
            }
        }

        /// <inheritdoc />
        public override void UpdateEntities()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        /// <inheritdoc />
        public override ResultBuffer GetParametersForXData()
        {
            throw new System.NotImplementedException();
            // При сохранении свойств типа Enum, лучше сохранять их как int
        }

        /// <inheritdoc />
        public override void GetParametersFromResBuf(ResultBuffer resBuf)
        {
            throw new System.NotImplementedException();
        }
    }
}
