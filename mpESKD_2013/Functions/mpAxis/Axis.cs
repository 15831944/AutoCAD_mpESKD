﻿// ReSharper disable InconsistentNaming
namespace mpESKD.Functions.mpAxis
{
    using System;
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using ModPlusAPI.Windows;

    [IntellectualEntityDisplayNameKey("h41")]
    public class Axis : IntellectualEntity
    {
        #region Constructors

        /// <inheritdoc />
        public Axis(ObjectId objectId) : base(objectId)
        {
        }

        public Axis()
        {
        }

        /// <summary>Инициализация экземпляра класса для BreakLine для создания</summary>
        public Axis(string lastHorizontalValue, string lastVerticalValue)
        {
            // last values
            LastHorizontalValue = lastHorizontalValue;
            LastVerticalValue = lastVerticalValue;
        }

        #endregion

        #region General Properties

        /// <summary>Минимальная длина от точки вставки до конечной точки</summary>
        public double AxisMinLength => 1.0;

        protected override void ProcessScaleChange(AnnotationScale oldScale, AnnotationScale newScale)
        {
            base.ProcessScaleChange(oldScale, newScale);
            if (oldScale != null && newScale != null)
            {
                if (MainStaticSettings.Settings.AxisLineTypeScaleProportionScale)
                {
                    LineTypeScale = LineTypeScale * newScale.GetNumericScale() / oldScale.GetNumericScale();
                }
            }
        }

        /// <inheritdoc />
        [EntityProperty(PropertiesCategory.General, 4, "p19", "d19", "осевая", null, null)]
        public override string LineType { get; set; } = "осевая";

        /// <inheritdoc />
        [EntityProperty(PropertiesCategory.General, 5, "p6", "d6", 10.0, 0.0, 1.0000E+99)]
        public override double LineTypeScale { get; set; } = 10;

        /// <inheritdoc />
        [EntityProperty(PropertiesCategory.Content, 1, "p17", "d17", "Standard", null, null)]
        public override string TextStyle { get; set; }

        #endregion

        #region Axis Properties

        private int _bottomFractureOffset;

        /// <summary>Положение маркеров</summary>
        [EntityProperty(PropertiesCategory.Geometry, 1, "p8", "d8", AxisMarkersPosition.Bottom, null, null)]
        [SaveToXData]
        public AxisMarkersPosition MarkersPosition { get; set; } = AxisMarkersPosition.Bottom;

        /// <summary>Излом</summary>
        [EntityProperty(PropertiesCategory.Geometry, 2, "p9", "d9", 10, 1, 20)]
        [PropertyNameKeyInStyleEditor("p9-1")]
        [SaveToXData]
        public int Fracture { get; set; } = 10;

        /// <summary>Нижний отступ излома</summary>
        [EntityProperty(PropertiesCategory.Geometry, 3, "p15", "d15", 0, 0, 30)]
        [PropertyNameKeyInStyleEditor("p15-1")]
        [SaveToXData]
        public int BottomFractureOffset
        {
            get => _bottomFractureOffset;
            set
            {
                var oldFracture = BottomFractureOffset;
                _bottomFractureOffset = value;

                // нужно сместить зависимые точки
                var vecNorm = (EndPoint - InsertionPoint).GetNormal() * (value - oldFracture) * GetScale();
                BottomOrientPoint = BottomOrientPoint + vecNorm;
            }
        }

        /// <summary>Верхний отступ излома</summary>
        [EntityProperty(PropertiesCategory.Geometry, 4, "p16", "d16", 0, 0, 30)]
        [PropertyNameKeyInStyleEditor("p16-1")]
        [SaveToXData]
        public int TopFractureOffset { get; set; } = 0;

        /// <summary>Диаметр маркеров</summary>
        [EntityProperty(PropertiesCategory.Geometry, 5, "p10", "d10", 10, 6, 12)]
        [PropertyNameKeyInStyleEditor("p10-1")]
        [SaveToXData]
        public int MarkersDiameter { get; set; } = 10;

        private int _markersCount = 1;

        /// <summary>Количество маркеров</summary>
        [EntityProperty(PropertiesCategory.Geometry, 6, "p11", "d11", 1, 1, 3)]
        [SaveToXData]
        public int MarkersCount
        {
            get => _markersCount;
            set
            {
                _markersCount = value;
                if (value == 1)
                {
                    SecondTextVisibility = false;
                    ThirdTextVisibility = false;
                }
                else if (value == 2)
                {
                    SecondTextVisibility = true;
                    ThirdTextVisibility = false;
                }
                else if (value == 3)
                {
                    SecondTextVisibility = true;
                    ThirdTextVisibility = true;
                }
            }
        }

        // Типы маркеров: Type 1 - один кружок, Type 2 - два кружка
        [EntityProperty(PropertiesCategory.Geometry, 7, "p12", "d12", AxisMarkerType.Type1, null, null)]
        [SaveToXData]
        public AxisMarkerType FirstMarkerType { get; set; } = AxisMarkerType.Type1;

        [EntityProperty(PropertiesCategory.Geometry, 8, "p13", "d13", AxisMarkerType.Type1, null, null)]
        [SaveToXData]
        public AxisMarkerType SecondMarkerType { get; set; } = AxisMarkerType.Type1;

