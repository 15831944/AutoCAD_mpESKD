﻿namespace mpESKD.Functions.mpLevelMark
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Base.Attributes;
    using Base.Enums;
    using Base.Utils;
    using ModPlusAPI.Windows;

    /// <summary>
    /// Отметка уровня
    /// </summary>
    [IntellectualEntityDisplayNameKey("h105")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "<Ожидание>")]
    public class LevelMark : IntellectualEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LevelMark"/> class.
        /// </summary>
        public LevelMark()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelMark"/> class.
        /// </summary>
        /// <param name="objectId">ObjectId анонимного блока, представляющего интеллектуальный объект</param>
        public LevelMark(ObjectId objectId)
            : base(objectId)
        {
        }
        
        /// <summary>
        /// Точка уровня (точка объекта измерения)
        /// </summary>
        [SaveToXData]
        public Point3d ObjectPoint { get; set; }

        /// <summary>
        /// Точка уровня в внутренней системе координат блока
        /// </summary>
        public Point3d ObjectPointOCS => ObjectPoint.TransformBy(BlockTransform.Inverse());

        /// <summary>
        /// Точка начала (со стороны объекта) нижней полки
        /// </summary>
        [SaveToXData]
        public Point3d BottomShelfStartPoint { get; set; }

        /// <summary>
        /// Точка начала (со стороны объекта) нижней полки в системе координат блока
        /// </summary>
        public Point3d BottomShelfStartPointOCS => BottomShelfStartPoint.TransformBy(BlockTransform.Inverse());

        /// <summary>
        /// Точка начала верхней полки. Задает высоту от нижней полки до верхней
        /// </summary>
        public Point3d ShelfPoint
        {
            get =>
                new Point3d(
                    EndPoint.X,
                    IsDownState
                        ? EndPoint.Y - (DistanceBetweenShelfs * GetFullScale())
                        : EndPoint.Y + (DistanceBetweenShelfs * GetFullScale()),
                    EndPoint.Z);
            set
            {
                var p1 = EndPoint;
                var p2 = value;
                var v = (p2 - p1).GetNormal();
                IsDownState = v.Y < 0;
                var minDistance =
                    (int)Math.Round(Math.Max(MainTextHeight, SecondTextHeight) + TextVerticalOffset, MidpointRounding.AwayFromZero);
                var distance = (int)(Math.Abs(p2.Y - p1.Y) / GetFullScale());
                DistanceBetweenShelfs = distance < minDistance ? minDistance : distance;
            }
        }

        /// <summary>
        /// Точка начала верхней полки в системе координат блока. Задает высоту от нижней полки до верхней
        /// </summary>
        public Point3d ShelfPointOCS => ShelfPoint.TransformBy(BlockTransform.Inverse());

        /// <inheritdoc/>
        [SaveToXData]
        public override Point3d EndPoint
        {
            get => base.EndPoint;
            set => base.EndPoint = LevelMarkJigState == mpLevelMark.LevelMarkJigState.ObjectPoint || LevelMarkJigState == null
                ? value
                : new Point3d(value.X, ObjectPoint.Y, value.Z);
        }
        
        /// <summary>
        /// Состояние Jig при создании высотной отметки
        /// </summary>
        public LevelMarkJigState? LevelMarkJigState { get; set; }

        /// <inheritdoc />
        public override double MinDistanceBetweenPoints
        {
            get
            {
                if (ObjectLine)
                    return 1.0;
                return BottomShelfLength;
            }
        }

        /// <inheritdoc />
        /// Не используется!
        public override string LineType { get; set; }

        /// <inheritdoc />
        /// Не используется!
        public override double LineTypeScale { get; set; }

        /// <inheritdoc/>
        [EntityProperty(PropertiesCategory.Content, 1, "p41", "Standard", descLocalKey: "d41-1")]
        [SaveToXData]
        public override string TextStyle { get; set; } = "Standard";

        private bool _objectLine;

        /// <summary>
        /// Линия объекта
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 1, "p55", false, descLocalKey: "d55")]
        [PropertyVisibilityDependency(new[] { nameof(ObjectLineOffset) }, new[] { nameof(BottomShelfLength) })]
        [SaveToXData]
        public bool ObjectLine
        {
            get => _objectLine;
            set
            {
                _objectLine = value;
                var horV = (EndPoint - ObjectPoint).GetNormal();
                BottomShelfStartPoint = value
                    ? ObjectPoint + (horV * ObjectLineOffset * GetFullScale())
                    : EndPoint - (horV * BottomShelfLength * GetFullScale());
            }
        }

        private int _objectLineOffset = 5;

        /// <summary>
        /// Отступ линии объекта
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 2, "p56", 5, 0, 20, descLocalKey: "d56", nameSymbol: "o1")]
        [SaveToXData]
        public int ObjectLineOffset
        {
            get => _objectLineOffset;
            set
            {
                _objectLineOffset = value;
                if (ObjectLine)
                {
                    var horV = (EndPoint - ObjectPoint).GetNormal();
                    BottomShelfStartPoint = ObjectPoint + (horV * value * GetFullScale());
                }
            }
        }

        /// <summary>
        /// Длина нижней полки
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 3, "p57", 10, 1, 20, descLocalKey: "d57", nameSymbol: "l2")]
        [SaveToXData]
        public int BottomShelfLength { get; set; } = 10;

        /// <summary>
        /// Выступ нижней полки
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 4, "p58", 2, 1, 5, descLocalKey: "d58", nameSymbol: "l3")]
        [SaveToXData]
        public int BottomShelfLedge { get; set; } = 2;

        /// <summary>
        /// Расстояние между полками
        /// </summary>
        [SaveToXData]
        public int DistanceBetweenShelfs { get; set; } = 6;

        /// <summary>
        /// Находится ли отметка уровня в положении "Низ" (т.е. TopShelf находится ниже BottomShelf)
        /// </summary>
        [SaveToXData]
        public bool IsDownState { get; set; }

        /// <summary>
        /// Высота стрелки
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 5, "p59", 3, 2, 4, nameSymbol: "a")]
        [SaveToXData]
        public int ArrowHeight { get; set; } = 3;

        /// <summary>
        /// Толщина стрелки
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 6, "p60", 0.5, 0.0, 2.0, nameSymbol: "t")]
        [SaveToXData]
        public double ArrowThickness { get; set; } = 0.5;

        /// <summary>
        /// Отступ текста
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 7, "p61", 1.0, 0.0, 3.0, nameSymbol: "o2")]
        [SaveToXData]
        public double TextIndent { get; set; } = 1.0;

        /// <summary>
        /// Вертикальный отступ текста
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 8, "p62", 1.0, 0.0, 3.0, nameSymbol: "v")]
        [SaveToXData]
        public double TextVerticalOffset { get; set; } = 1.0;

        /// <summary>
        /// Выступ полки
        /// </summary>
        [EntityProperty(PropertiesCategory.Geometry, 9, "p63", 1, 0, 3, descLocalKey: "d63", nameSymbol: "l1")]
        [SaveToXData]
        public int ShelfLedge { get; set; } = 1;

        /// <summary>
        /// Измеренное значение
        /// </summary>
        [EntityProperty(PropertiesCategory.Content, 2, "p65", null, isReadOnly: true, propertyScope: PropertyScope.Palette)]
        [SaveToXData]
        public double MeasuredValue { get; set; }

        /// <summary>
        /// Переопределение текста
        /// </summary>
        [EntityProperty(PropertiesCategory.Content, 3, "p66", "", propertyScope: PropertyScope.Palette)]
        [SaveToXData]
        public string OverrideValue { get; set; } = string.Empty;

        /// <summary>
        /// Показывать плюс
        /// </summary>
        [EntityProperty(PropertiesCategory.Content, 4, "p64", true, descLocalKey: "d64")]
        [SaveToXData]
        public bool ShowPlus { get; set; } = true;

        /// <summary>
        /// Точность
        /// </summary>
        [EntityProperty(PropertiesCategory.Content, 5, "p67", 3, 0, 5, descLocalKey: "d67")]
        [SaveToXData]
        public int Accuracy { get; set; } = 3;

        /// <summary>
        /// Примечание
        /// </summary>
        [EntityProperty(PropertiesCategory.Content, 6, "p68", "", propertyScope: PropertyScope.Palette)]
        [SaveToXData]
        public string Note { get; set; } = string.Empty;

        /// <summary>
        /// Высота текста
        /// </summary>
        [EntityProperty(PropertiesCategory.Content, 7, "p49", 3.5, 0.000000001, 1.0000E+99, nameSymbol: "h1")]
        [SaveToXData]
        public double MainTextHeight { get; set; } = 3.5;

        /// <summary>
        /// Высота малого текста
        /// </summary>
        [EntityProperty(PropertiesCategory.Content, 8, "p50", 2.5, 0.000000001, 1.0000E+99, nameSymbol: "h2")]
        [SaveToXData]
        public double SecondTextHeight { get; set; } = 2.5;

        /// <summary>
        /// Масштаб измерений
        /// </summary>
        [EntityProperty(PropertiesCategory.Content, 9, "p69", 1.0, 0.000001, 1000000, descLocalKey: "d69")]
        [SaveToXData]
        public double MeasurementScale { get; set; } = 1.0;
        
        /// <summary>
        /// Нижняя полка
        /// </summary>
        private Line _bottomShelfLine;

        /// <summary>
        /// Вертикальная линия между полками
        /// </summary>
        private Line _verticalLine;

        /// <summary>
        /// Верхняя полка
        /// </summary>
        private Line _topShelfLine;

        /// <summary>
        /// Стрелка
        /// </summary>
        private Polyline _arrowPolyline;

        /// <summary>
        /// Верхний (основной) текст
        /// </summary>
        private DBText _topDbText;

        /// <summary>
        /// Нижний (второстепенный) текст
        /// </summary>
        private DBText _bottomDbText;

        /// <inheritdoc />
        public override IEnumerable<Entity> Entities
        {
            get
            {
                var entities = new List<Entity>
                {
                    _bottomShelfLine,
                    _topShelfLine,
                    _verticalLine,
                    _arrowPolyline,
                    _topDbText,
                    _bottomDbText
                };

                foreach (var e in entities)
                {
                    if (e != null)
                    {
                        SetImmutablePropertiesToNestedEntity(e);
                    }
                }

                return entities;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<Point3d> GetPointsForOsnap()
        {
            yield return InsertionPoint;
            yield return ObjectPoint;
            yield return BottomShelfStartPoint;
            yield return EndPoint;
            yield return ShelfPoint;
        }

        /// <summary>
        /// Установка нового значения для точки стрелки с обработкой зависимых значений
        /// </summary>
        /// <param name="point3d">Новое значение точки стрелки</param>
        public void SetArrowPoint(Point3d point3d)
        {
            var horV = (EndPoint - ObjectPoint).GetNormal();
            EndPoint = point3d;
            ObjectPoint = new Point3d(
                ObjectPoint.X,
                EndPoint.Y,
                ObjectPoint.Z);

            if (ObjectLine)
            {
                BottomShelfStartPoint = ObjectPoint + (horV * ObjectLineOffset * GetFullScale());
            }
            else
            {
                BottomShelfStartPoint = EndPoint - (horV * BottomShelfLength * GetFullScale());
            }
        }

        /// <inheritdoc />
        protected override void ProcessScaleChange(AnnotationScale oldScale, AnnotationScale newScale)
        {
            base.ProcessScaleChange(oldScale, newScale);
            var horV = (EndPoint - ObjectPoint).GetNormal();

            if (ObjectLine)
            {
                BottomShelfStartPoint = ObjectPoint + (horV * ObjectLineOffset * GetFullScale());
            }
            else
            {
                BottomShelfStartPoint = EndPoint - (horV * BottomShelfLength * GetFullScale());
            }
        }

        /// <inheritdoc/>
        public override void UpdateEntities()
        {
            try
            {
                var scale = GetScale();

                //// Задание первой точки (точки вставки). Она же точка начала отсчета
                if (LevelMarkJigState == mpLevelMark.LevelMarkJigState.InsertionPoint)
                {
                    var tempEndPoint = new Point3d(
                        InsertionPointOCS.X + (BottomShelfLength * scale),
                        InsertionPointOCS.Y,
                        InsertionPointOCS.Z);
                    var tempShelfPoint = new Point3d(
                        tempEndPoint.X,
                        tempEndPoint.Y + (DistanceBetweenShelfs * scale),
                        tempEndPoint.Z);

                    AcadUtils.WriteMessageInDebug(
                        "Create when LevelMarkJigState == mpLevelMark.LevelMarkJigState.InsertionPoint");

                    BottomShelfStartPoint = InsertionPoint;
                    CreateEntities(
                        InsertionPointOCS, InsertionPointOCS, BottomShelfStartPointOCS, tempEndPoint, tempShelfPoint, scale);
                }
                //// Задание второй точки - точки уровня. При этом в jig устанавливается EndPoint, которая по завершении
                //// будет перемещена в ObjectPoint. Минимальные расстояния не учитываются
                else if (LevelMarkJigState == mpLevelMark.LevelMarkJigState.ObjectPoint)
                {
                    var tempEndPoint = new Point3d(
                        EndPointOCS.X + (BottomShelfLength * scale),
                        EndPointOCS.Y,
                        EndPointOCS.Z);
                    var tempShelfPoint = new Point3d(
                        tempEndPoint.X,
                        tempEndPoint.Y + (DistanceBetweenShelfs * scale),
                        tempEndPoint.Z);

                    AcadUtils.WriteMessageInDebug(
                        "Create when LevelMarkJigState == mpLevelMark.LevelMarkJigState.ObjectPoint");

                    BottomShelfStartPoint = EndPoint;
                    CreateEntities(
                        InsertionPointOCS, EndPointOCS, BottomShelfStartPointOCS, tempEndPoint, tempShelfPoint, scale);
                }
                //// Прочие случаи
                else
                {
                    //// Если указывается EndPoint (она же точка начала стрелки) и расстояние до ObjectPoint меньше допустимого
                    if (EndPointOCS.DistanceTo(ObjectPointOCS) < MinDistanceBetweenPoints * scale)
                    {
                        var isLeft = EndPointOCS.X < ObjectPointOCS.X;

                        var tempEndPoint = new Point3d(
                            isLeft
                            ? ObjectPointOCS.X - (MinDistanceBetweenPoints * scale)
                            : ObjectPointOCS.X + (MinDistanceBetweenPoints * scale),
                            ObjectPointOCS.Y,
                            ObjectPointOCS.Z);
                        var tempShelfPoint = new Point3d(
                            tempEndPoint.X,
                            tempEndPoint.Y + (DistanceBetweenShelfs * scale),
                            tempEndPoint.Z);

                        AcadUtils.WriteMessageInDebug(
                            "Create when EndPointOCS.DistanceTo(ObjectPointOCS) < MinDistanceBetweenPoints * scale");

                        BottomShelfStartPoint = ObjectPoint;
                        CreateEntities(
                            InsertionPointOCS, ObjectPointOCS, BottomShelfStartPointOCS, tempEndPoint, tempShelfPoint, scale);
                    }
                    else if (LevelMarkJigState == mpLevelMark.LevelMarkJigState.EndPoint)
                    {
                        var isLeft = EndPointOCS.X < ObjectPointOCS.X;

                        var tempBottomShelfStartPoint = ObjectLine
                            ? new Point3d(
                                isLeft
                                ? ObjectPointOCS.X - (ObjectLineOffset * scale)
                                : ObjectPointOCS.X + (ObjectLineOffset * scale),
                                ObjectPointOCS.Y,
                                ObjectPointOCS.Z)
                            : new Point3d(
                                isLeft
                                ? EndPointOCS.X + (BottomShelfLength * scale)
                                : EndPointOCS.X - (BottomShelfLength * scale),
                                EndPointOCS.Y,
                                EndPointOCS.Z);
                        var tempShelfPoint = new Point3d(
                            EndPointOCS.X,
                            EndPointOCS.Y + (DistanceBetweenShelfs * scale),
                            EndPointOCS.Z);

                        AcadUtils.WriteMessageInDebug(
                            "Create when LevelMarkJigState == mpLevelMark.LevelMarkJigState.EndPoint");

                        BottomShelfStartPoint = tempBottomShelfStartPoint.TransformBy(BlockTransform);
                        CreateEntities(
                            InsertionPointOCS, ObjectPointOCS, tempBottomShelfStartPoint, EndPointOCS, tempShelfPoint, scale);
                    }
                    else
                    {
                        AcadUtils.WriteMessageInDebug("Create other variant");
                        CreateEntities(
                            InsertionPointOCS, ObjectPointOCS, BottomShelfStartPointOCS, EndPointOCS, ShelfPointOCS, scale);
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        private void CreateEntities(
            Point3d insertionPoint,
            Point3d objectPoint,
            Point3d bottomShelfStartPoint,
            Point3d arrowPoint,
            Point3d shelfPoint,
            double scale)
        {
            var horV = (arrowPoint - objectPoint).GetNormal();
            var verV = (shelfPoint - arrowPoint).GetNormal();
            var isLeft = horV.X < 0;
            var isTop = verV.Y > 0;

            var mainTextHeight = MainTextHeight * scale;
            var secondTextHeight = SecondTextHeight * scale;
            var textIndent = TextIndent * scale;

            if (ObjectLine)
            {
                _bottomShelfLine = new Line(
                    objectPoint + (horV * ObjectLineOffset * scale),
                    arrowPoint + (horV * BottomShelfLedge * scale));
            }
            else
            {
                _bottomShelfLine = new Line(
                    bottomShelfStartPoint,
                    bottomShelfStartPoint + (horV * (BottomShelfLength + BottomShelfLedge) * scale));
            }

            _verticalLine = new Line(arrowPoint, shelfPoint);
            _arrowPolyline = GetArrow(objectPoint, arrowPoint, shelfPoint, scale);

            var topTextPosition = isTop
                ? shelfPoint + (TextVerticalOffset * scale * verV) + (textIndent * horV)
                : shelfPoint - (TextVerticalOffset * scale * verV) + (textIndent * horV);
            var bottomTextPosition = isTop
                ? shelfPoint - (TextVerticalOffset * scale * verV) + (textIndent * horV)
                : shelfPoint + (TextVerticalOffset * scale * verV) + (textIndent * horV);

            _topDbText = new DBText
            {
                TextString = GetMainTextContent(insertionPoint, objectPoint),
                Position = topTextPosition
            };

            _bottomDbText = new DBText
            {
                TextString = Note,
                Position = bottomTextPosition
            };

            if (isLeft)
            {
                _topDbText.SetPropertiesToDbText(
                    TextStyle, mainTextHeight, TextHorizontalMode.TextRight, attachmentPoint: AttachmentPoint.BottomRight);
                _topDbText.AlignmentPoint = topTextPosition;

                _bottomDbText.SetPropertiesToDbText(
                    TextStyle, secondTextHeight, TextHorizontalMode.TextRight, TextVerticalMode.TextBottom, AttachmentPoint.TopRight);
                _bottomDbText.AlignmentPoint = bottomTextPosition;
            }
            else
            {
                _topDbText.SetPropertiesToDbText(TextStyle, mainTextHeight);
                _bottomDbText.SetPropertiesToDbText(
                    TextStyle, secondTextHeight, TextHorizontalMode.TextLeft, TextVerticalMode.TextBottom, AttachmentPoint.TopLeft);
                _bottomDbText.AlignmentPoint = bottomTextPosition;
            }

            // верхний текст всегда имеет содержимое
            var topTextLength =
                Math.Abs(_topDbText.GeometricExtents.MaxPoint.X - _topDbText.GeometricExtents.MinPoint.X);
            AcadUtils.WriteMessageInDebug($"Top Text Length: {topTextLength}");
            var bottomTextLength = !string.IsNullOrEmpty(Note)
                ? Math.Abs(_bottomDbText.GeometricExtents.MaxPoint.X - _bottomDbText.GeometricExtents.MinPoint.X)
                : double.NaN;

            var maxTextWidth = double.IsNaN(bottomTextLength)
                ? topTextLength
                : Math.Max(topTextLength, bottomTextLength);
            AcadUtils.WriteMessageInDebug($"Max text width: {maxTextWidth}");
            var topShelfLength = textIndent + maxTextWidth + (ShelfLedge * scale);

            _topShelfLine = new Line(shelfPoint, shelfPoint + (topShelfLength * horV));
        }

        private Polyline GetArrow(Point3d objectPoint, Point3d endPoint, Point3d shelfPoint, double scale)
        {
            var width = ArrowThickness * scale;
            var height = ArrowHeight * scale;
            var angle = 45.DegreeToRadian();
            var wingLength = height / Math.Sin(angle);
            var verV = (shelfPoint - endPoint).GetNormal();
            var horV = (endPoint - objectPoint).GetNormal();
            var wingProjection = wingLength * Math.Cos(angle);

            var polyline = new Polyline(3);
            var pt2 = endPoint + (width / 2 / Math.Sin(angle) * verV);
            var pt1 = pt2 - (wingProjection * horV) + (height * verV);
            var pt3 = pt1 + (wingProjection * 2 * horV);
            polyline.AddVertexAt(0, pt1.ConvertPoint3dToPoint2d(), 0.0, width, width);
            polyline.AddVertexAt(0, pt2.ConvertPoint3dToPoint2d(), 0.0, width, width);
            polyline.AddVertexAt(0, pt3.ConvertPoint3dToPoint2d(), 0.0, width, width);
            return polyline;
        }

        private string GetMainTextContent(Point3d insertionPoint, Point3d objectPoint)
        {
            if (!string.IsNullOrWhiteSpace(OverrideValue))
                return OverrideValue;

            MeasuredValue = (objectPoint.Y - insertionPoint.Y) * MeasurementScale;

            if (MeasuredValue >= 0)
            {
                var plus = ShowPlus ? "+" : string.Empty;
                return $"{plus}{Math.Round(MeasuredValue, Accuracy).ToString($"F{Accuracy}")}";
            }

            return $"{Math.Round(MeasuredValue, Accuracy).ToString($"F{Accuracy}")}";
        }
    }
}
