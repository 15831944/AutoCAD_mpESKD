﻿// ReSharper disable InconsistentNaming
namespace mpESKD.Functions.mpGroundLine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using ModPlusAPI.Windows;

    [IntellectualEntityDisplayNameKey("h73")]
    public class GroundLine : IntellectualEntity
    {
        #region Constructor

        /// <inheritdoc />
        public GroundLine(ObjectId objectId) : base(objectId)
        {
        }

        public GroundLine()
        {
        }

        #endregion

        #region Points

        /// <summary>
        /// Промежуточные точки
        /// </summary>
        [SaveToXData]
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
        [EntityProperty(PropertiesCategory.Geometry, 1, "p36", "d36",
            GroundLineFirstStrokeOffset.ByHalfSpace, null, null)]
        [PropertyNameKeyInStyleEditor("p36-1")]
        [SaveToXData]
        public GroundLineFirstStrokeOffset FirstStrokeOffset { get; set; } = GroundLineFirstStrokeOffset.ByHalfSpace;

        /// <summary>
        /// Длина штриха
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 2, "p37", "d37", 8, 1, 10)]
        [PropertyNameKeyInStyleEditor("p37-1")]
        [SaveToXData]
        public int StrokeLength { get; set; } = 8;

        /// <summary>
        /// Расстояние между штрихами
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 3, "p38", "d38", 4, 1, 10)]
        [PropertyNameKeyInStyleEditor("p38-1")]
        [SaveToXData]
        public int StrokeOffset { get; set; } = 4;

        /// <summary>
        /// Угол наклона штриха в градусах
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 4, "p39", "d39", 60, 30, 90)]
        [PropertyNameKeyInStyleEditor("p39-1")]
        [SaveToXData]
        public int StrokeAngle { get; set; } = 60;

        /// <summary>
        /// Отступ группы штрихов
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 5, "p40", "d40", 10, 1, 20)]
        [PropertyNameKeyInStyleEditor("p40-1")]
        [SaveToXData]
        public int Space { get; set; } = 10;

        /// <inheritdoc />
        [EntityProperty(PropertiesCategory.General, 4, "p35", "d35", "Continuous", null, null)]
        public override string LineType { get; set; }

        /// <inheritdoc />
        [EntityProperty(PropertiesCategory.General, 5, "p6", "d6", 1.0, 0.0, 1.0000E+99)]
        public override double LineTypeScale { get; set; }

        /// <inheritdoc />
        public override string TextStyle { get; set; }

        #endregion

        #region Geometry

        /// <summary>
        /// Главная полилиния примитива
        /// </summary>
        private Polyline _mainPolyline;
        
        /// <summary>
        /// Список штрихов
        /// </summary>
        private readonly List<Line> _strokes = new List<Line>();

        /// <inheritdoc />
        public override IEnumerable<Entity> Entities
        {
            get
            {
                var entities = new List<Entity> { _mainPolyline };
                entities.AddRange(_strokes);
                foreach (var e in entities)
                    if (e != null)
                        SetPropertiesToCadEntity(e);
                return entities;
            }
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
            _mainPolyline = new Polyline(points.Count);
            SetPropertiesToCadEntity(_mainPolyline);
            for (var i = 0; i < points.Count; i++)
            {
                _mainPolyline.AddVertexAt(i, points[i], 0.0, 0.0, 0.0);
            }

            // create strokes
            _strokes.Clear();
            if (_mainPolyline.Length >= GroundLineMinLength)
            {
                for (var i = 1; i < _mainPolyline.NumberOfVertices; i++)
                {
                    var previousPoint = _mainPolyline.GetPoint3dAt(i - 1);
                    var currentPoint = _mainPolyline.GetPoint3dAt(i);
                    _strokes.AddRange(CreateStrokesOnMainPolylineSegment(currentPoint, previousPoint, scale));
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
            double distanceAtSegmentStart = _mainPolyline.GetDistAtPoint(previousPoint);

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

                var firstStrokePoint = _mainPolyline.GetPointAtDist(distanceAtSegmentStart + summDistanceAtSegment);
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

        private static Point2dCollection GetPointsForMainPolyline(Point3d insertionPoint, List<Point3d> middlePoints, Point3d endPoint)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var points = new Point2dCollection();

            points.Add(insertionPoint.ConvertPoint3dToPoint2d());
            middlePoints.ForEach(p => points.Add(p.ConvertPoint3dToPoint2d()));
            points.Add(endPoint.ConvertPoint3dToPoint2d());

            return points;
        }

        #endregion
    }
}