        [EntityProperty(PropertiesCategory.Geometry, 8, "p14", "d14", AxisMarkerType.Type1, null, null)]
        [SaveToXData]
        public AxisMarkerType ThirdMarkerType { get; set; } = AxisMarkerType.Type1;

        // Orient markers

        private bool _bottomOrientMarkerVisible;

        /// <summary>Видимость нижнего бокового кружка</summary>
        [EntityProperty(PropertiesCategory.Geometry, 9, "p32", "d32", false, null, null)]
        [PropertyVisibilityDependency(new[] { nameof(BottomOrientText) })]
        [SaveToXData]
        public bool BottomOrientMarkerVisible
        {
            get => _bottomOrientMarkerVisible;
            set
            {
                _bottomOrientMarkerVisible = value;
                if (value)
                {
                    OrientMarkerVisibilityDependency = true;
                }
                else if (!TopOrientMarkerVisible)
                {
                    OrientMarkerVisibilityDependency = false;
                }
            }
        }

        private bool _topOrientMarkerVisible;

        /// <summary>Видимость верхнего бокового кружка</summary>
        [EntityProperty(PropertiesCategory.Geometry, 10, "p33", "d33", false, null, null)]
        [PropertyVisibilityDependency(new[] { nameof(TopOrientText) })]
        [SaveToXData]
        public bool TopOrientMarkerVisible
        {
            get => _topOrientMarkerVisible;
            set
            {
                _topOrientMarkerVisible = value;
                if (value)
                {
                    OrientMarkerVisibilityDependency = true;
                }
                else if (!BottomOrientMarkerVisible)
                {
                    OrientMarkerVisibilityDependency = false;
                }
            }
        }

        [EntityProperty(PropertiesCategory.Geometry, 10, "", "", "", null, null, PropertyScope.Hidden)]
        [PropertyVisibilityDependency(new[] { nameof(OrientMarkerType), nameof(ArrowsSize) })]
        [SaveToXData]
        public bool OrientMarkerVisibilityDependency { get; private set; }

        [EntityProperty(PropertiesCategory.Geometry, 11, "p34", "d34", AxisMarkerType.Type1, null, null)]
        [SaveToXData]
        public AxisMarkerType OrientMarkerType { get; set; } = AxisMarkerType.Type1;

        /// <summary>Размер стрелок</summary>
        [EntityProperty(PropertiesCategory.Geometry, 12, "p29", "d29", 3, 0, 10)]
        [PropertyNameKeyInStyleEditor("p29-1")]
        [SaveToXData]
        public int ArrowsSize { get; set; } = 3;

        // Отступы маркеров-ориентиров
        [SaveToXData]
        private double BottomOrientMarkerOffset { get; set; } = double.NaN;

        [SaveToXData]
        private double TopOrientMarkerOffset { get; set; } = double.NaN;

        // текст и текстовые значения
        [EntityProperty(PropertiesCategory.Content, 1, "p18", "d18", 3.5, 0.000000001, 1.0000E+99)]
        [PropertyNameKeyInStyleEditor("p18-1")]
        [SaveToXData]
        public double TextHeight { get; set; } = 3.5;

