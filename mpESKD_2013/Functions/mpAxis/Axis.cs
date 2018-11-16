// ReSharper disable InconsistentNaming
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
    using Properties;
    using ModPlusAPI.Windows;

    [IntellectualEntityDisplayNameKeyAttribute("h41")]
    public class Axis : IntellectualEntity
    {
        #region Constructors

        /// <summary>Инициализация экземпляра класса для Axis без заполнения данными
        /// В данном случае уже все данные получены и нужно только "построить" 
        /// базовые примитивы</summary>
        public Axis(ObjectId objectId)
        {
            BlockId = objectId;
        }
        /// <summary>Инициализация экземпляра класса для BreakLine для создания</summary>
        public Axis(string lastHorizontalValue, string lastVerticalValue)
        {
            var blockTableRecord = new BlockTableRecord
            {
                Name = "*U",
                BlockScaling = BlockScaling.Uniform
            };
            BlockRecord = blockTableRecord;

            // last values
            LastHorizontalValue = lastHorizontalValue;
            LastVerticalValue = lastVerticalValue;
        }

        #endregion

        #region General Properties

        /// <summary>Минимальная длина от точки вставки до конечной точки</summary>
        public double AxisMinLength => 1.0;

        private AnnotationScale _scale;

        [EntityProperty(PropertiesCategory.General, 3, nameof(Scale), "p5", "d5", "1:1", null, null)]
        public new AnnotationScale Scale
        {
            get
            {
                if (_scale != null)
                    return _scale;
                _scale = new AnnotationScale { Name = "1:1", DrawingUnits = 1, PaperUnits = 1};
                return _scale;
            }
            set
            {
                var oldScale = GetScale();
                _scale = value;
                if (MainStaticSettings.Settings.AxisLineTypeScaleProportionScale)
                {
                    var newScale = GetScale();
                    LineTypeScale = LineTypeScale * newScale / oldScale;
                }
            }
        }

        /// <inheritdoc />
        [EntityProperty(PropertiesCategory.General, 4, nameof(LineType), "p19", "d19", "осевая", null, null)]
        public override string LineType { get; set; } = "осевая";

        /// <inheritdoc />
        [EntityProperty(PropertiesCategory.General, 5, nameof(LineTypeScale), "p6", "d6", 1.0, 0.0, 1.0000E+99)]
        public override double LineTypeScale { get; set; }

        /// <inheritdoc />
        [EntityProperty(PropertiesCategory.Content, 1, nameof(TextStyle), "p17", "d17", "Standard", null, null)]
        public override string TextStyle { get; set; }

        #endregion

        #region Axis Properties

        private int _bottomFractureOffset = 0;

        /// <summary>Положение маркеров</summary>
        [EntityProperty(PropertiesCategory.Geometry, 1, nameof(MarkersPosition), "p8", "d8", AxisMarkersPosition.Bottom, null, null)]
        public AxisMarkersPosition MarkersPosition { get; set; } = AxisMarkersPosition.Bottom;

        /// <summary>Излом</summary>
        [EntityProperty(PropertiesCategory.Geometry, 2, nameof(Fracture), "p9", "d9", 10, 1, 20)]
        public int Fracture { get; set; } = 10;

        /// <summary>Нижний отступ излома</summary>
        [EntityProperty(PropertiesCategory.Geometry, 3, nameof(BottomFractureOffset), "p15", "d15", 0, 0, 30)]
        public int BottomFractureOffset
        {
            get => _bottomFractureOffset;
            set
            {
                //todo check it
                var oldFracture = BottomFractureOffset;
                _bottomFractureOffset = value;
                // нужно сместить зависимые точки
                var vecNorm = (EndPoint - InsertionPoint).GetNormal() * (value - oldFracture) * GetScale();
                BottomOrientPoint = BottomOrientPoint + vecNorm;
            }
        }

        /// <summary>Верхний отступ излома</summary>
        [EntityProperty(PropertiesCategory.Geometry, 4, nameof(TopFractureOffset), "p16", "d16", 0, 0, 30)]
        public int TopFractureOffset { get; set; } = 0;

        /// <summary>Диаметр маркеров</summary>
        [EntityProperty(PropertiesCategory.Geometry, 5, nameof(MarkersDiameter), "p10", "d10", 10, 6, 12)]
        public int MarkersDiameter { get; set; } = 10;

        /// <summary>Количество маркеров</summary>
        [EntityProperty(PropertiesCategory.Geometry, 6, nameof(MarkersCount), "p11", "d11", 1, 1, 3)]
        public int MarkersCount { get; set; } = 1;

        // Типы маркеров: Type 1 - один кружок, Type 2 - два кружка
        [EntityProperty(PropertiesCategory.Geometry, 7, nameof(FirstMarkerType), "p12", "d12", AxisMarkerType.Type1, null, null)]
        public AxisMarkerType FirstMarkerType { get; set; } = AxisMarkerType.Type1;

        [EntityProperty(PropertiesCategory.Geometry, 8, nameof(SecondMarkerType), "p13", "d13", AxisMarkerType.Type1, null, null)]
        public AxisMarkerType SecondMarkerType { get; set; } = AxisMarkerType.Type1;

        [EntityProperty(PropertiesCategory.Geometry, 8, nameof(ThirdMarkerType), "p14", "d14", AxisMarkerType.Type1, null, null)]
        public AxisMarkerType ThirdMarkerType { get; set; } = AxisMarkerType.Type1;

        // Orient markers

        /// <summary>Видимость нижнего бокового кружка</summary>
        [EntityProperty(PropertiesCategory.Geometry, 9, nameof(BottomOrientMarkerVisible), "p32", "d32", false, null, null)]
        public bool BottomOrientMarkerVisible { get; set; }

        /// <summary>Видимость верхнего бокового кружка</summary>
        [EntityProperty(PropertiesCategory.Geometry, 10, nameof(TopOrientMarkerVisible), "p33", "d33", false, null, null)]
        public bool TopOrientMarkerVisible { get; set; }

        [EntityProperty(PropertiesCategory.Geometry, 11, nameof(OrientMarkerType), "p34", "d34", AxisMarkerType.Type1, null, null)]
        //todo visibility
        public AxisMarkerType OrientMarkerType { get; set; } = AxisMarkerType.Type1;

        /// <summary>Размер стрелок</summary>
        [EntityProperty(PropertiesCategory.Geometry, 12, nameof(ArrowsSize), "p29", "d29", 3, 0, 10)]
        //todo visibility
        public int ArrowsSize { get; set; } = 3;

        // Отступы маркеров-ориентиров
        private double BottomOrientMarkerOffset { get; set; } = double.NaN;

        private double TopOrientMarkerOffset { get; set; } = double.NaN;

        //todo visibility
        // текст и текстовые значения
        [EntityProperty(PropertiesCategory.Content, 1, nameof(TextHeight), "p18", "d18", 3.5, 0.000000001, 1.0000E+99)]
        public double TextHeight { get; set; } = 3.5;

        [EntityProperty(PropertiesCategory.Content, 2, nameof(FirstTextPrefix), "p20", "d20", "", null, null, PropertyScope.Palette)]
        public string FirstTextPrefix { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 3, nameof(FirstText), "p22", "d22", "", null, null, PropertyScope.Palette)]
        public string FirstText { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 4, nameof(FirstTextSuffix), "p21", "d21", "", null, null, PropertyScope.Palette)]
        public string FirstTextSuffix { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 5, nameof(SecondTextPrefix), "p23", "d23", "", null, null, PropertyScope.Palette)]
        public string SecondTextPrefix { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 6, nameof(SecondText), "p25", "d25", "", null, null, PropertyScope.Palette)]
        public string SecondText { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 7, nameof(SecondTextSuffix), "p24", "d24", "", null, null, PropertyScope.Palette)]
        public string SecondTextSuffix { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 8, nameof(ThirdTextPrefix), "p26", "d26", "", null, null, PropertyScope.Palette)]
        public string ThirdTextPrefix { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 9, nameof(ThirdText), "p28", "d28", "", null, null, PropertyScope.Palette)]
        public string ThirdText { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 10, nameof(ThirdTextSuffix), "p27", "d27", "", null, null, PropertyScope.Palette)]
        public string ThirdTextSuffix { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 11, nameof(BottomOrientText), "p30", "d30", "", null, null, PropertyScope.Palette)]
        public string BottomOrientText { get; set; } = string.Empty;

        [EntityProperty(PropertiesCategory.Content, 12, nameof(TopOrientText), "p31", "d31", "", null, null, PropertyScope.Palette)]
        public string TopOrientText { get; set; } = string.Empty;

        // last values
        private readonly string LastHorizontalValue = string.Empty;

        private readonly string LastVerticalValue = string.Empty;

        #endregion

        #region Geometry

        #region Points and Grips

        /// <summary>Средняя точка. Нужна для перемещения  примитива</summary>
        public Point3d MiddlePoint => new Point3d
        (
            (InsertionPoint.X + EndPoint.X) / 2,
            (InsertionPoint.Y + EndPoint.Y) / 2,
            (InsertionPoint.Z + EndPoint.Z) / 2
        );
        
        public double BottomLineAngle { get; set; } = 0.0;

        private Point3d _bottomMarkerPoint;
        /// <summary>Нижняя точка расположения маркеров</summary>  
        public Point3d BottomMarkerPoint
        {
            get
            {
                var baseVector = new Vector3d(1.0, 0.0, 0.0);
                var angleA = baseVector.GetAngleTo(EndPoint - InsertionPoint, Vector3d.ZAxis);
                var bottomLineLength = Fracture / Math.Cos(BottomLineAngle) * GetScale() * BlockTransform.GetScale();
                _bottomMarkerPoint = new Point3d(
                    EndPoint.X + bottomLineLength * Math.Cos(angleA + BottomLineAngle),
                    EndPoint.Y + bottomLineLength * Math.Sin(angleA + BottomLineAngle),
                    EndPoint.Z);
                return _bottomMarkerPoint + (EndPoint - InsertionPoint).GetNormal() * BottomFractureOffset * GetScale() * BlockTransform.GetScale();
            }
            set
            {
                _bottomMarkerPoint = value;
                BottomLineAngle = (EndPoint - InsertionPoint).GetAngleTo(value - EndPoint - (EndPoint - InsertionPoint).GetNormal() * BottomFractureOffset * GetScale() * BlockTransform.GetScale(), Vector3d.ZAxis);
            }
        }

        public double TopLineAngle { get; set; } = 0.0;

        private Point3d _topMarkerPoint;
        /// <summary>Верхняя точка расположения маркеров</summary>
        public Point3d TopMarkerPoint
        {
            get
            {
                var baseVector = new Vector3d(1.0, 0.0, 0.0);
                var angleA = baseVector.GetAngleTo(InsertionPoint - EndPoint, Vector3d.ZAxis);
                var topLineLength = Fracture / Math.Cos(TopLineAngle) * GetScale() * BlockTransform.GetScale();
                _topMarkerPoint = new Point3d(
                    InsertionPoint.X + topLineLength * Math.Cos(angleA + TopLineAngle),
                    InsertionPoint.Y + topLineLength * Math.Sin(angleA + TopLineAngle),
                    InsertionPoint.Z);
                return _topMarkerPoint + (InsertionPoint - EndPoint).GetNormal() * TopFractureOffset * GetScale() * BlockTransform.GetScale();
            }
            set
            {
                _topMarkerPoint = value;
                TopLineAngle = (InsertionPoint - EndPoint).GetAngleTo(value - InsertionPoint - (InsertionPoint - EndPoint).GetNormal() * TopFractureOffset * GetScale() * BlockTransform.GetScale(), Vector3d.ZAxis);
            }
        }
        /// <summary>Нижняя точка маркера ориентира</summary>
        public Point3d BottomOrientPoint
        {
            get
            {
                var mainLineVectorNormal = (EndPoint - InsertionPoint).GetPerpendicularVector().GetNormal();
                if (double.IsNaN(BottomOrientMarkerOffset))
                    BottomOrientMarkerOffset = MarkersDiameter + 10.0;
                return BottomMarkerPoint + mainLineVectorNormal * BottomOrientMarkerOffset * GetScale() * BlockTransform.GetScale();
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
                if (double.IsNaN(TopOrientMarkerOffset))
                    TopOrientMarkerOffset = MarkersDiameter + 10.0;
                return TopMarkerPoint - mainLineVectorNormal * TopOrientMarkerOffset * GetScale() * BlockTransform.GetScale();
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

        /// <summary>Установка свойств для примитивов, которые не меняются</summary>
        /// <param name="entity">Примитив автокада</param>
        private static void SetPropertiesToCadEntity(Entity entity)
        {
            entity.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
            entity.LineWeight = LineWeight.ByBlock;
            entity.Linetype = "Continuous";
            entity.LinetypeScale = 1.0;
        }

        /// <summary>Установка свойств для однострочного текста</summary>
        /// <param name="dbText"></param>
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
        private readonly Lazy<Line> _mainLine = new Lazy<Line>(() => new Line());

        /// <summary>Средняя (основная) линия оси</summary>
        public Line MainLine
        {
            get
            {
                _mainLine.Value.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
                _mainLine.Value.LineWeight = LineWeight.ByBlock;
                _mainLine.Value.Linetype = "ByBlock";
                _mainLine.Value.LinetypeScale = LineTypeScale;
                return _mainLine.Value;
            }
        }

        private readonly Lazy<Line> _bottomOrientLine = new Lazy<Line>(() => new Line());
        public Line BottomOrientLine
        {
            get
            {
                SetPropertiesToCadEntity(_bottomOrientLine.Value);
                return _bottomOrientLine.Value;
            }
        }

        private readonly Lazy<Line> _topOrientLine = new Lazy<Line>(() => new Line());
        public Line TopOrientLine
        {
            get
            {
                SetPropertiesToCadEntity(_topOrientLine.Value);
                return _topOrientLine.Value;
            }
        }

        private readonly Lazy<Polyline> _bottomOrientArrow = new Lazy<Polyline>(() =>
        {
            // Это нужно, чтобы не выводилось сообщение в командную строку
            var p = new Polyline();
            p.AddVertexAt(0, Point2d.Origin, 0.0, 0.0, 0.0);
            p.AddVertexAt(1, Point2d.Origin, 0.0, 0.0, 0.0);
            return p;
        });
        public Polyline BottomOrientArrow
        {
            get
            {
                SetPropertiesToCadEntity(_bottomOrientArrow.Value);
                return _bottomOrientArrow.Value;
            }
        }

        private readonly Lazy<Polyline> _topOrientArrow = new Lazy<Polyline>(() =>
        {
            // Это нужно, чтобы не выводилось сообщение в командную строку
            var p = new Polyline();
            p.AddVertexAt(0, Point2d.Origin, 0.0, 0.0, 0.0);
            p.AddVertexAt(1, Point2d.Origin, 0.0, 0.0, 0.0);
            return p;
        });
        public Polyline TopOrientArrow
        {
            get
            {
                SetPropertiesToCadEntity(_topOrientArrow.Value);
                return _topOrientArrow.Value;
            }
        }

        #region Fractures

        private readonly Lazy<Line> _bottomMarkerLine = new Lazy<Line>(() => new Line());
        /// <summary>"Палочка" от конечной точки до кружка (маркера)</summary>
        public Line BottomMarkerLine
        {
            get
            {
                SetPropertiesToCadEntity(_bottomMarkerLine.Value);
                return _bottomMarkerLine.Value;
            }
        }
        private readonly Lazy<Line> _bottomFractureOffsetLine = new Lazy<Line>(() => new Line());
        /// <summary>Палочка отступа нижнего излома</summary>
        public Line BottomFractureOffsetLine
        {
            get
            {
                SetPropertiesToCadEntity(_bottomFractureOffsetLine.Value);
                return _bottomFractureOffsetLine.Value;
            }
        }
        private readonly Lazy<Line> _topFractureOffsetLine = new Lazy<Line>(() => new Line());
        /// <summary>Палочка отступа верхнего излома</summary>
        public Line TopFractureOffsetLine
        {
            get
            {
                SetPropertiesToCadEntity(_topFractureOffsetLine.Value);
                return _topFractureOffsetLine.Value;
            }
        }
        private readonly Lazy<Line> _topMarkerLine = new Lazy<Line>(() => new Line());
        /// <summary>Палочка от точки вставки до кружка (маркера)</summary>
        public Line TopMarkerLine
        {
            get
            {
                SetPropertiesToCadEntity(_topMarkerLine.Value);
                return _topMarkerLine.Value;
            }
        }

        #endregion

        #region Circles

        #region Bottom

        private readonly Lazy<Circle> _bottomFirstMarker = new Lazy<Circle>(() => new Circle());
        public Circle BottomFirstCircle
        {
            get
            {
                SetPropertiesToCadEntity(_bottomFirstMarker.Value);
                return _bottomFirstMarker.Value;
            }
        }
        // Второй кружок при типе маркера 2
        private readonly Lazy<Circle> _bottomFirstMarkerType2 = new Lazy<Circle>(() => new Circle());
        public Circle BottomFirstCircleType2
        {
            get
            {
                SetPropertiesToCadEntity(_bottomFirstMarkerType2.Value);
                return _bottomFirstMarkerType2.Value;
            }
        }

        private readonly Lazy<Circle> _bottomSecondMarker = new Lazy<Circle>(() => new Circle());
        public Circle BottomSecondCircle
        {
            get
            {
                SetPropertiesToCadEntity(_bottomSecondMarker.Value);
                return _bottomSecondMarker.Value;
            }
        }

        private readonly Lazy<Circle> _bottomSecondMarkerType2 = new Lazy<Circle>(() => new Circle());
        public Circle BottomSecondCircleType2
        {
            get
            {
                SetPropertiesToCadEntity(_bottomSecondMarkerType2.Value);
                return _bottomSecondMarkerType2.Value;
            }
        }

        private readonly Lazy<Circle> _bottomThirdMarker = new Lazy<Circle>(() => new Circle());
        public Circle BottomThirdCircle
        {
            get
            {
                SetPropertiesToCadEntity(_bottomThirdMarker.Value);
                return _bottomThirdMarker.Value;
            }
        }

        private readonly Lazy<Circle> _bottomThirdMarkerType2 = new Lazy<Circle>(() => new Circle());
        public Circle BottomThirdCircleType2
        {
            get
            {
                SetPropertiesToCadEntity(_bottomThirdMarkerType2.Value);
                return _bottomThirdMarkerType2.Value;
            }
        }

        #endregion

        #region Top

        private readonly Lazy<Circle> _topFirstMarker = new Lazy<Circle>(() => new Circle());
        public Circle TopFirstCircle
        {
            get
            {
                SetPropertiesToCadEntity(_topFirstMarker.Value);
                return _topFirstMarker.Value;
            }
        }

        private readonly Lazy<Circle> _topFirstMarkerType2 = new Lazy<Circle>(() => new Circle());
        public Circle TopFirstCircleType2
        {
            get
            {
                SetPropertiesToCadEntity(_topFirstMarkerType2.Value);
                return _topFirstMarkerType2.Value;
            }
        }

        private readonly Lazy<Circle> _topSecondMarker = new Lazy<Circle>(() => new Circle());
        public Circle TopSecondCircle
        {
            get
            {
                SetPropertiesToCadEntity(_topSecondMarker.Value);
                return _topSecondMarker.Value;
            }
        }

        private readonly Lazy<Circle> _topSecondMarkerType2 = new Lazy<Circle>(() => new Circle());
        public Circle TopSecondCircleType2
        {
            get
            {
                SetPropertiesToCadEntity(_topSecondMarkerType2.Value);
                return _topSecondMarkerType2.Value;
            }
        }

        private readonly Lazy<Circle> _topThirdMarker = new Lazy<Circle>(() => new Circle());
        public Circle TopThirdCircle
        {
            get
            {
                SetPropertiesToCadEntity(_topThirdMarker.Value);
                return _topThirdMarker.Value;
            }
        }

        private readonly Lazy<Circle> _topThirdMarkerType2 = new Lazy<Circle>(() => new Circle());
        public Circle TopThirdCircleType2
        {
            get
            {
                SetPropertiesToCadEntity(_topThirdMarkerType2.Value);
                return _topThirdMarkerType2.Value;
            }
        }

        #endregion

        #region Orient

        private readonly Lazy<Circle> _bottomOrientMarker = new Lazy<Circle>(() => new Circle());
        public Circle BottomOrientCircle
        {
            get
            {
                SetPropertiesToCadEntity(_bottomOrientMarker.Value);
                return _bottomOrientMarker.Value;
            }
        }

        private readonly Lazy<Circle> _bottomOrientMarkerType2 = new Lazy<Circle>(() => new Circle());
        public Circle BottomOrientCircleType2
        {
            get
            {
                SetPropertiesToCadEntity(_bottomOrientMarkerType2.Value);
                return _bottomOrientMarkerType2.Value;
            }
        }

        private readonly Lazy<Circle> _topOrientMarker = new Lazy<Circle>(() => new Circle());
        public Circle TopOrientCircle
        {
            get
            {
                SetPropertiesToCadEntity(_topOrientMarker.Value);
                return _topOrientMarker.Value;
            }
        }

        private readonly Lazy<Circle> _topOrientMarkerType2 = new Lazy<Circle>(() => new Circle());
        public Circle TopOrientCircleType2
        {
            get
            {
                SetPropertiesToCadEntity(_topOrientMarkerType2.Value);
                return _topOrientMarkerType2.Value;
            }
        }

        #endregion

        #endregion

        #region Texts

        private readonly Lazy<DBText> _bottomFirstDBText = new Lazy<DBText>(() => new DBText());
        public DBText BottomFirstDBText
        {
            get
            {
                SetPropertiesToDBText(_bottomFirstDBText.Value);
                return _bottomFirstDBText.Value;
            }
        }
        private readonly Lazy<DBText> _topFirstDBText = new Lazy<DBText>(() => new DBText());
        public DBText TopFirstDBText
        {
            get
            {
                SetPropertiesToDBText(_topFirstDBText.Value);
                return _topFirstDBText.Value;
            }
        }

        private readonly Lazy<DBText> _bottomSecondDBText = new Lazy<DBText>(() => new DBText());
        public DBText BottomSecondDBText
        {
            get
            {
                SetPropertiesToDBText(_bottomSecondDBText.Value);
                return _bottomSecondDBText.Value;
            }
        }
        private readonly Lazy<DBText> _topSecondDBText = new Lazy<DBText>(() => new DBText());
        public DBText TopSecondDBText
        {
            get
            {
                SetPropertiesToDBText(_topSecondDBText.Value);
                return _topSecondDBText.Value;
            }
        }

        private readonly Lazy<DBText> _bottomThirdDBText = new Lazy<DBText>(() => new DBText());
        public DBText BottomThirdDBText
        {
            get
            {
                SetPropertiesToDBText(_bottomThirdDBText.Value);
                return _bottomThirdDBText.Value;
            }
        }
        private readonly Lazy<DBText> _topThirdDBText = new Lazy<DBText>(() => new DBText());
        public DBText TopThirdDBText
        {
            get
            {
                SetPropertiesToDBText(_topThirdDBText.Value);
                return _topThirdDBText.Value;
            }
        }

        private readonly Lazy<DBText> _bottomOrientDBText = new Lazy<DBText>(() => new DBText());
        public DBText BottomOrientDBText
        {
            get
            {
                SetPropertiesToDBText(_bottomOrientDBText.Value);
                return _bottomOrientDBText.Value;
            }
        }

        private readonly Lazy<DBText> _topOrientDBText = new Lazy<DBText>(() => new DBText());
        public DBText TopOrientDBText
        {
            get
            {
                SetPropertiesToDBText(_topOrientDBText.Value);
                return _topOrientDBText.Value;
            }
        }

        #endregion

        public override IEnumerable<Entity> Entities
        {
            get
            {
                yield return MainLine;
                yield return BottomMarkerLine;
                yield return TopMarkerLine;
                yield return BottomFirstCircle;
                yield return BottomFirstCircleType2;
                yield return BottomSecondCircle;
                yield return BottomSecondCircleType2;
                yield return BottomThirdCircle;
                yield return BottomThirdCircleType2;
                yield return TopFirstCircle;
                yield return TopFirstCircleType2;
                yield return TopSecondCircle;
                yield return TopSecondCircleType2;
                yield return TopThirdCircle;
                yield return TopThirdCircleType2;
                yield return BottomFractureOffsetLine;
                yield return TopFractureOffsetLine;
                yield return BottomFirstDBText;
                yield return BottomSecondDBText;
                yield return BottomThirdDBText;
                yield return TopFirstDBText;
                yield return TopSecondDBText;
                yield return TopThirdDBText;
                yield return BottomOrientArrow;
                yield return BottomOrientCircle;
                yield return BottomOrientCircleType2;
                yield return BottomOrientDBText;
                yield return BottomOrientLine;
                yield return TopOrientArrow;
                yield return TopOrientCircle;
                yield return TopOrientCircleType2;
                yield return TopOrientDBText;
                yield return TopOrientLine;
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
                var tmpEndPoint = new Point3d(InsertionPointOCS.X, InsertionPointOCS.Y - AxisMinLength * scale, InsertionPointOCS.Z);
                var tmpBottomMarkerPoint = new Point3d(tmpEndPoint.X, tmpEndPoint.Y - Fracture * scale, tmpEndPoint.Z);
                var tmpTopMarkerPoint = new Point3d(InsertionPointOCS.X, InsertionPointOCS.Y + Fracture * scale, InsertionPointOCS.Z);

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
            _mainLine.Value.StartPoint = insertionPoint;
            _mainLine.Value.EndPoint = endPoint;
            var mainVector = endPoint - insertionPoint;
            #region Bottom
            if (MarkersPosition == AxisMarkersPosition.Both ||
                MarkersPosition == AxisMarkersPosition.Bottom)
            {
                var firstMarkerCenter = bottomMarkerPoint + mainVector.GetNormal() * MarkersDiameter / 2 * scale;
                // bottom line
                var bottomLineStartPoint = endPoint + mainVector.GetNormal() * BottomFractureOffset * scale;
                if (BottomFractureOffset > 0)
                {
                    _bottomFractureOffsetLine.Value.StartPoint = endPoint;
                    _bottomFractureOffsetLine.Value.EndPoint = bottomLineStartPoint;
                }
                else _bottomFractureOffsetLine.Value.Visible = false;

                var markerLineVector = firstMarkerCenter - bottomLineStartPoint;
                _bottomMarkerLine.Value.Visible = true;
                _bottomMarkerLine.Value.StartPoint = bottomLineStartPoint;
                _bottomMarkerLine.Value.EndPoint = bottomLineStartPoint + markerLineVector.GetNormal() * (markerLineVector.Length - MarkersDiameter * scale / 2.0);
                // markers
                _bottomFirstMarker.Value.Visible = true;
                _bottomFirstMarker.Value.Center = firstMarkerCenter;
                _bottomFirstMarker.Value.Diameter = MarkersDiameter * scale;
                // text
                if (string.IsNullOrEmpty(FirstTextPrefix) && string.IsNullOrEmpty(FirstText) &&
                    string.IsNullOrEmpty(FirstTextSuffix))
                    BottomFirstDBText.Visible = false;
                else
                {
                    BottomFirstDBText.Visible = true;
                    BottomFirstDBText.Position = firstMarkerCenter;
                    BottomFirstDBText.AlignmentPoint = firstMarkerCenter;
                }
                // Второй кружок первого маркера
                if (FirstMarkerType == AxisMarkerType.Type2)
                {
                    _bottomFirstMarkerType2.Value.Center = firstMarkerCenter;
                    _bottomFirstMarkerType2.Value.Diameter = (MarkersDiameter - 2) * scale;
                }
                else _bottomFirstMarkerType2.Value.Visible = false;
                // Если количество маркеров больше 1
                if (MarkersCount > 1)
                {
                    // Значит второй маркер точно есть (независимо от 3-го)
                    var secontMarkerCenter = firstMarkerCenter + mainVector.GetNormal() * MarkersDiameter * scale;
                    _bottomSecondMarker.Value.Visible = true;
                    _bottomSecondMarker.Value.Center = secontMarkerCenter;
                    _bottomSecondMarker.Value.Diameter = MarkersDiameter * scale;
                    // text
                    if (string.IsNullOrEmpty(SecondTextPrefix) && string.IsNullOrEmpty(SecondText) &&
                        string.IsNullOrEmpty(SecondTextSuffix))
                        BottomSecondDBText.Visible = false;
                    else
                    {
                        BottomSecondDBText.Visible = true;
                        BottomSecondDBText.Position = secontMarkerCenter;
                        BottomSecondDBText.AlignmentPoint = secontMarkerCenter;
                    }
                    // второй кружок второго маркера
                    if (SecondMarkerType == AxisMarkerType.Type2)
                    {
                        _bottomSecondMarkerType2.Value.Center = secontMarkerCenter;
                        _bottomSecondMarkerType2.Value.Diameter = (MarkersDiameter - 2) * scale;
                    }
                    else _bottomSecondMarkerType2.Value.Visible = false;
                    // Если количество маркеров больше двух, тогда рисую 3-ий маркер
                    if (MarkersCount > 2)
                    {
                        var thirdMarkerCenter = secontMarkerCenter + mainVector.GetNormal() * MarkersDiameter * scale;
                        _bottomThirdMarker.Value.Visible = true;
                        _bottomThirdMarker.Value.Center = thirdMarkerCenter;
                        _bottomThirdMarker.Value.Diameter = MarkersDiameter * scale;
                        // text
                        if (string.IsNullOrEmpty(ThirdTextPrefix) && string.IsNullOrEmpty(ThirdText) &&
                            string.IsNullOrEmpty(ThirdTextSuffix))
                            BottomThirdDBText.Visible = false;
                        else
                        {
                            BottomThirdDBText.Visible = true;
                            BottomThirdDBText.Position = thirdMarkerCenter;
                            BottomThirdDBText.AlignmentPoint = thirdMarkerCenter;
                        }
                        // второй кружок третьего маркера
                        if (ThirdMarkerType == AxisMarkerType.Type2)
                        {
                            _bottomThirdMarkerType2.Value.Center = thirdMarkerCenter;
                            _bottomThirdMarkerType2.Value.Diameter = (MarkersDiameter - 2) * scale;
                        }
                        else _bottomThirdMarkerType2.Value.Visible = false;
                    }
                    else
                    {
                        _bottomThirdMarker.Value.Visible = false;
                        _bottomThirdMarkerType2.Value.Visible = false;
                    }
                }
                else
                {
                    _bottomSecondMarker.Value.Visible = false;
                    _bottomSecondMarkerType2.Value.Visible = false;
                    _bottomThirdMarker.Value.Visible = false;
                    _bottomThirdMarkerType2.Value.Visible = false;
                }

                #region Orient marker

                if (BottomOrientMarkerVisible)
                {
                    _bottomOrientLine.Value.Visible = true;
                    var bottomOrientMarkerCenter = BottomOrientPointOCS + mainVector.GetNormal() * MarkersDiameter / 2.0 * scale;
                    _bottomOrientMarker.Value.Visible = true;
                    _bottomOrientMarker.Value.Center = bottomOrientMarkerCenter;
                    _bottomOrientMarker.Value.Diameter = MarkersDiameter * scale;
                    // line
                    var _bottomOrientLineStartPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(
                        firstMarkerCenter, bottomOrientMarkerCenter, firstMarkerCenter,
                        MarkersDiameter / 2.0 * scale);
                    var _bottomOrientLineEndPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(
                        bottomOrientMarkerCenter, firstMarkerCenter, bottomOrientMarkerCenter,
                        MarkersDiameter / 2.0 * scale);
                    if (_bottomOrientLineEndPoint.IsEqualTo(_bottomOrientLineStartPoint, Tolerance.Global))
                    {
                        _bottomOrientLine.Value.Visible = false;
                        // arrow false
                        _bottomOrientArrow.Value.Visible = false;
                    }
                    else
                    {
                        _bottomOrientLine.Value.Visible = true;
                        _bottomOrientLine.Value.StartPoint = _bottomOrientLineStartPoint;
                        _bottomOrientLine.Value.EndPoint = _bottomOrientLineEndPoint;
                        // arrow
                        if (Math.Abs((_bottomOrientLineEndPoint - _bottomOrientLineStartPoint).Length) < ArrowsSize * scale ||
                            ArrowsSize == 0)
                        {
                            //arrow false
                            _bottomOrientArrow.Value.Visible = false;
                        }
                        else
                        {
                            _bottomOrientArrow.Value.Visible = true;
                            // arrow draw
                            var arrowStartPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(_bottomOrientLineEndPoint,
                                _bottomOrientLineStartPoint,
                                _bottomOrientLineEndPoint, ArrowsSize * scale);
                            if (_bottomOrientArrow.Value.NumberOfVertices == 2)
                            {
                                _bottomOrientArrow.Value.SetPointAt(0,
                                    ModPlus.Helpers.GeometryHelpers.ConvertPoint3dToPoint2d(arrowStartPoint));
                                _bottomOrientArrow.Value.SetBulgeAt(0, 0.0);
                                _bottomOrientArrow.Value.SetPointAt(1,
                                    ModPlus.Helpers.GeometryHelpers.ConvertPoint3dToPoint2d(_bottomOrientLineEndPoint));
                                _bottomOrientArrow.Value.SetBulgeAt(1, 0.0);
                                _bottomOrientArrow.Value.SetStartWidthAt(0, ArrowsSize * scale * 1 / 3);
                            }
                            else
                            {
                                _bottomOrientArrow.Value.AddVertexAt(0,
                                    ModPlus.Helpers.GeometryHelpers.ConvertPoint3dToPoint2d(arrowStartPoint),
                                    0.0, ArrowsSize * scale * 1 / 3, 0.0);
                                _bottomOrientArrow.Value.AddVertexAt(1,
                                    ModPlus.Helpers.GeometryHelpers.ConvertPoint3dToPoint2d(_bottomOrientLineEndPoint),
                                    0.0, 0.0, 0.0);
                            }
                        }
                    }
                    // text
                    if (string.IsNullOrEmpty(BottomOrientText))
                        BottomOrientDBText.Visible = false;
                    else
                    {
                        BottomOrientDBText.Visible = true;
                        BottomOrientDBText.Position = bottomOrientMarkerCenter;
                        BottomOrientDBText.AlignmentPoint = bottomOrientMarkerCenter;
                    }
                    // type2
                    if (OrientMarkerType == AxisMarkerType.Type2)
                    {
                        _bottomOrientMarkerType2.Value.Center = bottomOrientMarkerCenter;
                        _bottomOrientMarkerType2.Value.Diameter = (MarkersDiameter - 2) * scale;
                    }
                    else _bottomOrientMarkerType2.Value.Visible = false;
                }
                else
                {
                    _bottomOrientArrow.Value.Visible = false;
                    _bottomOrientDBText.Value.Visible = false;
                    _bottomOrientLine.Value.Visible = false;
                    _bottomOrientMarker.Value.Visible = false;
                    _bottomOrientMarkerType2.Value.Visible = false;
                }

                #endregion
            }
            else
            {
                _bottomMarkerLine.Value.Visible = false;
                _bottomFirstMarker.Value.Visible = false;
                _bottomFirstMarkerType2.Value.Visible = false;
                _bottomSecondMarker.Value.Visible = false;
                _bottomSecondMarkerType2.Value.Visible = false;
                _bottomThirdMarker.Value.Visible = false;
                _bottomThirdMarkerType2.Value.Visible = false;
                _bottomFractureOffsetLine.Value.Visible = false;
                _bottomFirstDBText.Value.Visible = false;
                _bottomSecondDBText.Value.Visible = false;
                _bottomThirdDBText.Value.Visible = false;
                _bottomOrientArrow.Value.Visible = false;
                _bottomOrientDBText.Value.Visible = false;
                _bottomOrientLine.Value.Visible = false;
                _bottomOrientMarker.Value.Visible = false;
                _bottomOrientMarkerType2.Value.Visible = false;
            }
            #endregion

            #region Top
            if (MarkersPosition == AxisMarkersPosition.Both ||
                MarkersPosition == AxisMarkersPosition.Top)
            {
                var firstMarkerCenter = topMarkerPoint - mainVector.GetNormal() * MarkersDiameter / 2 * scale;
                // top line
                var topLineStartPoint = insertionPoint - mainVector.GetNormal() * TopFractureOffset * scale;
                if (TopFractureOffset > 0)
                {
                    _topFractureOffsetLine.Value.StartPoint = insertionPoint;
                    _topFractureOffsetLine.Value.EndPoint = topLineStartPoint;
                }
                else _topFractureOffsetLine.Value.Visible = false;

                var markerLineVector = firstMarkerCenter - topLineStartPoint;
                _topMarkerLine.Value.Visible = true;
                _topMarkerLine.Value.StartPoint = topLineStartPoint;
                _topMarkerLine.Value.EndPoint = topLineStartPoint + markerLineVector.GetNormal() * (markerLineVector.Length - MarkersDiameter * scale / 2.0);
                // markers
                _topFirstMarker.Value.Visible = true;
                _topFirstMarker.Value.Center = firstMarkerCenter;
                _topFirstMarker.Value.Diameter = MarkersDiameter * scale;
                // text
                if (string.IsNullOrEmpty(FirstTextPrefix) && string.IsNullOrEmpty(FirstText) &&
                    string.IsNullOrEmpty(FirstTextSuffix))
                    TopFirstDBText.Visible = false;
                else
                {
                    TopFirstDBText.Visible = true;
                    TopFirstDBText.Position = firstMarkerCenter;
                    TopFirstDBText.AlignmentPoint = firstMarkerCenter;
                }
                // Второй кружок первого маркера
                if (FirstMarkerType == AxisMarkerType.Type2)
                {
                    _topFirstMarkerType2.Value.Center = firstMarkerCenter;
                    _topFirstMarkerType2.Value.Diameter = (MarkersDiameter - 2) * scale;
                }
                else _topFirstMarkerType2.Value.Visible = false;
                // Если количество маркеров больше 1
                if (MarkersCount > 1)
                {
                    // Значит второй маркер точно есть (независимо от 3-го)
                    var secontMarkerCenter = firstMarkerCenter - mainVector.GetNormal() * MarkersDiameter * scale;
                    _topSecondMarker.Value.Visible = true;
                    _topSecondMarker.Value.Center = secontMarkerCenter;
                    _topSecondMarker.Value.Diameter = MarkersDiameter * scale;
                    // text
                    if (string.IsNullOrEmpty(SecondTextPrefix) && string.IsNullOrEmpty(SecondText) &&
                        string.IsNullOrEmpty(SecondTextSuffix))
                        TopSecondDBText.Visible = false;
                    else
                    {
                        TopSecondDBText.Visible = true;
                        TopSecondDBText.Position = secontMarkerCenter;
                        TopSecondDBText.AlignmentPoint = secontMarkerCenter;
                    }
                    // второй кружок второго маркера
                    if (SecondMarkerType == AxisMarkerType.Type2)
                    {
                        _topSecondMarkerType2.Value.Center = secontMarkerCenter;
                        _topSecondMarkerType2.Value.Diameter = (MarkersDiameter - 2) * scale;
                    }
                    else _topSecondMarkerType2.Value.Visible = false;
                    // Если количество маркеров больше двух, тогда рисую 3-ий маркер
                    if (MarkersCount > 2)
                    {
                        var thirdMarkerCenter = secontMarkerCenter - mainVector.GetNormal() * MarkersDiameter * scale;
                        _topThirdMarker.Value.Visible = true;
                        _topThirdMarker.Value.Center = thirdMarkerCenter;
                        _topThirdMarker.Value.Diameter = MarkersDiameter * scale;
                        // text
                        if (string.IsNullOrEmpty(ThirdTextPrefix) && string.IsNullOrEmpty(ThirdText) &&
                            string.IsNullOrEmpty(ThirdTextSuffix))
                            TopThirdDBText.Visible = false;
                        else
                        {
                            TopThirdDBText.Visible = true;
                            TopThirdDBText.Position = thirdMarkerCenter;
                            TopThirdDBText.AlignmentPoint = thirdMarkerCenter;
                        }
                        // второй кружок третьего маркера
                        if (ThirdMarkerType == AxisMarkerType.Type2)
                        {
                            _topThirdMarkerType2.Value.Center = thirdMarkerCenter;
                            _topThirdMarkerType2.Value.Diameter = (MarkersDiameter - 2) * scale;
                        }
                        else _topThirdMarkerType2.Value.Visible = false;
                    }
                    else
                    {
                        _topThirdMarker.Value.Visible = false;
                        _topThirdMarkerType2.Value.Visible = false;
                    }
                }
                else
                {
                    _topSecondMarker.Value.Visible = false;
                    _topSecondMarkerType2.Value.Visible = false;
                    _topThirdMarker.Value.Visible = false;
                    _topThirdMarkerType2.Value.Visible = false;
                }

                #region Orient marker

                if (TopOrientMarkerVisible)
                {
                    _topOrientLine.Value.Visible = true;
                    var topOrientMarkerCenter = TopOrientPointOCS - mainVector.GetNormal() * MarkersDiameter / 2.0 * scale;
                    _topOrientMarker.Value.Visible = true;
                    _topOrientMarker.Value.Center = topOrientMarkerCenter;
                    _topOrientMarker.Value.Diameter = MarkersDiameter * scale;
                    // line
                    var _topOrientLineStartPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(
                        firstMarkerCenter, topOrientMarkerCenter, firstMarkerCenter,
                        MarkersDiameter / 2.0 * scale);
                    var _topOrientLineEndPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(
                        topOrientMarkerCenter, firstMarkerCenter, topOrientMarkerCenter,
                        MarkersDiameter / 2.0 * scale);
                    if (_topOrientLineEndPoint.IsEqualTo(_topOrientLineStartPoint, Tolerance.Global))
                    {
                        _topOrientLine.Value.Visible = false;
                        // arrow false
                        _topOrientArrow.Value.Visible = false;
                    }
                    else
                    {
                        _topOrientLine.Value.Visible = true;
                        _topOrientLine.Value.StartPoint = _topOrientLineStartPoint;
                        _topOrientLine.Value.EndPoint = _topOrientLineEndPoint;
                        // arrow
                        if (Math.Abs((_topOrientLineEndPoint - _topOrientLineStartPoint).Length) < ArrowsSize * scale ||
                            ArrowsSize == 0)
                        {
                            //arrow false
                            _topOrientArrow.Value.Visible = false;
                        }
                        else
                        {
                            _topOrientArrow.Value.Visible = true;
                            // arrow draw
                            var arrowStartPoint = ModPlus.Helpers.GeometryHelpers.Point3dAtDirection(_topOrientLineEndPoint,
                                _topOrientLineStartPoint,
                                _topOrientLineEndPoint, ArrowsSize * scale);
                            if (_topOrientArrow.Value.NumberOfVertices == 2)
                            {
                                _topOrientArrow.Value.SetPointAt(0,
                                    ModPlus.Helpers.GeometryHelpers.ConvertPoint3dToPoint2d(arrowStartPoint));
                                _topOrientArrow.Value.SetBulgeAt(0, 0.0);
                                _topOrientArrow.Value.SetPointAt(1,
                                    ModPlus.Helpers.GeometryHelpers.ConvertPoint3dToPoint2d(_topOrientLineEndPoint));
                                _topOrientArrow.Value.SetBulgeAt(1, 0.0);
                                _topOrientArrow.Value.SetStartWidthAt(0, ArrowsSize * scale * 1 / 3);
                            }
                            else
                            {
                                _topOrientArrow.Value.AddVertexAt(0,
                                    ModPlus.Helpers.GeometryHelpers.ConvertPoint3dToPoint2d(arrowStartPoint),
                                    0.0, ArrowsSize * scale * 1 / 3, 0.0);
                                _topOrientArrow.Value.AddVertexAt(1,
                                    ModPlus.Helpers.GeometryHelpers.ConvertPoint3dToPoint2d(_topOrientLineEndPoint),
                                    0.0, 0.0, 0.0);
                            }
                        }
                    }
                    // text
                    if (string.IsNullOrEmpty(TopOrientText))
                        TopOrientDBText.Visible = false;
                    else
                    {
                        TopOrientDBText.Visible = true;
                        TopOrientDBText.Position = topOrientMarkerCenter;
                        TopOrientDBText.AlignmentPoint = topOrientMarkerCenter;
                    }
                    // type2
                    if (OrientMarkerType == AxisMarkerType.Type2)
                    {
                        _topOrientMarkerType2.Value.Center = topOrientMarkerCenter;
                        _topOrientMarkerType2.Value.Diameter = (MarkersDiameter - 2) * scale;
                    }
                    else _topOrientMarkerType2.Value.Visible = false;
                }
                else
                {
                    _topOrientArrow.Value.Visible = false;
                    _topOrientDBText.Value.Visible = false;
                    _topOrientLine.Value.Visible = false;
                    _topOrientMarker.Value.Visible = false;
                    _topOrientMarkerType2.Value.Visible = false;
                }

                #endregion
            }
            else
            {
                _topMarkerLine.Value.Visible = false;
                _topFirstMarker.Value.Visible = false;
                _topFirstMarkerType2.Value.Visible = false;
                _topSecondMarker.Value.Visible = false;
                _topSecondMarkerType2.Value.Visible = false;
                _topThirdMarker.Value.Visible = false;
                _topThirdMarkerType2.Value.Visible = false;
                _topFractureOffsetLine.Value.Visible = false;
                _topFirstDBText.Value.Visible = false;
                _topSecondDBText.Value.Visible = false;
                _topThirdDBText.Value.Visible = false;
                _topOrientArrow.Value.Visible = false;
                _topOrientDBText.Value.Visible = false;
                _topOrientLine.Value.Visible = false;
                _topOrientMarker.Value.Visible = false;
                _topOrientMarkerType2.Value.Visible = false;
            }
            #endregion
        }

        private void UpdateTextEntities()
        {
            SetFirstTextOnCreation();
            BottomFirstDBText.TextString = FirstTextPrefix + FirstText + FirstTextSuffix;
            BottomSecondDBText.TextString = SecondTextPrefix + SecondText + SecondTextSuffix;
            BottomThirdDBText.TextString = ThirdTextPrefix + ThirdText + ThirdTextSuffix;
            TopFirstDBText.TextString = FirstTextPrefix + FirstText + FirstTextSuffix;
            TopSecondDBText.TextString = SecondTextPrefix + SecondText + SecondTextSuffix;
            TopThirdDBText.TextString = ThirdTextPrefix + ThirdText + ThirdTextSuffix;
            BottomOrientDBText.TextString = BottomOrientText;
            TopOrientDBText.TextString = TopOrientText;
        }

        private void SetFirstTextOnCreation()
        {
            if (EndPointOCS == Point3d.Origin) return;
            if (IsValueCreated)
            {
                var check = 1 / Math.Sqrt(2);
                var v = (EndPointOCS - InsertionPointOCS).GetNormal();
                if ((v.X > check || v.X < -check) && (v.Y < check || v.Y > -check))
                    FirstText = GetFirstTextValueByLastAxis("Horizontal");
                else
                    FirstText = GetFirstTextValueByLastAxis("Vertical");
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
                        if (AxisFunction.AxisRusAlphabet.Contains(LastHorizontalValue))
                        {
                            var index = AxisFunction.AxisRusAlphabet.IndexOf(LastHorizontalValue);
                            if (index == AxisFunction.AxisRusAlphabet.Count - 1)
                            {
                                _newHorizontalMarkValue = AxisFunction.AxisRusAlphabet[0];
                                return _newHorizontalMarkValue;
                            }
                            _newHorizontalMarkValue = AxisFunction.AxisRusAlphabet[index + 1];
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
                        if (AxisFunction.AxisRusAlphabet.Contains(LastVerticalValue))
                        {
                            var index = AxisFunction.AxisRusAlphabet.IndexOf(LastVerticalValue);
                            if (index == AxisFunction.AxisRusAlphabet.Count - 1)
                            {
                                _newVerticalMarkValue = AxisFunction.AxisRusAlphabet[0];
                                return _newVerticalMarkValue;
                            }
                            _newVerticalMarkValue = AxisFunction.AxisRusAlphabet[index + 1];
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

        public override ResultBuffer GetParametersForXData()
        {
            try
            {
                // ReSharper disable once UseObjectOrCollectionInitializer
                var resBuf = new ResultBuffer();
                // 1001 - DxfCode.ExtendedDataRegAppName. AppName
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, AxisInterface.Name));
                // 1010
                // Вектор от конечной точки до начальной с учетом масштаба блока и трансформацией блока
                var vector = EndPointOCS - InsertionPointOCS;
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataXCoordinate, new Point3d(vector.X, vector.Y, vector.Z))); //0
                // Текстовые значения (код 1000)
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, StyleGuid)); // 0
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, MarkersPosition.ToString())); // 1
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, Scale.Name)); // 2
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, TextStyle)); // 3
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, FirstText)); // 4
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, FirstTextPrefix)); // 5
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, FirstTextSuffix)); // 6
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, SecondText)); // 7
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, SecondTextPrefix)); // 8
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, SecondTextSuffix)); // 9
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, ThirdText)); // 10
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, ThirdTextPrefix)); // 11
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, ThirdTextSuffix)); // 12
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, BottomOrientText)); // 13
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, TopOrientText)); // 14
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, BottomOrientMarkerVisible.ToString()));//15
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, TopOrientMarkerVisible.ToString()));//16
                // Целочисленные значения (код 1070)
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, MarkersDiameter)); // 0
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, Fracture)); // 1
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, MarkersCount)); // 2
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, BottomFractureOffset)); // 3
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, TopFractureOffset)); // 4
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, (int)FirstMarkerType)); // 5
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, (int)SecondMarkerType)); // 6
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, (int)ThirdMarkerType)); // 7
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, (int)OrientMarkerType)); // 8
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, ArrowsSize)); // 9
                // Значения типа double (dxfCode 1040)
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, LineTypeScale)); // 0
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, BottomLineAngle)); // 1
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, TopLineAngle)); // 2
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, TextHeight)); // 3
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, BottomOrientMarkerOffset)); // 4
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, TopOrientMarkerOffset)); // 5

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
                var index1000 = 0;
                var index1070 = 0;
                var index1040 = 0;
                foreach (TypedValue typedValue in resBufArr)
                {
                    switch ((DxfCode)typedValue.TypeCode)
                    {
                        case DxfCode.ExtendedDataXCoordinate:
                            {
                                // Получаем вектор от последней точки до первой в системе координат блока
                                var vectorFromEndToInsertion = ((Point3d)typedValue.Value).GetAsVector();
                                // получаем конечную точку в мировой системе координат
                                EndPoint = (InsertionPointOCS + vectorFromEndToInsertion).TransformBy(BlockTransform);
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
                                        MarkersPosition = Enum.TryParse(typedValue.Value.ToString(), out AxisMarkersPosition p) ? p : AxisMarkersPosition.Bottom;
                                        break;
                                    case 2:
                                        Scale = AcadHelpers.GetAnnotationScaleByName(typedValue.Value.ToString());
                                        break;
                                    case 3:
                                        TextStyle = typedValue.Value.ToString();
                                        break;
                                    case 4:
                                        FirstText = typedValue.Value.ToString();
                                        break;
                                    case 5:
                                        FirstTextPrefix = typedValue.Value.ToString();
                                        break;
                                    case 6:
                                        FirstTextSuffix = typedValue.Value.ToString();
                                        break;
                                    case 7:
                                        SecondText = typedValue.Value.ToString();
                                        break;
                                    case 8:
                                        SecondTextPrefix = typedValue.Value.ToString();
                                        break;
                                    case 9:
                                        SecondTextSuffix = typedValue.Value.ToString();
                                        break;
                                    case 10:
                                        ThirdText = typedValue.Value.ToString();
                                        break;
                                    case 11:
                                        ThirdTextPrefix = typedValue.Value.ToString();
                                        break;
                                    case 12:
                                        ThirdTextSuffix = typedValue.Value.ToString();
                                        break;
                                    case 13:
                                        BottomOrientText = typedValue.Value.ToString();
                                        break;
                                    case 14:
                                        TopOrientText = typedValue.Value.ToString();
                                        break;
                                    case 15:
                                        BottomOrientMarkerVisible = bool.TryParse(typedValue.Value.ToString(), out var b) && b;// false
                                        break;
                                    case 16:
                                        TopOrientMarkerVisible = bool.TryParse(typedValue.Value.ToString(), out b) && b;//false
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
                                        MarkersDiameter = (Int16)typedValue.Value;
                                        break;
                                    case 1:
                                        Fracture = (Int16)typedValue.Value;
                                        break;
                                    case 2:
                                        MarkersCount = (Int16)typedValue.Value;
                                        break;
                                    case 3:
                                        BottomFractureOffset = (Int16)typedValue.Value;
                                        break;
                                    case 4:
                                        TopFractureOffset = (Int16)typedValue.Value;
                                        break;
                                    case 5:
                                        FirstMarkerType = (AxisMarkerType)(Int16)typedValue.Value;
                                        break;
                                    case 6:
                                        SecondMarkerType = (AxisMarkerType)(Int16)typedValue.Value;
                                        break;
                                    case 7:
                                        ThirdMarkerType = (AxisMarkerType)(Int16)typedValue.Value;
                                        break;
                                    case 8:
                                        OrientMarkerType = (AxisMarkerType)(Int16)typedValue.Value;
                                        break;
                                    case 9:
                                        ArrowsSize = (Int16)typedValue.Value;
                                        break;
                                }
                                //index
                                index1070++;
                                break;
                            }
                        case DxfCode.ExtendedDataReal:
                            {
                                switch (index1040)
                                {
                                    case 0:
                                        LineTypeScale = (double)typedValue.Value;
                                        break;
                                    case 1:
                                        BottomLineAngle = (double)typedValue.Value;
                                        break;
                                    case 2:
                                        TopLineAngle = (double)typedValue.Value;
                                        break;
                                    case 3:
                                        TextHeight = (double)typedValue.Value;
                                        break;
                                    case 4:
                                        BottomOrientMarkerOffset = (double)typedValue.Value;
                                        break;
                                    case 5:
                                        TopOrientMarkerOffset = (double)typedValue.Value;
                                        break;
                                }
                                index1040++;
                                break;
                            }
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
