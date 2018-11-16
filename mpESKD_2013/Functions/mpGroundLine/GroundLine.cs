// ReSharper disable InconsistentNaming
namespace mpESKD.Functions.mpGroundLine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using ModPlusAPI.Windows;

    [IntellectualEntityDisplayNameKeyAttribute("h73")]
    public class GroundLine : IntellectualEntity
    {
        #region Constructor
        
        /// <summary>Инициализация экземпляра класса для GroundLine без заполнения данными
        /// В данном случае уже все данные получены и нужно только "построить" 
        /// базовые примитивы</summary>
        public GroundLine(ObjectId objectId)
        {
            BlockId = objectId;
        }

        public GroundLine()
        {
            var blockTableRecord = new BlockTableRecord
            {
                Name = "*U",
                BlockScaling = BlockScaling.Uniform
            };
            BlockRecord = blockTableRecord;
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
        
        /// <summary>
        /// Минимальная длина линии грунта
        /// </summary>
        public double GroundLineMinLength => 20.0;

        /// <summary>
        /// Отступ первого штриха в каждом сегменте полилинии
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 1, nameof(FirstStrokeOffset), "p36", "d36", 
            GroundLineFirstStrokeOffset.ByHalfSpace, null, null)]
        public GroundLineFirstStrokeOffset FirstStrokeOffset { get; set; } = GroundLineFirstStrokeOffset.ByHalfSpace;

        /// <summary>
        /// Длина штриха
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 2, nameof(StrokeLength), "p37", "d37", 8, 1, 10)]
        public int StrokeLength { get; set; } = 8;

        /// <summary>
        /// Расстояние между штрихами
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 3, nameof(StrokeOffset), "p38", "d38", 4, 1, 10)]
        public int StrokeOffset { get; set; } = 4;

        /// <summary>
        /// Угол наклона штриха в градусах
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 4, nameof(StrokeAngle), "p39", "d39", 60, 30, 90)]
        public int StrokeAngle { get; set; } = 60;

        /// <summary>
        /// Отступ группы штрихов
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 5, nameof(Space), "p40", "d40", 10, 1, 20)]
        public int Space { get; set; } = 10;

        /// <inheritdoc />
        [EntityProperty(PropertiesCategory.General, 4, nameof(LineType), "p35", "d35", "Continuous", null, null)]
        public override string LineType { get; set; }

        /// <inheritdoc />
        [EntityProperty(PropertiesCategory.General, 5, nameof(LineTypeScale), "p6", "d6", 1.0, 0.0, 1.0000E+99)]
        public override double LineTypeScale { get; set; }

        /// <inheritdoc />
        public override string TextStyle { get; set; }

        #endregion

        #region Примитивы ЕСКД объекта

        private readonly Lazy<Polyline> _mainPolyline = new Lazy<Polyline>(() => new Polyline());

        public Polyline MainPolyline
        {
            get
            {
                _mainPolyline.Value.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
                _mainPolyline.Value.LineWeight = LineWeight.ByBlock;
                _mainPolyline.Value.Linetype = "ByBlock";
                _mainPolyline.Value.LinetypeScale = LineTypeScale;
                return _mainPolyline.Value;
            }
        }

        public List<Line> Strokes { get; } = new List<Line>();

        public override IEnumerable<Entity> Entities
        {
            get
            {
                yield return MainPolyline;
                foreach (var s in Strokes)
                {
                    yield return s;
                }
            }
        }
        
        /// <summary>Установка свойств для примитивов, которые не меняются</summary>
        /// <param name="entity">Примитив автокада</param>
        private static void SetPropertiesToCadEntity(Entity entity)
        {
            entity.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
            entity.LineWeight = LineWeight.ByBlock;
            entity.Linetype = "Continuous";
            entity.LinetypeScale = 1.0;
        }

        /// <summary>
        /// Перестроение точек - помещение EndPoint в список
        /// </summary>
        public void RebasePoints()
        {
            if (!MiddlePoints.Contains(EndPoint))
                MiddlePoints.Add(EndPoint);
        }

        /// <inheritdoc />
        public override void UpdateEntities()
        {
            try
            {
                var length = EndPointOCS.DistanceTo(InsertionPointOCS);
                var scale = GetScale();
                if (EndPointOCS.Equals(Point3d.Origin))
                {
                    // Задание точки вставки. Второй точки еще нет - отрисовка типового элемента
                    MakeSimplyEntity(UpdateVariant.SetInsertionPoint, scale);
                }
                else if (length < GroundLineMinLength * scale && MiddlePoints.Count == 0)
                {
                    // Задание второй точки - случай когда расстояние между точками меньше минимального
                    MakeSimplyEntity(UpdateVariant.SetEndPointMinLength, scale);
                }
                else
                {
                    // Задание любой другой точки
                    CreateEntities(InsertionPointOCS, MiddlePointsOCS, EndPointOCS, scale);
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        private void MakeSimplyEntity(UpdateVariant variant, double scale)
        {
            if (variant == UpdateVariant.SetInsertionPoint)
            {
                /* Изменение базовых примитивов в момент указания второй точки при условии второй точки нет
                 * Примерно аналогично созданию, только точки не создаются, а меняются
                */
                var tmpEndPoint = new Point3d(InsertionPointOCS.X + GroundLineMinLength * scale, InsertionPointOCS.Y, InsertionPointOCS.Z);
                CreateEntities(InsertionPointOCS, MiddlePointsOCS, tmpEndPoint, scale);
            }
            else if (variant == UpdateVariant.SetEndPointMinLength)
            {
                /* Изменение базовых примитивов в момент указания второй точки
                * при условии что расстояние от второй точки до первой больше минимального допустимого
                */
                var tmpEndPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(InsertionPoint, EndPoint, InsertionPointOCS, GroundLineMinLength * scale);
                CreateEntities(InsertionPointOCS, MiddlePointsOCS, tmpEndPoint, scale);
                EndPoint = tmpEndPoint.TransformBy(BlockTransform);
            }
        }

        /// <summary>
        /// Создание примитивов ЕСКД элемента
        /// </summary>
        /// <param name="insertionPoint"></param>
        /// <param name="middlePoints"></param>
        /// <param name="endPoint"></param>
        /// <param name="scale"></param>
        private void CreateEntities(
            Point3d insertionPoint, List<Point3d> middlePoints,
            Point3d endPoint, double scale)
        {
            var points = GetPointsForMainPolyline(insertionPoint, middlePoints, endPoint);

            // Если количество точек совпадает, то просто их меняем
            if (points.Count == MainPolyline.NumberOfVertices)
            {
                for (var i = 0; i < points.Count; i++)
                {
                    MainPolyline.SetPointAt(i, points[i]);
                }
            }
            // Иначе создаем заново
            else
            {
                for (var i = 0; i < MainPolyline.NumberOfVertices; i++)
                    MainPolyline.RemoveVertexAt(i);
                for (var i = 0; i < points.Count; i++)
                {
                    if (i < MainPolyline.NumberOfVertices)
                        MainPolyline.SetPointAt(i, points[i]);
                    else MainPolyline.AddVertexAt(i, points[i], 0.0, 0.0, 0.0);
                }
            }

            // create strokes
            Strokes.Clear();
            if (MainPolyline.Length >= GroundLineMinLength)
            {
                for (var i = 1; i < MainPolyline.NumberOfVertices; i++)
                {
                    var previousPoint = MainPolyline.GetPoint3dAt(i - 1);
                    var currentPoint = MainPolyline.GetPoint3dAt(i);
                    Strokes.AddRange(CreateStrokesOnMainPolylineSegment(currentPoint, previousPoint, scale));
                }
            }
        }

        /// <summary>
        /// Создание штрихов на сегменте полилинии
        /// </summary>
        /// <param name="previousPoint"></param>
        /// <param name="currentPoint"></param>
        /// <param name="scale">Масштаб ЕСКД объекта</param>
        private List<Line> CreateStrokesOnMainPolylineSegment(
            Point3d currentPoint, Point3d previousPoint, double scale)
        {
            List<Line> segmentStrokeDependencies = new List<Line>();

            Vector3d segmentVector = currentPoint - previousPoint;
            double segmentLength = segmentVector.Length;
            Vector3d perpendicular = segmentVector.GetPerpendicularVector().Negate();
            double distanceAtSegmentStart = MainPolyline.GetDistAtPoint(previousPoint);

            var overflowIndex = 0;

            // Индекс штриха. Возможные значения - 0, 1, 2
            var strokeIndex = 0;
            var summDistanceAtSegment = 0.0;
            while (true)
            {
                double distance = 0.0;
                if (Math.Abs(summDistanceAtSegment) < 0.0001)
                {
                    if (FirstStrokeOffset == GroundLineFirstStrokeOffset.ByHalfSpace)
                        distance = Space / 2.0 * scale;
                    else if (FirstStrokeOffset == GroundLineFirstStrokeOffset.BySpace)
                        distance = Space * scale;
                    else distance = StrokeOffset * scale;
                }
                else
                {
                    if (strokeIndex == 0)
                        distance = Space * scale;
                    if (strokeIndex == 1 || strokeIndex == 2)
                        distance = StrokeOffset * scale;
                }

                if (strokeIndex == 2)
                    strokeIndex = 0;
                else strokeIndex++;

                summDistanceAtSegment += distance;

                if (summDistanceAtSegment >= segmentLength)
                    break;

                var firstStrokePoint = MainPolyline.GetPointAtDist(distanceAtSegmentStart + summDistanceAtSegment);
                var helpPoint =
                    firstStrokePoint + segmentVector.Negate().GetNormal() * StrokeLength * scale * Math.Cos(StrokeAngle.DegreeToRadian());
                var secondStrokePoint =
                    helpPoint + perpendicular * StrokeLength * scale * Math.Sin(StrokeAngle.DegreeToRadian());
                Line stroke = new Line(firstStrokePoint, secondStrokePoint);
                SetPropertiesToCadEntity(stroke);

                // индекс сегмента равен "левой" вершине
                segmentStrokeDependencies.Add(stroke);

                Debug.Assert(overflowIndex < 1000, "Overflow in stroke creation");
                if (overflowIndex >= 1000)
                    break;
            }

            return segmentStrokeDependencies;
        }

        private Point2dCollection GetPointsForMainPolyline(Point3d insertionPoint, List<Point3d> middlePoints, Point3d endPoint)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var points = new Point2dCollection();

            points.Add(ModPlus.Helpers.GeometryHelpers.ConvertPoint3dToPoint2d(insertionPoint));
            middlePoints.ForEach(p => points.Add(ModPlus.Helpers.GeometryHelpers.ConvertPoint3dToPoint2d(p)));
            points.Add(ModPlus.Helpers.GeometryHelpers.ConvertPoint3dToPoint2d(endPoint));

            return points;
        }

        #endregion
        
        public override ResultBuffer GetParametersForXData()
        {
            try
            {
                // ReSharper disable once UseObjectOrCollectionInitializer
                var resBuf = new ResultBuffer();

                // 1001 - DxfCode.ExtendedDataRegAppName. AppName
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, GroundLineInterface.Name));

                // 1010
                // Векторы от средних точек до начальной точки
                foreach (Point3d middlePointOCS in MiddlePointsOCS)
                {
                    var vector = middlePointOCS - InsertionPointOCS;
                    resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataXCoordinate, new Point3d(vector.X, vector.Y, vector.Z)));
                }

                // Вектор от конечной точки до начальной с учетом масштаба блока и трансформацией блока
                {
                    var vector = EndPointOCS - InsertionPointOCS;
                    resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataXCoordinate, new Point3d(vector.X, vector.Y, vector.Z)));
                }

                // Текстовые значения (код 1000)
                // Стиль
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, StyleGuid)); // 0
                // scale
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, Scale.Name)); // 1
                // Отступ первого штриха
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, FirstStrokeOffset.ToString())); // 2

                // Целочисленные значения (код 1070)
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, StrokeLength)); // 0
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, StrokeOffset)); // 1
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, StrokeAngle)); // 2
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, Space)); // 3

                // Значения типа double (dxfCode 1040)
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, LineTypeScale)); // 0

                return resBuf;
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
                return null;
            }
        }

        public override void GetParametersFromResBuf(ResultBuffer resBuf)
        {
            try
            {
                TypedValue[] resBufArr = resBuf.AsArray();
                /* indexes
                 * Для каждого значения с повторяющимся кодом назначен свой индекc (см. метод GetParametersForXData)
                 */
                List<Point3d> middleAndEndPoints = new List<Point3d>();
                var index1000 = 0;
                var index1040 = 0;
                var index1070 = 0;
                foreach (TypedValue typedValue in resBufArr)
                {
                    switch ((DxfCode)typedValue.TypeCode)
                    {
                        case DxfCode.ExtendedDataXCoordinate:
                            {
                                // Получаем вектор от точки до первой в системе координат блока
                                var vectorFromPointToInsertion = ((Point3d)typedValue.Value).GetAsVector();
                                // получаем точку в мировой системе координат
                                var point = (InsertionPointOCS + vectorFromPointToInsertion).TransformBy(BlockTransform);
                                middleAndEndPoints.Add(point);
                                break;
                            }
                        case DxfCode.ExtendedDataAsciiString:
                            {
                                switch (index1000)
                                {
                                    case 0:
                                        StyleGuid = typedValue.Value.ToString();
                                        break;
                                    case 1:
                                        Scale = AcadHelpers.GetAnnotationScaleByName(typedValue.Value.ToString());
                                        break;
                                    case 2:
                                        FirstStrokeOffset = Enum.TryParse(typedValue.Value.ToString(), out GroundLineFirstStrokeOffset so)
                                            ? so 
                                            : GroundLineFirstStrokeOffset.ByHalfSpace;
                                        break;
                                }
                                // index
                                index1000++;
                                break;
                            }
                        case DxfCode.ExtendedDataInteger16:
                            {
                                switch (index1070)
                                {
                                    case 0:
                                        StrokeLength = (Int16)typedValue.Value;
                                        break;
                                    case 1:
                                        StrokeOffset = (Int16)typedValue.Value;
                                        break;
                                    case 2:
                                        StrokeAngle = (Int16)typedValue.Value;
                                        break;
                                    case 3:
                                        Space = (Int16)typedValue.Value;
                                        break;
                                }
                                //index
                                index1070++;
                                break;
                            }
                        case DxfCode.ExtendedDataReal:
                            {
                                if (index1040 == 0) // 0 - LineTypeScale
                                    LineTypeScale = (double)typedValue.Value;
                                index1040++;
                                break;
                            }
                    }
                }

                // rebase points
                if (middleAndEndPoints.Any())
                {
                    EndPoint = middleAndEndPoints.Last();
                    MiddlePoints.Clear();
                    for (var i = 0; i < middleAndEndPoints.Count - 1; i++)
                    {
                        MiddlePoints.Add(middleAndEndPoints[i]);
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
    }
}