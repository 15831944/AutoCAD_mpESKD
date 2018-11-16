using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using mpESKD.Base.Helpers;
using ModPlusAPI.Windows;
// ReSharper disable InconsistentNaming
#pragma warning disable CS0618

namespace mpESKD.Base
{
    using Enums;

    public abstract class IntellectualEntity : IDisposable
    {
        protected IntellectualEntity()
        {
            BlockTransform = Matrix3d.Identity;
        }

        /// <summary>
        /// Первая точка примитива в мировой системе координат.
        /// Должна соответствовать точке вставке блока
        /// </summary>
        public Point3d InsertionPoint { get; set; } = Point3d.Origin;

        /// <summary>
        /// Первая точка примитива в системе координат блока для работы с геометрией в
        /// методе <see cref="UpdateEntities"/> ("внутри" блока)
        /// </summary>
        public Point3d InsertionPointOCS => InsertionPoint.TransformBy(BlockTransform.Inverse());

        /// <summary>
        /// Конечная точка примитива в мировой системе координат. Свойство содержится в базовом классе для
        /// работы <see cref="DefaultEntityJig"/>. Имеется в каждом примитиве, но
        /// если не требуется, то просто не использовать её
        /// </summary>
        public Point3d EndPoint { get; set; } = Point3d.Origin;

        /// <summary>
        /// Конечная точка примитива в системе координат блока для работы с геометрией в
        /// методе <see cref="UpdateEntities"/> ("внутри" блока). Имеется в каждом примитиве, но
        /// если не требуется, то просто не использовать её
        /// </summary>
        public Point3d EndPointOCS => EndPoint.TransformBy(BlockTransform.Inverse());

        /// <summary>Коллекция базовых примитивов, входящих в примитив</summary>
        public abstract IEnumerable<Entity> Entities { get; }

        public bool IsValueCreated { get; set; }

        /// <summary>Матрица трансформации BlockReference</summary>
        public Matrix3d BlockTransform { get; set; }

        /// <summary>
        /// Стиль примитива. Свойство используется для работы палитры, а стиль задается через свойство <see cref="StyleGuid"/>
        /// </summary>
        [EntityProperty(PropertiesCategory.General, 1, nameof(Style), "h50", "h52", "", null, null, PropertyScope.Palette)]
        public string Style { get; set; } = string.Empty;

        /// <summary>
        /// Имя слоя
        /// </summary>
        [EntityProperty(PropertiesCategory.General, 2, nameof(LayerName), "p7", "d7", "", null, null)]
        public string LayerName { get; set; } = string.Empty;

        private AnnotationScale _scale;
        /// <summary>Масштаб примитива</summary>
        [EntityProperty(PropertiesCategory.General, 3, nameof(Scale), "p5", "d5", "1:1", null, null)]
        public AnnotationScale Scale
        {
            get
            {
                if (_scale != null)
                    return _scale;
                _scale = new AnnotationScale { Name = "1:1", DrawingUnits = 1, PaperUnits = 1};
                return _scale;
            }
            set => _scale = value;
        }

        /// <summary>
        /// Тип линии. Свойство является абстрактным, так как в зависимости от интеллектуального примитива
        /// может отличатся описание или может вообще быть не нужным. Индекс всегда нужно ставить = 4
        /// </summary>
        public abstract string LineType { get; set; }
        
        /// <summary>
        /// Масштаб типа линии для примитивов, имеющих изменяемый тип линии.
        /// Свойство является абстрактным, так как в зависимости от интеллектуального примитива
        /// может отличатся описание или может вообще быть не нужным. Индекс всегда нужно ставить = 5
        /// </summary>
        public abstract double LineTypeScale { get; set; }

        /// <summary>
        /// Текстовый стиль.
        /// Свойство является абстрактным, так как в зависимости от интеллектуального примитива
        /// может отличатся описание или может вообще быть не нужным. Индекс всегда нужно ставить = 1
        /// Категория всегда Content
        /// </summary>
        public abstract string TextStyle { get; set; }

        /// <summary>Текущий масштаб</summary>
        public double GetScale()
        {
            return Scale.DrawingUnits / Scale.PaperUnits;
        }

        #region Block

        // ObjectId "примитива"
        public ObjectId BlockId { get; set; }

        // Описание блока
        private BlockTableRecord _blockRecord;
        
