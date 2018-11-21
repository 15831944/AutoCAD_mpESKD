// ReSharper disable InconsistentNaming
namespace mpESKD.Functions.mpSection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using ModPlusAPI.Windows;

    public class Section : IntellectualEntity
    {
        #region Constructors

        public Section()
        {
        }

        /// <inheritdoc />
        public Section(ObjectId objectId) : base(objectId)
        {
        }

        #endregion

        #region Points and Grips

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

        /// <inheritdoc />
        /// В примитиве не используется!
        public override string LineType { get; set; }

        /// <inheritdoc />
        /// В примитиве не используется!
        public override double LineTypeScale { get; set; }

        /// <inheritdoc />
        /// todo translate
        [EntityProperty(PropertiesCategory.Content, 1, "", "", "Standard", null, null)]
        [SaveToXData]
        public override string TextStyle { get; set; }

        /// <summary>
        /// Минимальная длина
        /// </summary>
        public double SectionMinLength => 0.2;

        /// <summary>
        /// Длина среднего штриха (половина длины полилинии на переломе)
        /// </summary>
        /// todo translate
        [EntityProperty(PropertiesCategory.Geometry, 1, "", "", 8, 1, 20)]
        [SaveToXData]
        public int MiddleStrokeLength { get; set; } = 8;

        /// <summary>
        /// Толщина штрихов
        /// </summary>
        /// todo translate
        [EntityProperty(PropertiesCategory.Geometry, 2, "", "", 0.5, 0, 2)]
        [SaveToXData]
        public double StrokeWidth { get; set; } = 0.5;

        //todo добавить ли эти свойства в палитру и стиль?

        /// <summary>
        /// Длина верхнего и нижнего штриха
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 3, "", "", 10, 5, 10)]
        [SaveToXData]
        public int StrokeLength { get; set; } = 10;

        /// <summary>
        /// Отступ полки по длине штриха в процентах
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 4, "", "", 80, 0, 100)]
        [SaveToXData]
        public int ShelfOffset { get; set; } = 80;

        /// <summary>
        /// Длина полки
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 5, "", "", 10, 5, 15)]
        [SaveToXData]
        public int ShelfLength { get; set; } = 10;

        /// <summary>
        /// Длина стрелки
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 6, "", "", 5, 1, 8)]
        [SaveToXData]
        public int ShelfArrowLength { get; set; } = 5;

        /// <summary>
        /// Толщина стрелки
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 7, "", "", 1.5, 0.1, 5)]
        [SaveToXData]
        public double ShelfArrowWidth { get; set; } = 1.5;

        #endregion

        #region Geometry

        /// <summary>
        /// Средние штрихи - штрихи, создаваемые в средних точках
        /// </summary>
        public List<Polyline> MiddleStrokes { get; } = new List<Polyline>();

        private readonly Lazy<Line> _topShelfLine = new Lazy<Line>(() => new Line());

        /// <summary>
        /// Верхняя полка
        /// </summary>
        public Line TopShelfLine
        {
            get
            {
                SetPropertiesToCadEntity(_topShelfLine.Value);
                return _topShelfLine.Value;
            }
        }

        private readonly Lazy<Polyline> _topShelfArrow = new Lazy<Polyline>(() =>
        {
            // Это нужно, чтобы не выводилось сообщение в командную строку
            var p = new Polyline();
            p.AddVertexAt(0, Point2d.Origin, 0.0, 0.0, 0.0);
            p.AddVertexAt(1, Point2d.Origin, 0.0, 0.0, 0.0);
            return p;
        });

        /// <summary>
        /// Стрелка верхней полки
        /// </summary>
        public Polyline TopShelfArrow
        {
            get
            {
                SetPropertiesToCadEntity(_topShelfArrow.Value);
                //todo add width
                return _topShelfArrow.Value;
            }
        }

        private readonly Lazy<Polyline> _topStroke = new Lazy<Polyline>(() =>
        {
            // Это нужно, чтобы не выводилось сообщение в командную строку
            var p = new Polyline();
            p.AddVertexAt(0, Point2d.Origin, 0.0, 0.0, 0.0);
            p.AddVertexAt(1, Point2d.Origin, 0.0, 0.0, 0.0);
            return p;
        });

        /// <summary>
        /// Верхний штрих
        /// </summary>
        public Polyline TopStroke
        {
            get
            {
                SetPropertiesToCadEntity(_topStroke.Value);
                var width = StrokeWidth * GetScale();
                _topStroke.Value.SetStartWidthAt(0, width);
                _topStroke.Value.SetEndWidthAt(0, width);
                _topStroke.Value.SetStartWidthAt(1, width);
                _topStroke.Value.SetEndWidthAt(1, width);
                return _topStroke.Value;
            }
        }

        private readonly Lazy<Line> _bottomShelfLine = new Lazy<Line>(() => new Line());

        /// <summary>
        /// Нижняя полка
        /// </summary>
        public Line BottomShelfLine
        {
            get
            {
                SetPropertiesToCadEntity(_bottomShelfLine.Value);
                return _bottomShelfLine.Value;
            }
        }

        private readonly Lazy<Polyline> _bottomShelfArrow = new Lazy<Polyline>(() =>
        {
            // Это нужно, чтобы не выводилось сообщение в командную строку
            var p = new Polyline();
            p.AddVertexAt(0, Point2d.Origin, 0.0, 0.0, 0.0);
            p.AddVertexAt(1, Point2d.Origin, 0.0, 0.0, 0.0);
            return p;
        });

        /// <summary>
        /// Стрелка нижней полки
        /// </summary>
        public Polyline BottomShelfArrow
        {
            get
            {
                SetPropertiesToCadEntity(_bottomShelfArrow.Value);
                //todo add width
                return _bottomShelfArrow.Value;
            }
        }

        private readonly Lazy<Polyline> _bottomStroke = new Lazy<Polyline>(() =>
        {
            // Это нужно, чтобы не выводилось сообщение в командную строку
            var p = new Polyline();
            p.AddVertexAt(0, Point2d.Origin, 0.0, 0.0, 0.0);
            p.AddVertexAt(1, Point2d.Origin, 0.0, 0.0, 0.0);
            return p;
        });

        /// <summary>
        /// Нижний штрих
        /// </summary>
        public Polyline BottomStroke
        {
            get
            {
                SetPropertiesToCadEntity(_bottomStroke.Value);
                var width = StrokeWidth * GetScale();
                _bottomStroke.Value.SetStartWidthAt(0, width);
                _bottomStroke.Value.SetEndWidthAt(0, width);
                _bottomStroke.Value.SetStartWidthAt(1, width);
                _bottomStroke.Value.SetEndWidthAt(1, width);
                return _bottomStroke.Value;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<Entity> Entities
        {
            get
            {
                yield return TopShelfLine;
                yield return TopShelfArrow;
                yield return TopStroke;
                yield return BottomShelfLine;
                yield return BottomShelfArrow;
                yield return BottomStroke;
                foreach (var s in MiddleStrokes)
                {
                    yield return s;
                }
            }
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
                else if (length < SectionMinLength * scale && MiddlePoints.Count == 0)
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

        /// <summary>
        /// Перестроение точек - помещение EndPoint в список
        /// </summary>
        public void RebasePoints()
        {
            if (!MiddlePoints.Contains(EndPoint))
                MiddlePoints.Add(EndPoint);
        }

        private void MakeSimplyEntity(UpdateVariant variant, double scale)
        {
            if (variant == UpdateVariant.SetInsertionPoint)
            {
                /* Изменение базовых примитивов в момент указания второй точки при условии второй точки нет
                 * Примерно аналогично созданию, только точки не создаются, а меняются
                */
                var tmpEndPoint = new Point3d(InsertionPointOCS.X, InsertionPointOCS.Y - SectionMinLength * scale, InsertionPointOCS.Z);
                CreateEntities(InsertionPointOCS, MiddlePointsOCS, tmpEndPoint, scale);
            }
            else if (variant == UpdateVariant.SetEndPointMinLength)
            {
                /* Изменение базовых примитивов в момент указания второй точки
                * при условии что расстояние от второй точки до первой больше минимального допустимого
                */
                var tmpEndPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(InsertionPoint, EndPoint, InsertionPointOCS, SectionMinLength * scale);
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
            // top and bottom strokes
            var topStrokeEndPoint = GetTopStrokeEndPoint(insertionPoint, endPoint, middlePoints, scale);
            var bottomStrokeEndPoint = GetBottomStrokeEndPoint(insertionPoint, endPoint, middlePoints, scale);

            _topStroke.Value.SetPointAt(0, topStrokeEndPoint.ConvertPoint3dToPoint2d());
            _topStroke.Value.SetPointAt(1, insertionPoint.ConvertPoint3dToPoint2d());

            _bottomStroke.Value.SetPointAt(0, endPoint.ConvertPoint3dToPoint2d());
            _bottomStroke.Value.SetPointAt(1, bottomStrokeEndPoint.ConvertPoint3dToPoint2d());

            var topStrokeNormalVector = (topStrokeEndPoint - insertionPoint).GetNormal();
            var bottomStrokeNormalVector = (bottomStrokeEndPoint - endPoint).GetNormal();

            // shelf lines
            var topShelfFirstPoint = insertionPoint + topStrokeNormalVector * GetShelfOffset() * scale;
            var topShelfSecondPoint = topShelfFirstPoint + topStrokeNormalVector.GetPerpendicularVector() * ShelfLength * scale;
            _topShelfLine.Value.StartPoint = topShelfFirstPoint;
            _topShelfLine.Value.EndPoint = topShelfSecondPoint;

            var bottomShelfFirstPoint = endPoint + bottomStrokeNormalVector * GetShelfOffset() * scale;
            var bottomShelfSecondPoint = bottomShelfFirstPoint + bottomStrokeNormalVector.GetPerpendicularVector().Negate() * ShelfLength * scale;
            _bottomShelfLine.Value.StartPoint = bottomShelfFirstPoint;
            _bottomShelfLine.Value.EndPoint = bottomShelfSecondPoint;

            // shelf arrows
            var topShelfArrowStartPoint = topShelfFirstPoint + topStrokeNormalVector.GetPerpendicularVector() * ShelfArrowLength * scale;
            _topShelfArrow.Value.SetPointAt(0, topShelfArrowStartPoint.ConvertPoint3dToPoint2d());
            _topShelfArrow.Value.SetPointAt(1, topShelfFirstPoint.ConvertPoint3dToPoint2d());
            _topShelfArrow.Value.SetStartWidthAt(0, ShelfArrowWidth * scale);

            var bottomShelfArrowStartPoint =
                bottomShelfFirstPoint + bottomStrokeNormalVector.GetPerpendicularVector().Negate() * ShelfArrowLength * scale;
            _bottomShelfArrow.Value.SetPointAt(0, bottomShelfArrowStartPoint.ConvertPoint3dToPoint2d());
            _bottomShelfArrow.Value.SetPointAt(1, bottomShelfFirstPoint.ConvertPoint3dToPoint2d());
            _bottomShelfArrow.Value.SetStartWidthAt(0, ShelfArrowWidth * scale);

            MiddleStrokes.Clear();
            // middle strokes
            if (MiddlePoints.Any())
            {
                var strokesWidth = StrokeWidth * scale;
                var middleStrokeLength = MiddleStrokeLength * scale;

                var points = new List<Point3d> { insertionPoint };
                points.AddRange(middlePoints);
                points.Add(endPoint);

                for (var i = 1; i < points.Count - 1; i++)
                {
                    var previousPoint = points[i - 1];
                    var currentPoint = points[i];
                    var nextPoint = points[i + 1];

                    var middleStrokePolyline = new Polyline(3);
                    middleStrokePolyline.AddVertexAt(0,
                        (currentPoint + (previousPoint - currentPoint).GetNormal() * middleStrokeLength).ConvertPoint3dToPoint2d(),
                        0, strokesWidth, strokesWidth);
                    middleStrokePolyline.AddVertexAt(1, currentPoint.ConvertPoint3dToPoint2d(), 0, strokesWidth, strokesWidth);
                    middleStrokePolyline.AddVertexAt(2,
                        (currentPoint + (nextPoint - currentPoint).GetNormal() * middleStrokeLength).ConvertPoint3dToPoint2d(),
                        0, strokesWidth, strokesWidth);

                    MiddleStrokes.Add(middleStrokePolyline);
                }
            }
        }

        private double GetShelfOffset()
        {
            return StrokeLength * ShelfOffset / 100.0;
        }

        private Point3d GetBottomStrokeEndPoint(Point3d insertionPoint, Point3d endPoint, List<Point3d> middlePoints, double scale)
        {
            if (MiddlePoints.Any())
                return endPoint + (endPoint - middlePoints.Last()).GetNormal() * StrokeLength * scale;
            return endPoint + (endPoint - insertionPoint).GetNormal() * StrokeLength * scale;
        }

        private Point3d GetTopStrokeEndPoint(Point3d insertionPoint, Point3d endPoint, List<Point3d> middlePoints, double scale)
        {
            if (MiddlePoints.Any())
                return insertionPoint + (insertionPoint - middlePoints.First()).GetNormal() * StrokeLength * scale;
            return insertionPoint + (insertionPoint - endPoint).GetNormal() * StrokeLength * scale;
        }

        #endregion
    }
}
