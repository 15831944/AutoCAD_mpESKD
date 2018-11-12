namespace mpESKD.Functions.mpGroundLine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using Base.Styles;
    using ModPlusAPI.Windows;
    using Properties;
    using Styles;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GroundLine : MPCOEntity
    {
        #region Constructor

        /// <summary>Инициализация экземпляра класса для GroundLine без заполнения данными
        /// В данном случае уже все данные получены и нужно только "построить" 
        /// базовые примитивы</summary>
        public GroundLine(ObjectId objectId)
        {
            BlockId = objectId;
        }

        public GroundLine(GroundLineStyle style)
        {
            var blockTableRecord = new BlockTableRecord
            {
                Name = "*U",
                BlockScaling = BlockScaling.Uniform
            };
            BlockRecord = blockTableRecord;
            StyleGuid = style.Guid;

            // Применяем текущий стиль к ЕСКД примитиву
            ApplyStyle(style);
        }

        #endregion

        #region Points and Grips

        /// <summary>
        /// Промежуточные точки
        /// </summary>
        public List<Point3d> MiddlePoints { get; set; } = new List<Point3d>();

        /// <summary>Конечная точка</summary>
        public Point3d EndPoint { get; set; } = Point3d.Origin;

        // Получение управляющих точек в системе координат блока для отрисовки содержимого
        private Point3d InsertionPointOCS => InsertionPoint.TransformBy(BlockTransform.Inverse());

        private List<Point3d> MiddlePointsOCS
        {
            get
            {
                List<Point3d> points = new List<Point3d>();
                MiddlePoints.ForEach(p => points.Add(p.TransformBy(BlockTransform.Inverse())));
                return points;
            }
        }

        private Point3d EndPointOCS => EndPoint.TransformBy(BlockTransform.Inverse());
        
        #endregion

        #region Properties

        /// <summary>
        /// Минимальная длина линии грунта
        /// </summary>
        public double GroundLineMinLength => 20.0;

        /// <summary>
        /// Отступ первого штриха в каждом сегменте полилинии
        /// </summary>
        public GroundLineFirstStrokeOffset FirstStrokeOffset { get; set; } = GroundLineProperties.FirstStrokeOffset.DefaultValue;

        /// <summary>
        /// Длина штриха
        /// </summary>
        public int StrokeLength { get; set; } = GroundLineProperties.StrokeLength.DefaultValue;

        /// <summary>
        /// Расстояние между штрихами
        /// </summary>
        public int StrokeOffset { get; set; } = GroundLineProperties.StrokeOffset.DefaultValue;

        /// <summary>
        /// Угол наклона штриха в градусах
        /// </summary>
        public int StrokeAngle { get; set; } = GroundLineProperties.StrokeAngle.DefaultValue;

        /// <summary>
        /// Отступ группы штрихов
        /// </summary>
        public int Space { get; set; } = GroundLineProperties.Space.DefaultValue;

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

        #region Style

        /////// <summary>Идентификатор стиля</summary>
        ////public string StyleGuid { get; set; } = "00000000-0000-0000-0000-000000000000";

        public override void ApplyStyle(MPCOStyle style)
        {
            // apply settings from style
            FirstStrokeOffset = StyleHelpers.GetPropertyValue(style, nameof(FirstStrokeOffset), GroundLineProperties.FirstStrokeOffset.DefaultValue);
            StrokeLength = StyleHelpers.GetPropertyValue(style, nameof(StrokeLength), GroundLineProperties.StrokeLength.DefaultValue);
            StrokeOffset = StyleHelpers.GetPropertyValue(style, nameof(StrokeOffset), GroundLineProperties.StrokeOffset.DefaultValue);
            StrokeAngle = StyleHelpers.GetPropertyValue(style, nameof(StrokeAngle), GroundLineProperties.StrokeAngle.DefaultValue);
            Space = StyleHelpers.GetPropertyValue(style, nameof(Space), GroundLineProperties.Space.DefaultValue);
            // general
            Scale = MainStaticSettings.Settings.UseScaleFromStyle
                ? StyleHelpers.GetPropertyValue(style, nameof(Scale), GroundLineProperties.Scale.DefaultValue)
                : AcadHelpers.Database.Cannoscale;
            LineTypeScale = StyleHelpers.GetPropertyValue(style, nameof(LineTypeScale), GroundLineProperties.LineTypeScale.DefaultValue);
            // set layer
            var layerName = StyleHelpers.GetPropertyValue(style, GroundLineProperties.LayerName.Name, GroundLineProperties.LayerName.DefaultValue);
            AcadHelpers.SetLayerByName(BlockId, layerName, style.LayerXmlData);
            // set line type
            var lineType = StyleHelpers.GetPropertyValue(style, GroundLineProperties.LineType.Name, GroundLineProperties.LineType.DefaultValue);
            AcadHelpers.SetLineType(BlockId, lineType);
        }

        /////// <summary>Применение стиля по сути должно переопределять текущие параметры</summary>
        ////public override void ApplyStyle(GroundLineStyle style)
        ////{
        ////    // apply settings from style
        ////    FirstStrokeOffset = StyleHelpers.GetPropertyValue(style, nameof(FirstStrokeOffset), GroundLineProperties.FirstStrokeOffset.DefaultValue);
        ////    StrokeLength = StyleHelpers.GetPropertyValue(style, nameof(StrokeLength), GroundLineProperties.StrokeLength.DefaultValue);
        ////    StrokeOffset = StyleHelpers.GetPropertyValue(style, nameof(StrokeOffset), GroundLineProperties.StrokeOffset.DefaultValue);
        ////    StrokeAngle = StyleHelpers.GetPropertyValue(style, nameof(StrokeAngle), GroundLineProperties.StrokeAngle.DefaultValue);
        ////    Space = StyleHelpers.GetPropertyValue(style, nameof(Space), GroundLineProperties.Space.DefaultValue);
        ////    // general
        ////    Scale = MainStaticSettings.Settings.UseScaleFromStyle
        ////        ? StyleHelpers.GetPropertyValue(style, nameof(Scale), GroundLineProperties.Scale.DefaultValue)
        ////        : AcadHelpers.Database.Cannoscale;
        ////    LineTypeScale = StyleHelpers.GetPropertyValue(style, nameof(LineTypeScale), GroundLineProperties.LineTypeScale.DefaultValue);
        ////    // set layer
        ////    var layerName = StyleHelpers.GetPropertyValue(style, GroundLineProperties.LayerName.Name, GroundLineProperties.LayerName.DefaultValue);
        ////    AcadHelpers.SetLayerByName(BlockId, layerName, style.LayerXmlData);
        ////    // set line type
        ////    var lineType = StyleHelpers.GetPropertyValue(style, GroundLineProperties.LineType.Name, GroundLineProperties.LineType.DefaultValue);
        ////    AcadHelpers.SetLineType(BlockId, lineType);
        ////}

        #endregion

        public override ResultBuffer GetParametersForXData()
        {
            try
            {
                // ReSharper disable once UseObjectOrCollectionInitializer
                var resBuf = new ResultBuffer();
                // 1001 - DxfCode.ExtendedDataRegAppName. AppName
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, GroundLineFunction.MPCOEntName));

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
                                        FirstStrokeOffset = GroundLinePropertiesHelpers.GetFirstStrokeOffsetFromString(typedValue.Value.ToString());
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
        
        public static GroundLine GetGroundLineFromEntity(Entity ent)
        {
            using (ResultBuffer resBuf = ent.GetXDataForApplication(GroundLineFunction.MPCOEntName))
            {
                // В случае команды ОТМЕНА может вернуть null
                if (resBuf == null) return null;
                GroundLine groundLine = new GroundLine(ent.ObjectId);
                // Получаем параметры из самого блока
                // ОБЯЗАТЕЛЬНО СНАЧАЛА ИЗ БЛОКА!!!!!!
                groundLine.GetParametersFromEntity(ent);
                // Получаем параметры из XData
                groundLine.GetParametersFromResBuf(resBuf);

                return groundLine;
            }
        }
    }
}