        public BlockTableRecord BlockRecord
        {
            get
            {
                try
                {
                    if (!BlockId.IsNull)
                    {
                        using (AcadHelpers.Document.LockDocument())
                        {
                            using (var tr = AcadHelpers.Database.TransactionManager.StartTransaction())
                            {
                                var blkRef = (BlockReference)tr.GetObject(BlockId, OpenMode.ForWrite);
                                _blockRecord = (BlockTableRecord)tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForWrite);
                                if (_blockRecord.GetBlockReferenceIds(true, true).Count <= 1)
                                {
                                    //Debug.Print("Erasing");
                                    foreach (var objectId in _blockRecord)
                                    {
                                        tr.GetObject(objectId, OpenMode.ForWrite).Erase();
                                    }
                                }
                                else
                                {
                                    _blockRecord = new BlockTableRecord { Name = "*U", BlockScaling = BlockScaling.Uniform };
                                    using (var blockTable =
                                        AcadHelpers.Database.BlockTableId.Write<BlockTable>())
                                    {
                                        //Debug.Print("Creating new (no erasing)");
                                        blockTable.Add(_blockRecord);
                                        tr.AddNewlyCreatedDBObject(_blockRecord, true);
                                    }
                                    blkRef.BlockTableRecord = _blockRecord.Id;
                                }
                                tr.Commit();
                            }
                            using (var tr = AcadHelpers.Database.TransactionManager.StartTransaction())
                            {
                                var blkRef = (BlockReference)tr.GetObject(BlockId, OpenMode.ForWrite);
                                _blockRecord = (BlockTableRecord)tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForWrite);
                                _blockRecord.BlockScaling = BlockScaling.Uniform;
                                var matrix3D = Matrix3d.Displacement(-InsertionPoint.TransformBy(BlockTransform.Inverse()).GetAsVector());
                                //Debug.Print("Transformed copy");
                                foreach (var entity in Entities)
                                {
                                    if (entity.Visible)
                                    {
                                        var transformedCopy = entity.GetTransformedCopy(matrix3D);
                                        _blockRecord.AppendEntity(transformedCopy);
                                        tr.AddNewlyCreatedDBObject(transformedCopy, true);
                                    }
                                }
                                tr.Commit();
                            }
                            AcadHelpers.Document.TransactionManager.FlushGraphics();
                        }
                    }
                    else if (!IsValueCreated)
                    {
                        //Debug.Print("Value not created");
                        var matrix3D = Matrix3d.Displacement(-InsertionPoint.TransformBy(BlockTransform.Inverse()).GetAsVector());
                        foreach (var entity in Entities)
                        {
                            if (entity.Visible)
                            {
                                var transformedCopy = entity.GetTransformedCopy(matrix3D);
                                _blockRecord.AppendEntity(transformedCopy);
                            }
                        }
                        IsValueCreated = true;
                    }
                }
                catch (Exception exception)
                {
                    ExceptionBox.Show(exception);
                }
                return _blockRecord;
            }
            set => _blockRecord = value;
        }

        public BlockTableRecord GetBlockTableRecordForUndo(BlockReference blkRef)
        {
            BlockTableRecord blockTableRecord;
            using (AcadHelpers.Document.LockDocument())
            {
                using (var tr = AcadHelpers.Database.TransactionManager.StartTransaction())
                {
                    blockTableRecord = new BlockTableRecord { Name = "*U", BlockScaling = BlockScaling.Uniform };
                    using (var blockTable = AcadHelpers.Database.BlockTableId.Write<BlockTable>())
                    {
                        blockTable.Add(blockTableRecord);
                        tr.AddNewlyCreatedDBObject(blockTableRecord, true);
                    }
                    blkRef.BlockTableRecord = blockTableRecord.Id;
                    tr.Commit();
                }
                using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    blockTableRecord = (BlockTableRecord)tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForWrite);
                    blockTableRecord.BlockScaling = BlockScaling.Uniform;
                    var matrix3D = Matrix3d.Displacement(-InsertionPoint.TransformBy(BlockTransform.Inverse()).GetAsVector());
                    foreach (var entity in Entities)
                    {
                        var transformedCopy = entity.GetTransformedCopy(matrix3D);
                        blockTableRecord.AppendEntity(transformedCopy);
                        tr.AddNewlyCreatedDBObject(transformedCopy, true);
                    }
                    tr.Commit();
                }
            }
            _blockRecord = blockTableRecord;
            return blockTableRecord;
        }

        public BlockTableRecord GetBlockTableRecordWithoutTransaction(BlockReference blkRef)
        {
            BlockTableRecord blockTableRecord;
            using (AcadHelpers.Document.LockDocument())
            {
                using (blockTableRecord = blkRef.BlockTableRecord.Open(OpenMode.ForWrite) as BlockTableRecord)
                {
                    if (blockTableRecord != null)
                    {
                        foreach (ObjectId objectId in blockTableRecord)
                        {
                            using (var ent = objectId.Open(OpenMode.ForWrite))
                            {
                                ent.Erase(true);
                            }
                        }
                        foreach (Entity entity in Entities)
                        {
                            using (entity)
                            {
                                blockTableRecord.AppendEntity(entity);
                            }
                        }
                    }
                }
            }
            _blockRecord = blockTableRecord;
            return blockTableRecord;
        }

        #endregion

        /// <summary>Получение свойств блока, которые присуще примитиву</summary>
        public void GetParametersFromEntity(Entity entity)
        {
            var blockReference = (BlockReference)entity;
            if (blockReference != null)
            {
                InsertionPoint = blockReference.Position;
                BlockTransform = blockReference.BlockTransform;
            }
        }

        /// <summary>Идентификатор стиля</summary>
        public string StyleGuid { get; set; } = "00000000-0000-0000-0000-000000000000";
        
        /// <summary>
        /// Перерисовка элементов блока по параметрам ЕСКД элемента
        /// </summary>
        public abstract void UpdateEntities();

        public abstract ResultBuffer GetParametersForXData();

        public abstract void GetParametersFromResBuf(ResultBuffer resBuf);


        public void Draw(WorldDraw draw)
        {
            var geometry = draw.Geometry;
            foreach (var entity in Entities)
            {
                geometry.Draw(entity);
            }
        }

        public void Erase()
        {
            foreach (var entity in Entities)
            {
                entity.Erase();
            }
        }
        /// <summary>
        /// Расчлинение интеллектуального примитива
        /// </summary>
        /// <param name="entitySet"></param>
        public void Explode(DBObjectCollection entitySet)
        {
            entitySet.Clear();
            foreach (var entity in Entities)
            {
                entitySet.Add(entity);
            }
        }

        public void Dispose()
        {
            _blockRecord?.Dispose();
        }
    }

    public sealed class MyBinder : SerializationBinder
    {
        public override Type BindToType(
            string assemblyName,
            string typeName)
        {
            return Type.GetType($"{typeName}, {assemblyName}");
        }
    }
}