        [EntityProperty(PropertiesCategory.Content, 2, "p20", "d20", "", null, null, PropertyScope.Palette)]
        [SaveToXData]
        public string FirstTextPrefix { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 3, "p22", "d22", "", null, null, PropertyScope.Palette)]
        [SaveToXData]
        public string FirstText { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 4, "p21", "d21", "", null, null, PropertyScope.Palette)]
        [SaveToXData]
        public string FirstTextSuffix { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 4, "", "", "", null, null, PropertyScope.Hidden)]
        [PropertyVisibilityDependency(new[] { nameof(SecondText), nameof(SecondTextPrefix), nameof(SecondTextSuffix), nameof(SecondMarkerType) })]
        [SaveToXData]
        public bool SecondTextVisibility { get; set; }

        [EntityProperty(PropertiesCategory.Content, 5, "p23", "d23", "", null, null, PropertyScope.Palette)]
        [SaveToXData]
        public string SecondTextPrefix { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 6, "p25", "d25", "", null, null, PropertyScope.Palette)]
        [SaveToXData]
        public string SecondText { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 7, "p24", "d24", "", null, null, PropertyScope.Palette)]
        [SaveToXData]
        public string SecondTextSuffix { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 7, "", "", "", null, null, PropertyScope.Hidden)]
        [PropertyVisibilityDependency(new[] { nameof(ThirdText), nameof(ThirdTextPrefix), nameof(ThirdTextSuffix), nameof(ThirdMarkerType) })]
        [SaveToXData]
        public bool ThirdTextVisibility { get; set; }

        [EntityProperty(PropertiesCategory.Content, 8, "p26", "d26", "", null, null, PropertyScope.Palette)]
        [SaveToXData]
        public string ThirdTextPrefix { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 9, "p28", "d28", "", null, null, PropertyScope.Palette)]
        [SaveToXData]
        public string ThirdText { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 10, "p27", "d27", "", null, null, PropertyScope.Palette)]
        [SaveToXData]
        public string ThirdTextSuffix { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 11, "p30", "d30", "", null, null, PropertyScope.Palette)]
        [SaveToXData]
        public string BottomOrientText { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 12, "p31", "d31", "", null, null, PropertyScope.Palette)]
        [SaveToXData]
        public string TopOrientText { get; set; } = string.Empty;

        // last values
        private readonly string LastHorizontalValue = string.Empty;

        private readonly string LastVerticalValue = string.Empty;

        #endregion

        #region Geometry

        #region Points and Grips

        /// <summary>Средняя точка. Нужна для перемещения  примитива</summary>
        public Point3d MiddlePoint => new Point3d(
            (InsertionPoint.X + EndPoint.X) / 2,
            (InsertionPoint.Y + EndPoint.Y) / 2,
            (InsertionPoint.Z + EndPoint.Z) / 2);

        [SaveToXData]
        public double BottomLineAngle { get; set; }

        private Point3d _bottomMarkerPoint;

        /// <summary>Нижняя точка расположения маркеров</summary>  
        public Point3d BottomMarkerPoint
        {
            get
            {
                var baseVector = new Vector3d(1.0, 0.0, 0.0);
                var angleA = baseVector.GetAngleTo(EndPoint - InsertionPoint, Vector3d.ZAxis);
                var bottomLineLength = Fracture / Math.Cos(BottomLineAngle) * GetFullScale();
                _bottomMarkerPoint = new Point3d(
                    EndPoint.X + (bottomLineLength * Math.Cos(angleA + BottomLineAngle)),
                    EndPoint.Y + (bottomLineLength * Math.Sin(angleA + BottomLineAngle)),
                    EndPoint.Z);
                return _bottomMarkerPoint + ((EndPoint - InsertionPoint).GetNormal() * BottomFractureOffset * GetFullScale());
            }

            set
            {
                _bottomMarkerPoint = value;
                BottomLineAngle = (EndPoint - InsertionPoint).GetAngleTo(value - EndPoint - ((EndPoint - InsertionPoint).GetNormal() * BottomFractureOffset * GetFullScale()), Vector3d.ZAxis);
            }
        }

        [SaveToXData]
        public double TopLineAngle { get; set; }

        private Point3d _topMarkerPoint;

        /// <summary>Верхняя точка расположения маркеров</summary>
        public Point3d TopMarkerPoint
        {
            get
            {
                var baseVector = new Vector3d(1.0, 0.0, 0.0);
                var angleA = baseVector.GetAngleTo(InsertionPoint - EndPoint, Vector3d.ZAxis);
                var topLineLength = Fracture / Math.Cos(TopLineAngle) * GetFullScale();
                _topMarkerPoint = new Point3d(
                    InsertionPoint.X + (topLineLength * Math.Cos(angleA + TopLineAngle)),
                    InsertionPoint.Y + (topLineLength * Math.Sin(angleA + TopLineAngle)),
                    InsertionPoint.Z);
                return _topMarkerPoint + ((InsertionPoint - EndPoint).GetNormal() * TopFractureOffset * GetFullScale());
            }

            set
            {
                _topMarkerPoint = value;
                TopLineAngle = (InsertionPoint - EndPoint).GetAngleTo(value - InsertionPoint - ((InsertionPoint - EndPoint).GetNormal() * TopFractureOffset * GetFullScale()), Vector3d.ZAxis);
            }
        }

        /// <summary>Нижняя точка маркера ориентира</summary>
        public Point3d BottomOrientPoint
        {
            get
            {
                var mainLineVectorNormal = (EndPoint - InsertionPoint).GetPerpendicularVector().GetNormal();
                if (double.IsNaN(BottomOrientMarkerOffset) || Math.Abs(BottomOrientMarkerOffset) < 0.0001)
                {
                    BottomOrientMarkerOffset = MarkersDiameter + 10.0;
                }

                return BottomMarkerPoint + (mainLineVectorNormal * BottomOrientMarkerOffset * GetFullScale());
            }

            set
            {
                var mainLineVectorNormal = (EndPoint - InsertionPoint).GetPerpendicularVector().GetNormal();
                var vector = value - BottomMarkerPoint;
                BottomOrientMarkerOffset = mainLineVectorNormal.DotProduct(vector) / GetScale() / BlockTransform.GetScale();
            }
        }

        /// <summary>Верхняя точка маркера ориентации</summary>
        public Point3d TopOrientPoint
        {
            get
            {
                var mainLineVectorNormal = (InsertionPoint - EndPoint).GetPerpendicularVector().GetNormal();
                if (double.IsNaN(TopOrientMarkerOffset) || Math.Abs(TopOrientMarkerOffset) < 0.0001)
                {
                    TopOrientMarkerOffset = MarkersDiameter + 10.0;
                }

                return TopMarkerPoint - (mainLineVectorNormal * TopOrientMarkerOffset * GetFullScale());
            }

            set
            {
                var mainLineVectorNormal = (EndPoint - InsertionPoint).GetPerpendicularVector().GetNormal();
                var vector = value - TopMarkerPoint;
                TopOrientMarkerOffset = mainLineVectorNormal.DotProduct(vector) / GetScale() / BlockTransform.GetScale();
            }
        }

        // Получение управляющих точек в системе координат блока для отрисовки содержимого
        private Point3d BottomMarkerPointOCS => BottomMarkerPoint.TransformBy(BlockTransform.Inverse());

        private Point3d TopMarkerPointOCS => TopMarkerPoint.TransformBy(BlockTransform.Inverse());

        private Point3d BottomOrientPointOCS => BottomOrientPoint.TransformBy(BlockTransform.Inverse());

        private Point3d TopOrientPointOCS => TopOrientPoint.TransformBy(BlockTransform.Inverse());

        #endregion

        /// <summary>Установка свойств для однострочного текста</summary>
        private void SetPropertiesToDBText(DBText dbText)
        {
            dbText.Height = TextHeight * GetScale();
            dbText.HorizontalMode = TextHorizontalMode.TextCenter;
            dbText.Justify = AttachmentPoint.MiddleCenter;
            dbText.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
            dbText.Linetype = "ByBlock";
            dbText.LineWeight = LineWeight.ByBlock;
            dbText.TextStyleId = AcadHelpers.GetTextStyleIdByName(TextStyle);
        }

        /// <summary>Средняя (основная) линия оси</summary>
        private Line _mainLine;

        private Line _bottomOrientLine;

        private Line _topOrientLine;

        private Polyline _bottomOrientArrow;

        private Polyline _topOrientArrow;

        #region Fractures

        /// <summary>"Палочка" от конечной точки до кружка (маркера)</summary>
        private Line _bottomMarkerLine;

        /// <summary>Палочка отступа нижнего излома</summary>
        private Line _bottomFractureOffsetLine;

        /// <summary>Палочка отступа верхнего излома</summary>
        private Line _topFractureOffsetLine;

        /// <summary>Палочка от точки вставки до кружка (маркера)</summary>
        private Line _topMarkerLine;

        #endregion

        #region Circles

        #region Bottom

        private Circle _bottomFirstMarker;

        private Circle _bottomFirstMarkerType2;

        private Circle _bottomSecondMarker;

        private Circle _bottomSecondMarkerType2;

        private Circle _bottomThirdMarker;

        private Circle _bottomThirdMarkerType2;

        #endregion

        #region Top

        private Circle _topFirstMarker;

        private Circle _topFirstMarkerType2;

        private Circle _topSecondMarker;

        private Circle _topSecondMarkerType2;

        private Circle _topThirdMarker;

        private Circle _topThirdMarkerType2;

        #endregion

        #region Orient

        private Circle _bottomOrientMarker;

        private Circle _bottomOrientMarkerType2;

        private Circle _topOrientMarker;

        private Circle _topOrientMarkerType2;

        #endregion

        #endregion

        #region Texts

        private DBText _bottomFirstDBText;

        private DBText _topFirstDBText;

        private DBText _bottomSecondDBText;

        private DBText _topSecondDBText;

        private DBText _bottomThirdDBText;

        private DBText _topThirdDBText;

        private DBText _bottomOrientDBText;

        private DBText _topOrientDBText;

        #endregion

        public override IEnumerable<Entity> Entities
        {
            get
            {
                var entities = new List<Entity>
                {
                    _bottomOrientLine,
                    _topOrientLine,
                    _bottomOrientArrow,
                    _topOrientArrow,
                    _bottomMarkerLine,
                    _bottomFractureOffsetLine,
                    _topFractureOffsetLine,
                    _topMarkerLine,
                    _bottomFirstMarker,
                    _bottomFirstMarkerType2,
                    _bottomSecondMarker,
                    _bottomSecondMarkerType2,
                    _bottomThirdMarker,
                    _bottomThirdMarkerType2,
                    _topFirstMarker,
                    _topFirstMarkerType2,
                    _topSecondMarker,
                    _topSecondMarkerType2,
                    _topThirdMarker,
                    _topThirdMarkerType2,
                    _bottomOrientMarker,
                    _bottomOrientMarkerType2,
                    _topOrientMarker,
                    _topOrientMarkerType2,
                    _bottomFirstDBText,
                    _topFirstDBText,
                    _bottomSecondDBText,
                    _topSecondDBText,
                    _bottomThirdDBText,
                    _topThirdDBText,
                    _bottomOrientDBText,
                    _topOrientDBText
                };
                foreach (var e in entities)
                {
                    if (e != null && !(e is DBText))
                    {
                        SetImmutablePropertiesToNestedEntity(e);
                    }
                }

                if (_mainLine != null)
                {
                    SetChangeablePropertiesToNestedEntity(_mainLine);
                }

                entities.Add(_mainLine);

                return entities;
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

                    // Задание точки вставки (т.е. второй точки еще нет)
                    MakeSimplyEntity(UpdateVariant.SetInsertionPoint, scale);
                }
                else if (length < AxisMinLength * scale)
                {
                    // Задание второй точки - случай когда расстояние между точками меньше минимального
                    MakeSimplyEntity(UpdateVariant.SetEndPointMinLength, scale);
                }
                else
                {
                    // Задание второй точки
                    SetEntitiesPoints(InsertionPointOCS, EndPointOCS, BottomMarkerPointOCS, TopMarkerPointOCS, scale);
                }

                UpdateTextEntities();
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        /// <summary>
        /// Построение "базового" простого варианта ЕСКД примитива
        /// Тот вид, который висит на мышке при создании и указании точки вставки
        /// </summary>
        private void MakeSimplyEntity(UpdateVariant variant, double scale)
        {
            // Создание вершин полилинии
            if (variant == UpdateVariant.SetInsertionPoint)
            {
                /* Изменение базовых примитивов в момент указания второй точки при условии второй точки нет
                 * Примерно аналогично созданию, только точки не создаются, а меняются
                */
                var tmpEndPoint = new Point3d(InsertionPointOCS.X, InsertionPointOCS.Y - (AxisMinLength * scale), InsertionPointOCS.Z);
                var tmpBottomMarkerPoint = new Point3d(tmpEndPoint.X, tmpEndPoint.Y - (Fracture * scale), tmpEndPoint.Z);
                var tmpTopMarkerPoint = new Point3d(InsertionPointOCS.X, InsertionPointOCS.Y + (Fracture * scale), InsertionPointOCS.Z);

                SetEntitiesPoints(InsertionPointOCS, tmpEndPoint, tmpBottomMarkerPoint, tmpTopMarkerPoint, scale);
            }
            else if (variant == UpdateVariant.SetEndPointMinLength) // изменение вершин полилинии
            {
                /* Изменение базовых примитивов в момент указания второй точки
                * при условии что расстояние от второй точки до первой больше минимального допустимого
                */
                var tmpEndPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(InsertionPoint, EndPoint, InsertionPointOCS, AxisMinLength * scale);
                SetEntitiesPoints(InsertionPointOCS, tmpEndPoint, BottomMarkerPointOCS, TopMarkerPointOCS, scale);
                EndPoint = tmpEndPoint.TransformBy(BlockTransform);
            }
        }

        /// <summary>Изменение примитивов по точкам</summary>
        private void SetEntitiesPoints(Point3d insertionPoint, Point3d endPoint, Point3d bottomMarkerPoint, Point3d topMarkerPoint, double scale)
        {
            // main line
            _mainLine = new Line
            {
                StartPoint = insertionPoint,
                EndPoint = endPoint
            };
            var mainVector = endPoint - insertionPoint;

            #region Bottom
            if (MarkersPosition == AxisMarkersPosition.Both ||
                MarkersPosition == AxisMarkersPosition.Bottom)
            {
                var firstMarkerCenter = bottomMarkerPoint + (mainVector.GetNormal() * MarkersDiameter / 2 * scale);

                // bottom line
                var bottomLineStartPoint = endPoint + (mainVector.GetNormal() * BottomFractureOffset * scale);
                if (BottomFractureOffset > 0)
                {
                    _bottomFractureOffsetLine = new Line
                    {
                        StartPoint = endPoint,
                        EndPoint = bottomLineStartPoint
                    };
                }

                var markerLineVector = firstMarkerCenter - bottomLineStartPoint;
                _bottomMarkerLine = new Line
                {
                    StartPoint = bottomLineStartPoint,
                    EndPoint = bottomLineStartPoint + (markerLineVector.GetNormal() * (markerLineVector.Length - (MarkersDiameter * scale / 2.0)))
                };

                // markers
                _bottomFirstMarker = new Circle
                {
                    Center = firstMarkerCenter,
                    Diameter = MarkersDiameter * scale
                };

                // text
                if (!string.IsNullOrEmpty(FirstTextPrefix) ||
                    !string.IsNullOrEmpty(FirstText) ||
                    !string.IsNullOrEmpty(FirstTextSuffix))
                {
                    _bottomFirstDBText = new DBText();
                    SetPropertiesToDBText(_bottomFirstDBText);
                    _bottomFirstDBText.Position = firstMarkerCenter;
                    _bottomFirstDBText.AlignmentPoint = firstMarkerCenter;
                }

                // Второй кружок первого маркера
                if (FirstMarkerType == AxisMarkerType.Type2)
                {
                    _bottomFirstMarkerType2 = new Circle
                    {
                        Center = firstMarkerCenter,
                        Diameter = (MarkersDiameter - 2) * scale
                    };
                }

                // Если количество маркеров больше 1
                if (MarkersCount > 1)
                {
                    // Значит второй маркер точно есть (независимо от 3-го)
                    var secontMarkerCenter = firstMarkerCenter + (mainVector.GetNormal() * MarkersDiameter * scale);
                    _bottomSecondMarker = new Circle
                    {
                        Center = secontMarkerCenter,
                        Diameter = MarkersDiameter * scale
                    };

                    // text
                    if (!string.IsNullOrEmpty(SecondTextPrefix) ||
                        !string.IsNullOrEmpty(SecondText) ||
                        !string.IsNullOrEmpty(SecondTextSuffix))
                    {
                        _bottomSecondDBText = new DBText();
                        SetPropertiesToDBText(_bottomSecondDBText);
                        _bottomSecondDBText.Position = secontMarkerCenter;
                        _bottomSecondDBText.AlignmentPoint = secontMarkerCenter;
                    }

                    // второй кружок второго маркера
                    if (SecondMarkerType == AxisMarkerType.Type2)
                    {
                        _bottomSecondMarkerType2 = new Circle
                        {
                            Center = secontMarkerCenter,
                            Diameter = (MarkersDiameter - 2) * scale
                        };
                    }

                    // Если количество маркеров больше двух, тогда рисую 3-ий маркер
                    if (MarkersCount > 2)
                    {
                        var thirdMarkerCenter = secontMarkerCenter + (mainVector.GetNormal() * MarkersDiameter * scale);
                        _bottomThirdMarker = new Circle
                        {
                            Center = thirdMarkerCenter,
                            Diameter = MarkersDiameter * scale
                        };

                        // text
                        if (!string.IsNullOrEmpty(ThirdTextPrefix) ||
                            !string.IsNullOrEmpty(ThirdText) ||
                            !string.IsNullOrEmpty(ThirdTextSuffix))
                        {
                            _bottomThirdDBText = new DBText();
                            SetPropertiesToDBText(_bottomThirdDBText);
                            _bottomThirdDBText.Position = thirdMarkerCenter;
                            _bottomThirdDBText.AlignmentPoint = thirdMarkerCenter;
                        }

                        // второй кружок третьего маркера
                        if (ThirdMarkerType == AxisMarkerType.Type2)
                        {
                            _bottomThirdMarkerType2 = new Circle
                            {
                                Center = thirdMarkerCenter,
                                Diameter = (MarkersDiameter - 2) * scale
                            };
                        }
                    }
                }

                #region Orient marker

                if (BottomOrientMarkerVisible)
                {
                    var bottomOrientMarkerCenter = BottomOrientPointOCS + (mainVector.GetNormal() * MarkersDiameter / 2.0 * scale);
                    _bottomOrientMarker = new Circle
                    {
                        Center = bottomOrientMarkerCenter,
                        Diameter = MarkersDiameter * scale
                    };

                    // line
                    var _bottomOrientLineStartPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(
                        firstMarkerCenter, bottomOrientMarkerCenter, firstMarkerCenter,
                        MarkersDiameter / 2.0 * scale);
                    var _bottomOrientLineEndPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(
                        bottomOrientMarkerCenter, firstMarkerCenter, bottomOrientMarkerCenter,
                        MarkersDiameter / 2.0 * scale);
                    if (!_bottomOrientLineEndPoint.IsEqualTo(_bottomOrientLineStartPoint, Tolerance.Global))
                    {
                        _bottomOrientLine = new Line
                        {
                            StartPoint = _bottomOrientLineStartPoint,
                            EndPoint = _bottomOrientLineEndPoint
                        };

                        // arrow
                        if (!(Math.Abs((_bottomOrientLineEndPoint - _bottomOrientLineStartPoint).Length) < ArrowsSize * scale) &&
                            ArrowsSize != 0)
                        {
                            _bottomOrientArrow = new Polyline(2);
                            var arrowStartPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(
                                _bottomOrientLineEndPoint,
                                _bottomOrientLineStartPoint,
                                _bottomOrientLineEndPoint, ArrowsSize * scale);
                            _bottomOrientArrow.AddVertexAt(0, arrowStartPoint.ConvertPoint3dToPoint2d(), 0.0, ArrowsSize * scale * 1 / 3, 0.0);
                            _bottomOrientArrow.AddVertexAt(1, _bottomOrientLineEndPoint.ConvertPoint3dToPoint2d(), 0.0, 0.0, 0.0);
                        }
                    }

                    // text
                    if (!string.IsNullOrEmpty(BottomOrientText))
                    {
                        _bottomOrientDBText = new DBText();
                        SetPropertiesToDBText(_bottomOrientDBText);
                        _bottomOrientDBText.Position = bottomOrientMarkerCenter;
                        _bottomOrientDBText.AlignmentPoint = bottomOrientMarkerCenter;
                    }

                    // type2
                    if (OrientMarkerType == AxisMarkerType.Type2)
                    {
                        _bottomOrientMarkerType2 = new Circle
                        {
                            Center = bottomOrientMarkerCenter,
                            Diameter = (MarkersDiameter - 2) * scale
                        };
                    }
                }

                #endregion
            }
            #endregion

            #region Top
            if (MarkersPosition == AxisMarkersPosition.Both ||
                MarkersPosition == AxisMarkersPosition.Top)
            {
                var firstMarkerCenter = topMarkerPoint - (mainVector.GetNormal() * MarkersDiameter / 2 * scale);

                // top line
                var topLineStartPoint = insertionPoint - (mainVector.GetNormal() * TopFractureOffset * scale);
                if (TopFractureOffset > 0)
                {
                    _topFractureOffsetLine = new Line
                    {
                        StartPoint = insertionPoint,
                        EndPoint = topLineStartPoint
                    };
                }

                var markerLineVector = firstMarkerCenter - topLineStartPoint;
                _topMarkerLine = new Line
                {
                    StartPoint = topLineStartPoint,
                    EndPoint = topLineStartPoint + (markerLineVector.GetNormal() * (markerLineVector.Length - (MarkersDiameter * scale / 2.0)))
                };

                // markers
                _topFirstMarker = new Circle
                {
                    Center = firstMarkerCenter,
                    Diameter = MarkersDiameter * scale
                };

                // text
                if (!string.IsNullOrEmpty(FirstTextPrefix) ||
                    !string.IsNullOrEmpty(FirstText) ||
                    !string.IsNullOrEmpty(FirstTextSuffix))
                {
                    _topFirstDBText = new DBText();
                    SetPropertiesToDBText(_topFirstDBText);
                    _topFirstDBText.Position = firstMarkerCenter;
                    _topFirstDBText.AlignmentPoint = firstMarkerCenter;
                }

                // Второй кружок первого маркера
                if (FirstMarkerType == AxisMarkerType.Type2)
                {
                    _topFirstMarkerType2 = new Circle
                    {
                        Center = firstMarkerCenter,
                        Diameter = (MarkersDiameter - 2) * scale
                    };
                }

                // Если количество маркеров больше 1
                if (MarkersCount > 1)
                {
                    // Значит второй маркер точно есть (независимо от 3-го)
                    var secontMarkerCenter = firstMarkerCenter - (mainVector.GetNormal() * MarkersDiameter * scale);
                    _topSecondMarker = new Circle
                    {
                        Center = secontMarkerCenter,
                        Diameter = MarkersDiameter * scale
                    };

                    // text
                    if (!string.IsNullOrEmpty(SecondTextPrefix) ||
                        !string.IsNullOrEmpty(SecondText) ||
                        !string.IsNullOrEmpty(SecondTextSuffix))
                    {
                        _topSecondDBText = new DBText();
                        SetPropertiesToDBText(_topSecondDBText);
                        _topSecondDBText.Position = secontMarkerCenter;
                        _topSecondDBText.AlignmentPoint = secontMarkerCenter;
                    }

                    // второй кружок второго маркера
                    if (SecondMarkerType == AxisMarkerType.Type2)
                    {
                        _topSecondMarkerType2 = new Circle
                        {
                            Center = secontMarkerCenter,
                            Diameter = (MarkersDiameter - 2) * scale
                        };
                    }

                    // Если количество маркеров больше двух, тогда рисую 3-ий маркер
                    if (MarkersCount > 2)
                    {
                        var thirdMarkerCenter = secontMarkerCenter - (mainVector.GetNormal() * MarkersDiameter * scale);
                        _topThirdMarker = new Circle
                        {
                            Center = thirdMarkerCenter,
                            Diameter = MarkersDiameter * scale
                        };

                        // text
                        if (!string.IsNullOrEmpty(ThirdTextPrefix) ||
                            !string.IsNullOrEmpty(ThirdText) ||
                            !string.IsNullOrEmpty(ThirdTextSuffix))
                        {
                            _topThirdDBText = new DBText();
                            SetPropertiesToDBText(_topThirdDBText);
                            _topThirdDBText.Position = thirdMarkerCenter;
                            _topThirdDBText.AlignmentPoint = thirdMarkerCenter;
                        }

                        // второй кружок третьего маркера
                        if (ThirdMarkerType == AxisMarkerType.Type2)
                        {
                            _topThirdMarkerType2 = new Circle
                            {
                                Center = thirdMarkerCenter,
                                Diameter = (MarkersDiameter - 2) * scale
                            };
                        }
                    }
                }

                #region Orient marker

                if (TopOrientMarkerVisible)
                {
                    var topOrientMarkerCenter = TopOrientPointOCS - (mainVector.GetNormal() * MarkersDiameter / 2.0 * scale);
                    _topOrientMarker = new Circle
                    {
                        Center = topOrientMarkerCenter,
                        Diameter = MarkersDiameter * scale
                    };

                    // line
                    var _topOrientLineStartPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(
                        firstMarkerCenter, topOrientMarkerCenter, firstMarkerCenter,
                        MarkersDiameter / 2.0 * scale);
                    var _topOrientLineEndPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(
                        topOrientMarkerCenter, firstMarkerCenter, topOrientMarkerCenter,
                        MarkersDiameter / 2.0 * scale);
                    if (!_topOrientLineEndPoint.IsEqualTo(_topOrientLineStartPoint, Tolerance.Global))
                    {
                        _topOrientLine = new Line
                        {
                            StartPoint = _topOrientLineStartPoint,
                            EndPoint = _topOrientLineEndPoint
                        };

                        // arrow
                        if (!(Math.Abs((_topOrientLineEndPoint - _topOrientLineStartPoint).Length) < ArrowsSize * scale) &&
                            ArrowsSize != 0)
                        {
                            _topOrientArrow = new Polyline(2);

                            // arrow draw
                            var arrowStartPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(
                                _topOrientLineEndPoint,
                                _topOrientLineStartPoint,
                                _topOrientLineEndPoint, ArrowsSize * scale);
                            _topOrientArrow.AddVertexAt(0, arrowStartPoint.ConvertPoint3dToPoint2d(), 0.0, ArrowsSize * scale * 1 / 3, 0.0);
                            _topOrientArrow.AddVertexAt(1, _topOrientLineEndPoint.ConvertPoint3dToPoint2d(), 0.0, 0.0, 0.0);
                        }
                    }

                    // text
                    if (!string.IsNullOrEmpty(TopOrientText))
                    {
                        _topOrientDBText = new DBText();
                        SetPropertiesToDBText(_topOrientDBText);
                        _topOrientDBText.Position = topOrientMarkerCenter;
                        _topOrientDBText.AlignmentPoint = topOrientMarkerCenter;
                    }

                    // type2
                    if (OrientMarkerType == AxisMarkerType.Type2)
                    {
                        _topOrientMarkerType2 = new Circle
                        {
                            Center = topOrientMarkerCenter,
                            Diameter = (MarkersDiameter - 2) * scale
                        };
                    }
                }

                #endregion
            }
            #endregion
        }

        private void UpdateTextEntities()
        {
            SetFirstTextOnCreation();
            if (_bottomFirstDBText != null)
            {
                _bottomFirstDBText.TextString = FirstTextPrefix + FirstText + FirstTextSuffix;
            }

            if (_bottomSecondDBText != null)
            {
                _bottomSecondDBText.TextString = SecondTextPrefix + SecondText + SecondTextSuffix;
            }

            if (_bottomThirdDBText != null)
            {
                _bottomThirdDBText.TextString = ThirdTextPrefix + ThirdText + ThirdTextSuffix;
            }

            if (_topFirstDBText != null)
            {
                _topFirstDBText.TextString = FirstTextPrefix + FirstText + FirstTextSuffix;
            }

            if (_topSecondDBText != null)
            {
                _topSecondDBText.TextString = SecondTextPrefix + SecondText + SecondTextSuffix;
            }

            if (_topThirdDBText != null)
            {
                _topThirdDBText.TextString = ThirdTextPrefix + ThirdText + ThirdTextSuffix;
            }

            if (_bottomOrientDBText != null)
            {
                _bottomOrientDBText.TextString = BottomOrientText;
            }

            if (_topOrientDBText != null)
            {
                _topOrientDBText.TextString = TopOrientText;
            }
        }

        private void SetFirstTextOnCreation()
        {
            // if (EndPointOCS == Point3d.Origin)
            //    return;
            if (IsValueCreated)
            {
                var check = 1 / Math.Sqrt(2);
                var v = (EndPointOCS - InsertionPointOCS).GetNormal();
                if ((v.X > check || v.X < -check) && (v.Y < check || v.Y > -check))
                {
                    FirstText = GetFirstTextValueByLastAxis("Horizontal");
                }
                else
                {
                    FirstText = GetFirstTextValueByLastAxis("Vertical");
                }
            }
        }

        // Чтобы не вычислять каждый раз заново создам переменные
        private string _newVerticalMarkValue = string.Empty;

        private string _newHorizontalMarkValue = string.Empty;

        private string GetFirstTextValueByLastAxis(string direction)
        {
            if (direction.Equals("Horizontal"))
            {
                if (!string.IsNullOrEmpty(LastHorizontalValue))
                {
                    if (string.IsNullOrEmpty(_newHorizontalMarkValue))
                    {
                        if (int.TryParse(LastHorizontalValue, out int i))
                        {
                            _newHorizontalMarkValue = (i + 1).ToString();
                            return _newHorizontalMarkValue;
                        }

                        if (Invariables.AxisRusAlphabet.Contains(LastHorizontalValue))
                        {
                            var index = Invariables.AxisRusAlphabet.IndexOf(LastHorizontalValue);
                            if (index == Invariables.AxisRusAlphabet.Count - 1)
                            {
                                _newHorizontalMarkValue = Invariables.AxisRusAlphabet[0];
                                return _newHorizontalMarkValue;
                            }

                            _newHorizontalMarkValue = Invariables.AxisRusAlphabet[index + 1];
                            return _newHorizontalMarkValue;
                        }

                        _newHorizontalMarkValue = "А";
                        return _newHorizontalMarkValue;
                    }

                    return _newHorizontalMarkValue;
                }

                _newHorizontalMarkValue = "А";
                return _newHorizontalMarkValue;
            }

            if (direction.Equals("Vertical"))
            {
                if (!string.IsNullOrEmpty(LastVerticalValue))
                {
                    if (string.IsNullOrEmpty(_newVerticalMarkValue))
                    {
                        if (int.TryParse(LastVerticalValue, out int i))
                        {
                            _newVerticalMarkValue = (i + 1).ToString();
                            return _newVerticalMarkValue;
                        }

                        if (Invariables.AxisRusAlphabet.Contains(LastVerticalValue))
                        {
                            var index = Invariables.AxisRusAlphabet.IndexOf(LastVerticalValue);
                            if (index == Invariables.AxisRusAlphabet.Count - 1)
                            {
                                _newVerticalMarkValue = Invariables.AxisRusAlphabet[0];
                                return _newVerticalMarkValue;
                            }

                            _newVerticalMarkValue = Invariables.AxisRusAlphabet[index + 1];
                            return _newVerticalMarkValue;
                        }

                        _newVerticalMarkValue = "1";
                        return _newVerticalMarkValue;
                    }

                    return _newVerticalMarkValue;
                }

                _newVerticalMarkValue = "1";
                return _newVerticalMarkValue;
            }

            return string.Empty;
        }
        #endregion
    }
}