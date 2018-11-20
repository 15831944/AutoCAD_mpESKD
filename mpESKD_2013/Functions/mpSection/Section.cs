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
        private int _strokeLength = 10;

        /// <summary>
        /// Отступ полки по длине штриха
        /// </summary>
        private int _shelfOffset = 8;

        /// <summary>
        /// Длина полки
        /// </summary>
        private int _shelfLength = 10;

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
                //todo check width
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
            var topStrokeEndPoint = GetTopStrokeEndPoint(insertionPoint, endPoint, middlePoints, scale);
            var bottomStrokeEndPoint = GetBottomStrokeEndPoint(insertionPoint, endPoint, middlePoints, scale);

            _topStroke.Value.SetPointAt(0, topStrokeEndPoint.ConvertPoint3dToPoint2d());
            _topStroke.Value.SetPointAt(1, insertionPoint.ConvertPoint3dToPoint2d());

            _bottomStroke.Value.SetPointAt(0, endPoint.ConvertPoint3dToPoint2d());
            _bottomStroke.Value.SetPointAt(1, bottomStrokeEndPoint.ConvertPoint3dToPoint2d());

            var topStrokeNormalVector = (topStrokeEndPoint - insertionPoint).GetNormal();
            var bottomStrokeNormalVector = (bottomStrokeEndPoint - endPoint).GetNormal();
            //todo to properties?
            var topShelfFirstPoint = insertionPoint + topStrokeNormalVector * _shelfOffset * scale;
            var topShelfSecondPoint = topShelfFirstPoint + topStrokeNormalVector.GetPerpendicularVector() * _shelfLength * scale;
            _topShelfLine.Value.StartPoint = topShelfFirstPoint;
            _topShelfLine.Value.EndPoint = topShelfSecondPoint;

            var bottomShelfFirstPoint = endPoint + bottomStrokeNormalVector * _shelfOffset * scale;
            var bottomShelfSecondPoint = bottomShelfFirstPoint + bottomStrokeNormalVector.GetPerpendicularVector().Negate() * _shelfLength * scale;
            _bottomShelfLine.Value.StartPoint = bottomShelfFirstPoint;
            _bottomShelfLine.Value.EndPoint = bottomShelfSecondPoint;

            if (MiddlePoints.Any())
            {
                MiddleStrokes.Clear();
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

        private Point3d GetBottomStrokeEndPoint(Point3d insertionPoint, Point3d endPoint, List<Point3d> middlePoints, double scale)
        {
            if (MiddlePoints.Any())
                return endPoint + (endPoint - middlePoints.Last()).GetNormal() * _strokeLength * scale;
            return endPoint + (endPoint - insertionPoint).GetNormal() * _strokeLength * scale;
        }

        private Point3d GetTopStrokeEndPoint(Point3d insertionPoint, Point3d endPoint, List<Point3d> middlePoints, double scale)
        {
            if (MiddlePoints.Any())
                return insertionPoint + (insertionPoint - middlePoints.First()).GetNormal() * _strokeLength * scale;
            return insertionPoint + (insertionPoint - endPoint).GetNormal() * _strokeLength * scale;
        }

        #endregion

        //todo remove after test
        ///// <inheritdoc />
        //public override ResultBuffer GetParametersForXData()
        //{
        //    // При сохранении свойств типа Enum, лучше сохранять их как int
        //    try
        //    {
        //        // ReSharper disable once UseObjectOrCollectionInitializer
        //        var resBuf = new ResultBuffer();
        //        // 1001 - DxfCode.ExtendedDataRegAppName. AppName
        //        resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, SectionInterface.Name));
        //        // 1010
        //        // Векторы от средних точек до начальной точки
        //        foreach (Point3d middlePointOCS in MiddlePointsOCS)
        //        {
        //            var vector = middlePointOCS - InsertionPointOCS;
        //            resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataXCoordinate, new Point3d(vector.X, vector.Y, vector.Z)));
        //        }

        //        // Вектор от конечной точки до начальной с учетом масштаба блока и трансформацией блока
        //        {
        //            var vector = EndPointOCS - InsertionPointOCS;
        //            resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataXCoordinate, new Point3d(vector.X, vector.Y, vector.Z)));
        //        }
        //        // Текстовые значения (код 1000)
        //        // Стиль
        //        resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, StyleGuid)); // 0
        //        // scale
        //        resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, Scale.Name)); // 1

        //        // Целочисленные значения (код 1070)
        //        resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, MiddleStrokeLength)); // 0

        //        // Значения типа double (dxfCode 1040)
        //        resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, LineTypeScale)); // 0
        //        resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, StrokeWidth)); // 1

        //        return resBuf;
        //    }
        //    catch (Exception exception)
        //    {
        //        ExceptionBox.Show(exception);
        //        return null;
        //    }
        //}

        ///// <inheritdoc />
        //public override void GetParametersFromResBuf(ResultBuffer resBuf)
        //{
        //    try
        //    {
        //        TypedValue[] resBufArr = resBuf.AsArray();
        //        /* indexes
        //         * Для каждого значения с повторяющимся кодом назначен свой индекc (см. метод GetParametersForXData)
        //         */
        //        List<Point3d> middleAndEndPoints = new List<Point3d>();
        //        var index1000 = 0;
        //        var index1040 = 0;
        //        var index1070 = 0;
        //        foreach (TypedValue typedValue in resBufArr)
        //        {
        //            switch ((DxfCode)typedValue.TypeCode)
        //            {
        //                case DxfCode.ExtendedDataXCoordinate:
        //                    {
        //                        // Получаем вектор от точки до первой в системе координат блока
        //                        var vectorFromPointToInsertion = ((Point3d)typedValue.Value).GetAsVector();
        //                        // получаем точку в мировой системе координат
        //                        var point = (InsertionPointOCS + vectorFromPointToInsertion).TransformBy(BlockTransform);
        //                        middleAndEndPoints.Add(point);
        //                        break;
        //                    }
        //                case DxfCode.ExtendedDataAsciiString:
        //                    {
        //                        switch (index1000)
        //                        {
        //                            case 0:
        //                                StyleGuid = typedValue.Value.ToString();
        //                                break;
        //                            case 1:
        //                                Scale = AcadHelpers.GetAnnotationScaleByName(typedValue.Value.ToString());
        //                                break;
        //                        }
        //                        // index
        //                        index1000++;
        //                        break;
        //                    }
        //                case DxfCode.ExtendedDataInteger16:
        //                    {
        //                        switch (index1070)
        //                        {
        //                            case 0:
        //                                MiddleStrokeLength = (Int16)typedValue.Value;
        //                                break;
        //                        }
        //                        //index
        //                        index1070++;
        //                        break;
        //                    }
        //                case DxfCode.ExtendedDataReal:
        //                    {
        //                        if (index1040 == 0) // 0 - LineTypeScale
        //                            LineTypeScale = (double)typedValue.Value;
        //                        if (index1040 == 1) 
        //                            StrokeWidth = (double)typedValue.Value;
        //                        // index
        //                        index1040++;
        //                        break;
        //                    }
        //            }
        //        }

        //        // rebase points
        //        if (middleAndEndPoints.Any())
        //        {
        //            EndPoint = middleAndEndPoints.Last();
        //            MiddlePoints.Clear();
        //            for (var i = 0; i < middleAndEndPoints.Count - 1; i++)
        //            {
        //                MiddlePoints.Add(middleAndEndPoints[i]);
        //            }
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        ExceptionBox.Show(exception);
        //    }
        //}
    }
}
