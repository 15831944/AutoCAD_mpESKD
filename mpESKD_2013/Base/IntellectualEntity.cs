// ReSharper disable InconsistentNaming

#pragma warning disable CS0618

namespace mpESKD.Base
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Helpers;
    using ModPlusAPI.Windows;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.Internal;
    using Enums;
    using ModPlusAPI.Annotations;
    using Styles;

    public abstract class IntellectualEntity : IDisposable
    {
        protected IntellectualEntity()
        {
            BlockTransform = Matrix3d.Identity;
            var blockTableRecord = new BlockTableRecord
            {
                Name = "*U",
                BlockScaling = BlockScaling.Uniform
            };
            BlockRecord = blockTableRecord;
        }

        /// <summary>Инициализация экземпляра класса IntellectualEntity без заполнения данными
        /// В данном случае уже все данные получены и нужно только "построить" 
        /// базовые примитивы</summary>
        protected IntellectualEntity(ObjectId blockId)
        {
            BlockId = blockId;
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
        [SaveToXData]
        public Point3d EndPoint { get; set; } = Point3d.Origin;

        /// <summary>
        /// Конечная точка примитива в системе координат блока для работы с геометрией в
        /// методе <see cref="UpdateEntities"/> ("внутри" блока). Имеется в каждом примитиве, но
        /// если не требуется, то просто не использовать её
        /// </summary>
        public Point3d EndPointOCS => EndPoint.TransformBy(BlockTransform.Inverse());

        /// <summary>
        /// Коллекция примитивов, создающих графическое представление интеллектуального примитива
        /// согласно его свойств
        /// </summary>
        public abstract IEnumerable<Entity> Entities { get; }

        /// <summary>
        /// Коллекция примитивов, которая передается в BlockReference
        /// </summary>
        private IEnumerable<Entity> _entities
        {
            get { return Entities.Where(e => e != null); }
        }

        public bool IsValueCreated { get; set; }

        /// <summary>Матрица трансформации BlockReference</summary>
        public Matrix3d BlockTransform { get; set; }

        /// <summary>
        /// Стиль примитива. Свойство используется для работы палитры, а стиль задается через свойство <see cref="StyleGuid"/>
        /// </summary>
        [EntityProperty(PropertiesCategory.General, 1, "h50", "h52", "", null, null, PropertyScope.Palette)]
        public string Style { get; set; } = string.Empty;

        /// <summary>
        /// Имя слоя
        /// </summary>
        [EntityProperty(PropertiesCategory.General, 2, "p7", "d7", "", null, null)]
        [SaveToXData]
        public string LayerName { get; set; } = string.Empty;

        private AnnotationScale _scale;

        /// <summary>Масштаб примитива</summary>
        [EntityProperty(PropertiesCategory.General, 3, "p5", "d5", "1:1", null, null)]
        [SaveToXData]
        public AnnotationScale Scale
        {
            get
            {
                if (_scale != null)
                    return _scale;
                return new AnnotationScale { Name = "1:1", DrawingUnits = 1, PaperUnits = 1 };
            }
            set
            {
                var oldScale = _scale;
                _scale = value;
                if (oldScale != null && oldScale != value)
                    ProcessScaleChange(oldScale, value);
            }
        }

        protected virtual void ProcessScaleChange(AnnotationScale oldScale, AnnotationScale newScale)
        {
        }
        
        /// <summary>
        /// Тип линии. Свойство является абстрактным, так как в зависимости от интеллектуального примитива
        /// может отличатся описание или может вообще быть не нужным. Индекс всегда нужно ставить = 4
        /// </summary>
        [SaveToXData]
        public abstract string LineType { get; set; }

        /// <summary>
        /// Масштаб типа линии для примитивов, имеющих изменяемый тип линии.
        /// Свойство является абстрактным, так как в зависимости от интеллектуального примитива
        /// может отличатся описание или может вообще быть не нужным. Индекс всегда нужно ставить = 5
        /// </summary>
        [SaveToXData]
        public abstract double LineTypeScale { get; set; }

        /// <summary>
        /// Текстовый стиль.
        /// Свойство является абстрактным, так как в зависимости от интеллектуального примитива
        /// может отличатся описание или может вообще быть не нужным. Индекс всегда нужно ставить = 1
        /// Категория всегда Content
        /// </summary>
        [SaveToXData]
        public abstract string TextStyle { get; set; }

        /// <summary>Текущий численный масштаб масштаб</summary>
        public double GetScale()
        {
            return Scale.GetNumericScale();
        }

        /// <summary>
        /// Текущий полный численный масштаб (с учетом масштаба блока)
        /// </summary>
        public double GetFullScale()
        {
            return GetScale() * BlockTransform.GetScale();
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
                                    using (var blockTable = AcadHelpers.Database.BlockTableId.Write<BlockTable>())
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
                                foreach (var entity in _entities)
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
                        foreach (var entity in _entities)
                        {
                            var transformedCopy = entity.GetTransformedCopy(matrix3D);
                            _blockRecord.AppendEntity(transformedCopy);
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
                    foreach (var entity in _entities)
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

                        foreach (Entity entity in _entities)
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
        public void GetPropertiesFromCadEntity(Entity entity)
        {
            var blockReference = (BlockReference)entity;
            if (blockReference != null)
            {
                InsertionPoint = blockReference.Position;
                BlockTransform = blockReference.BlockTransform;
            }
        }

        /// <summary>Идентификатор стиля</summary>
        [SaveToXData]
        public string StyleGuid { get; set; } = "00000000-0000-0000-0000-000000000000";

        /// <summary>
        /// Перерисовка элементов блока по параметрам ЕСКД элемента
        /// </summary>
        public abstract void UpdateEntities();

        /// <summary>
        /// Сериализация значений параметров, помеченных атрибутом <see cref="SaveToXDataAttribute"/>, в экземпляр <see cref="ResultBuffer"/>
        /// </summary>
        public ResultBuffer GetDataForXData()
        {
            return GetDataForXData("mp" + GetType().Name);
        }

        [CanBeNull]
        private ResultBuffer GetDataForXData(string appName)
        {
            try
            {
                // ReSharper disable once UseObjectOrCollectionInitializer
                var resultBuffer = new ResultBuffer();
                // 1001 - DxfCode.ExtendedDataRegAppName. AppName
                resultBuffer.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, appName));

                Dictionary<string, object> propertiesDataDictionary = new Dictionary<string, object>();
                foreach (PropertyInfo propertyInfo in GetType().GetProperties())
                {
                    var attribute = propertyInfo.GetCustomAttribute<SaveToXDataAttribute>();
                    if (attribute != null)
                    {
                        var value = propertyInfo.GetValue(this);
                        switch (value)
                        {
                            case AnnotationScale scale:
                                propertiesDataDictionary.Add(propertyInfo.Name, scale.Name);
                                break;
                            case Point3d point:
                                var vector = point.TransformBy(BlockTransform.Inverse()) - InsertionPointOCS;
                                propertiesDataDictionary.Add(propertyInfo.Name, vector.AsString());
                                break;
                            case List<Point3d> list:
                                var str = string.Join("#",
                                    list.Select(p => (p.TransformBy(BlockTransform.Inverse()) - InsertionPointOCS).AsString()));
                                propertiesDataDictionary.Add(propertyInfo.Name, str);
                                break;
                            case Enum e:
                                propertiesDataDictionary.Add(propertyInfo.Name, value.ToString());
                                break;
                            default:
                                propertiesDataDictionary.Add(propertyInfo.Name, value);
                                break;
                        }
                    }
                }

                DataHolder dataHolder = new DataHolder(propertiesDataDictionary);
                var binaryFormatter = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    binaryFormatter.Serialize(ms, dataHolder);
                    ms.Position = 0;
                    AcadHelpers.WriteMessageInDebug($"MemoryStream Length: {ms.Length} bytes or {ms.Length / 1024} KB");
                    int kMaxChunkSize = 127;
                    for (int i = 0; i < ms.Length; i += kMaxChunkSize)
                    {
                        var length = (int)Math.Min(ms.Length - i, kMaxChunkSize);
                        byte[] dataChunk = new byte[length];
                        ms.Read(dataChunk, 0, length);
                        resultBuffer.Add(new TypedValue((int)DxfCode.ExtendedDataBinaryChunk, dataChunk));
                    }
                }

                return resultBuffer;
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
                return null;
            }
        }

        /// <summary>
        /// Установка значений свойств, отмеченных атрибутом <see cref="SaveToXDataAttribute"/> из расширенных данных примитива AutoCAD
        /// </summary>
        /// <param name="resultBuffer"></param>
        /// <param name="skipPoints"></param>
        public void SetPropertiesValuesFromXData(ResultBuffer resultBuffer, bool skipPoints = false)
        {
            try
            {
                TypedValue typedValue1001 = resultBuffer.AsArray().FirstOrDefault(tv =>
                    tv.TypeCode == (int)DxfCode.ExtendedDataRegAppName && tv.Value.ToString() == "mp" + GetType().Name);
                if (typedValue1001.Value != null)
                {
                    //todo со временем убрать совсем
                    var json = GetJsonFromXDataValues(resultBuffer);
                    if (!string.IsNullOrEmpty(json))
                    {
                        using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                        {
                            WritePropertiesFromMemoryStream(skipPoints, ms);
                        }
                    }
                    else
                    {
                        var binaryFormatter = new BinaryFormatter { Binder = new Binder() };
                        var memoryStream = GetMemoryStreamFromResultBuffer(resultBuffer);
                        var dataHolder = (DataHolder)binaryFormatter.Deserialize(memoryStream);
                        WritePropertiesFromReadedData(skipPoints, dataHolder.Data);
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        private MemoryStream GetMemoryStreamFromResultBuffer(ResultBuffer resultBuffer)
        {
            var memoryStream = new MemoryStream();

            foreach (TypedValue typedValue in resultBuffer.AsArray()
                .Where(tv => tv.TypeCode == (int)DxfCode.ExtendedDataBinaryChunk))
            {
                var dataChunk = (byte[])typedValue.Value;
                memoryStream.Write(dataChunk, 0, dataChunk.Length);
            }

            memoryStream.Position = 0;

            return memoryStream;
        }

        //todo со временем убрать совсем
        private string GetJsonFromXDataValues(ResultBuffer resultBuffer)
        {
            string json = string.Empty;
            foreach (TypedValue typedValue in resultBuffer.AsArray()
                .Where(tv => tv.TypeCode == (int)DxfCode.ExtendedDataAsciiString))
            {
                json += typedValue.Value.ToString();
            }

            return json;
        }

        [Serializable]
        internal class DataHolder
        {
            public DataHolder(Dictionary<string, object> data)
            {
                Data = data;
            }

            public Dictionary<string, object> Data { get; }
        }

        internal class Binder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                return Type.GetType($"{typeName}, {assemblyName}");
            }
        }

        /// <summary>
        /// Запись данных в интеллектуальный объект из объекта <see cref="MemoryStream"/>, представляющего собой Json
        /// </summary>
        /// <param name="skipPoints"></param>
        /// <param name="ms"></param>
        //todo со временем убрать совсем
        private void WritePropertiesFromMemoryStream(bool skipPoints, MemoryStream ms)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<string, object>),
                new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });
            if (serializer.ReadObject(ms) is Dictionary<string, object> data)
            {
                WritePropertiesFromReadedData(skipPoints, data);
            }
        }

        /// <summary>
        /// Запись свойств текущего экземпляра интеллектуального объекта, полученных из расширенных
        /// данных блока в виде словаря
        /// </summary>
        /// <param name="skipPoints"></param>
        /// <param name="data"></param>
        private void WritePropertiesFromReadedData(bool skipPoints, Dictionary<string, object> data)
        {
            foreach (PropertyInfo propertyInfo in GetType().GetProperties())
            {
                var attribute = propertyInfo.GetCustomAttribute<SaveToXDataAttribute>();
                if (attribute != null && data.ContainsKey(propertyInfo.Name))
                {
                    string valueForProperty = data[propertyInfo.Name] != null
                        ? data[propertyInfo.Name].ToString()
                        : string.Empty;
                    if (string.IsNullOrEmpty(valueForProperty))
                        continue;

                    if (propertyInfo.Name == nameof(StyleGuid))
                    {
                        Style = StyleManager.GetStyleNameByGuid(GetType(), valueForProperty);
                    }
                    else if (propertyInfo.Name == nameof(Scale))
                    {
                        Scale = AcadHelpers.GetAnnotationScaleByName(valueForProperty);
                    }
                    else if (propertyInfo.PropertyType == typeof(Point3d))
                    {
                        if (skipPoints)
                            continue;
                        var vector = valueForProperty.ParseToPoint3d().GetAsVector();
                        var point = (InsertionPointOCS + vector).TransformBy(BlockTransform);
                        propertyInfo.SetValue(this, point);
                    }
                    else if (propertyInfo.PropertyType == typeof(List<Point3d>))
                    {
                        if (skipPoints)
                            continue;
                        List<Point3d> points = new List<Point3d>();
                        foreach (string s in valueForProperty.Split('#'))
                        {
                            var vector = s.ParseToPoint3d().GetAsVector();
                            var point = (InsertionPointOCS + vector).TransformBy(BlockTransform);
                            points.Add(point);
                        }

                        propertyInfo.SetValue(this, points);
                    }
                    else if (propertyInfo.PropertyType == typeof(int))
                    {
                        propertyInfo.SetValue(this, Convert.ToInt32(valueForProperty));
                    }
                    else if (propertyInfo.PropertyType == typeof(double))
                    {
                        propertyInfo.SetValue(this, Convert.ToDouble(valueForProperty));
                    }
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        propertyInfo.SetValue(this, Convert.ToBoolean(valueForProperty));
                    }
                    else if (propertyInfo.PropertyType.BaseType == typeof(Enum))
                    {
                        propertyInfo.SetValue(this, Enum.Parse(propertyInfo.PropertyType, valueForProperty));
                    }
                    else
                    {
                        propertyInfo.SetValue(this, valueForProperty);
                    }
                }
            }
        }

        /// <summary>
        /// Копирование свойств, отмеченных атрибутом <see cref="SaveToXDataAttribute"/> из расширенных данных примитива AutoCAD
        /// в текущий интеллектуальный примитив
        /// </summary>
        public void SetPropertiesFromIntellectualEntity(IntellectualEntity sourceEntity, bool copyLayer)
        {
            ResultBuffer dataForXData = sourceEntity.GetDataForXData();
            if (dataForXData != null)
            {
                SetPropertiesValuesFromXData(dataForXData, true);

                if (sourceEntity.BlockId != ObjectId.Null)
                {
                    using (var tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var entity = tr.GetObject(sourceEntity.BlockId, OpenMode.ForRead) as Entity;
                        var destinationBlockReference = tr.GetObject(BlockId, OpenMode.ForWrite) as BlockReference;
                        if (entity != null && destinationBlockReference != null)
                        {
                            destinationBlockReference.LinetypeId = entity.LinetypeId;
                            if(copyLayer)
                                destinationBlockReference.Layer = entity.Layer;
                        }

                        tr.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// Установка свойств для примитивов, которые не меняются
        /// </summary>
        /// <param name="entity">Примитив автокада</param>
        public void SetImmutablePropertiesToNestedEntity(Entity entity)
        {
            entity.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
            entity.LineWeight = LineWeight.ByBlock;
            entity.Linetype = "Continuous";
            entity.LinetypeScale = 1.0;
        }

        /// <summary>
        /// Установка свойств для примитива, которые могут меняться "из вне" (ByBlock)
        /// </summary>
        /// <param name="entity">Примитив автокада</param>
        public void SetChangeablePropertiesToNestedEntity(Entity entity)
        {
            entity.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
            entity.LineWeight = LineWeight.ByBlock;
            entity.Linetype = "ByBlock";
            entity.LinetypeScale = LineTypeScale;
        }

        public void Dispose()
        {
            _blockRecord?.Dispose();
        }
    }
}