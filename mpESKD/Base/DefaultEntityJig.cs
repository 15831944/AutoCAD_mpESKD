namespace mpESKD.Base
{
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Enums;
    using Helpers;
    using ModPlusAPI;

    /// <summary>
    /// Реализация стандартной EntityJig
    /// </summary>
    public class DefaultEntityJig : EntityJig
    {
        /// <summary>
        /// Обрабатываемый интеллектуальный примитив
        /// </summary>
        private readonly IntellectualEntity _intellectualEntity;

        private readonly JigHelper.PointSampler _insertionPoint = new JigHelper.PointSampler(Point3d.Origin);

        private readonly JigHelper.PointSampler _nextPoint;

        private readonly string _promptForNextPoint;

        /// <summary>
        /// Экземпляр <see cref="DefaultEntityJig"/>
        /// </summary>
        /// <param name="intellectualEntity">Экземпляр обрабатываемого интеллектуального примитива</param>
        /// <param name="blockReference">Вставка блока, представляющая обрабатываемый интеллектуальный примитив</param>
        /// <param name="startValueForNextPoint">Начальное значение для второй точки. Для примитивов, использующих
        /// конечную точку (EndPoint) влияет на отрисовку при указании первой точки</param>
        /// <param name="promptForNextPoint">Текстовое значение при запросе второй точки</param>
        public DefaultEntityJig(
            IntellectualEntity intellectualEntity,
            BlockReference blockReference,
            Point3d startValueForNextPoint,
            string promptForNextPoint)
            : base(blockReference)
        {
            _intellectualEntity = intellectualEntity;
            _nextPoint = new JigHelper.PointSampler(startValueForNextPoint);
            _promptForNextPoint = promptForNextPoint;
        }

        /// <summary>
        /// Статус
        /// </summary>
        public JigState JigState { get; set; } = JigState.PromptInsertPoint;

        /// <summary>
        /// Если значение не null, то PreviousPoint устанавливается как базовая точка
        /// при запросе второй точки. При этом JigState должен быть PromptNextPoint
        /// </summary>
        public Point3d? PreviousPoint { get; set; }

        /// <inheritdoc/>
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            try
            {
                switch (JigState)
                {
                    case JigState.PromptInsertPoint:
                        return _insertionPoint.Acquire(prompts, "\n" + Language.GetItem(Invariables.LangItem, "msg1"), value =>
                            {
                                _intellectualEntity.InsertionPoint = value;
                            });
                    case JigState.PromptNextPoint:
                        {
                            var basePoint = _insertionPoint.Value;
                            if (PreviousPoint != null)
                            {
                                basePoint = PreviousPoint.Value;
                            }

                            return _nextPoint.Acquire(prompts, "\n" + _promptForNextPoint, basePoint, value =>
                        {
                            _intellectualEntity.EndPoint = value;
                        });
                        }

                    default:
                        return SamplerStatus.NoChange;
                }
            }
            catch
            {
                return SamplerStatus.NoChange;
            }
        }

        /// <inheritdoc/>
        protected override bool Update()
        {
            try
            {
                using (AcadHelpers.Document.LockDocument(DocumentLockMode.ProtectedAutoWrite, null, null, true))
                {
                    using (var tr = AcadHelpers.Document.TransactionManager.StartTransaction())
                    {
                        var obj = (BlockReference)tr.GetObject(Entity.Id, OpenMode.ForWrite, true, true);
                        obj.Position = _intellectualEntity.InsertionPoint;
                        obj.BlockUnit = AcadHelpers.Database.Insunits;
                        tr.Commit();
                    }

                    _intellectualEntity.UpdateEntities();
                    _intellectualEntity.BlockRecord.UpdateAnonymousBlocks();
                }

                return true;
            }
            catch
            {
                // ignored
            }

            return false;
        }
    }
}
