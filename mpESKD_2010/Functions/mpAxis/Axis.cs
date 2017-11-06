using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry;
using mpESKD.Base;
using mpESKD.Base.Helpers;
using mpESKD.Base.Properties;
using mpESKD.Base.Styles;
using mpESKD.Functions.mpAxis.Properties;
using mpESKD.Functions.mpAxis.Styles;
using ModPlus.Helpers;
using ModPlusAPI.Windows;
// ReSharper disable InconsistentNaming

namespace mpESKD.Functions.mpAxis
{
    public class Axis : MPCOEntity
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
        public Axis(AxisStyle style)
        {
            var blockTableRecord = new BlockTableRecord
            {
                Name = "*U",
                BlockScaling = BlockScaling.Uniform
            };
            BlockRecord = blockTableRecord;
            StyleGuid = style.Guid;
            // Применяем текущий стиль к СПДС примитиву
            ApplyStyle(style);
        }
        #endregion

        #region Points and Grips

        /// <summary>Средняя точка. Нужна для перемещения  примитива</summary>
        public Point3d MiddlePoint => new Point3d
        (
            (InsertionPoint.X + EndPoint.X) / 2,
            (InsertionPoint.Y + EndPoint.Y) / 2,
            (InsertionPoint.Z + EndPoint.Z) / 2
        );

        /// <summary>Вторая (конечная) точка примитива в мировой системе координат</summary>
        public Point3d EndPoint { get; set; } = Point3d.Origin;

        public double BottomLineAngle { get; set; } = 0.0;

        private Point3d _bottomMarkerPoint;
        /// <summary>Нижняя точка расположения маркеров</summary>  
        public Point3d BottomMarkerPoint
        {
            get
            {
                var baseVector = new Vector3d(1.0, 0.0, 0.0);
                var angleA = baseVector.GetAngleTo(EndPoint - InsertionPoint, Vector3d.ZAxis);
                var bottomLineLength = Fracture / Math.Cos(BottomLineAngle) * GetScale();
                _bottomMarkerPoint = new Point3d(
                    EndPoint.X + bottomLineLength * Math.Cos(angleA + BottomLineAngle),
                    EndPoint.Y + bottomLineLength * Math.Sin(angleA + BottomLineAngle),
                    EndPoint.Z);
                return _bottomMarkerPoint + (EndPoint - InsertionPoint).GetNormal() * BottomFractureOffset * GetScale();
            }
            set
            {
                _bottomMarkerPoint = value;
                BottomLineAngle = (EndPoint - InsertionPoint).GetAngleTo(value - EndPoint - (EndPoint - InsertionPoint).GetNormal() * BottomFractureOffset * GetScale(), Vector3d.ZAxis);
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
                var topLineLength = Fracture / Math.Cos(TopLineAngle) * GetScale();
                _topMarkerPoint = new Point3d(
                    InsertionPoint.X + topLineLength * Math.Cos(angleA + TopLineAngle),
                    InsertionPoint.Y + topLineLength * Math.Sin(angleA + TopLineAngle),
                    InsertionPoint.Z);
                return _topMarkerPoint + (InsertionPoint - EndPoint).GetNormal() * TopFractureOffset * GetScale();
            }
            set
            {
                _topMarkerPoint = value;
                TopLineAngle = (InsertionPoint - EndPoint).GetAngleTo(value - InsertionPoint - (InsertionPoint - EndPoint).GetNormal() * TopFractureOffset * GetScale(), Vector3d.ZAxis);
            }
        }
        // Получение управляющих точек в системе координат блока для отрисовки содержимого
        private Point3d InsertionPointOCS => InsertionPoint.TransformBy(BlockTransform.Inverse());
        private Point3d EndPointOCS => EndPoint.TransformBy(BlockTransform.Inverse());
        private Point3d BottomMarkerPointOCS => BottomMarkerPoint.TransformBy(BlockTransform.Inverse());
        private Point3d TopMarkerPointOCS => TopMarkerPoint.TransformBy(BlockTransform.Inverse());


        #region Grips
        /* Можно создать коллекцию ручек и логически получать их из коллекции по индексу
         * Но я сделаю отдельно каждую ручку, таким образом я буду работать с конкретной 
         * ручкой по ее "имени"
         * Ручки "зависимы" от точек примитива, поэтому их будем только "получать"
         * Я бы мог просто получать точки примитива, но так можно и запутаться
         */
        /// <summary>Первая ручка. Равна точке вставки</summary>
        public Point3d StartGrip => InsertionPoint;
        /// <summary>Средняя ручка. Равна средней точке</summary>
        public Point3d MiddleGrip => MiddlePoint;
        /// <summary>Конечная ручка. Равна конечной точке</summary>
        public Point3d EndGrip => EndPoint;
        /// <summary>Ручка нижней точки расположения маркеров</summary>
        public Point3d BottomMarkerGrip => BottomMarkerPoint;
        /// <summary>Ручка верхней точки расположения маркеров</summary>
        public Point3d TopMarkerGrip => TopMarkerPoint;
        #endregion

        #endregion

        #region General Properties

        /// <summary>Минимальная длина от точки вставки до конечной точки</summary>
        public double AxisMinLength => 1.0;


        #endregion

        #region Axis Properties
        /// <summary>Диаметр маркеров</summary>
        public int MarkersDiameter { get; set; } = AxisProperties.MarkersDiameterPropertyDescriptive.DefaultValue;

        /// <summary>Количество маркеров</summary>
        public int MarkersCount { get; set; } = AxisProperties.MarkersCountPropertyDescriptive.DefaultValue;

        /// <summary>Положение маркеров</summary>
        public AxisMarkersPosition MarkersPosition { get; set; } = AxisProperties.MarkersPositionPropertyDescriptive.DefaultValue;
        /// <summary>Излом</summary>
        public int Fracture { get; set; } = 10;
        /// <summary>Нижний отступ излома</summary>
        public int BottomFractureOffset { get; set; } = 0;
        /// <summary>Верхний отступ излома</summary>
        public int TopFractureOffset { get; set; } = 0;
        // Типы маркеров: 0 - один кружок, 1 - два кружка
        public int FirstMarkerType { get; set; } = 0;
        public int SecondMarkerType { get; set; } = 0;
        public int ThirdMarkerType { get; set; } = 0;
        public int OrientMarkerType { get; set; } = 0;

        // 
        public string TextStyle { get; set; } = AxisProperties.TextStylePropertyDescriptive.DefaultValue;
        public double TextHeight { get; set; }
        public string FirstTextPrefix { get; set; } = string.Empty;
        public string FirstText { get; set; } = "A";
        public string FirstTextSuffix { get; set; } = string.Empty;

        #endregion

        #region Style

        /// <summary>Идентификатор стиля</summary>
        public string StyleGuid { get; set; } = "00000000-0000-0000-0000-000000000000";

        /// <summary>Применение стиля по сути должно переопределять текущие параметры</summary>
        public void ApplyStyle(AxisStyle style)
        {
            // apply settings from style
            Fracture = StyleHelpers.GetPropertyValue(style, nameof(Fracture), AxisProperties.FracturePropertyDescriptive.DefaultValue);
            MarkersPosition = StyleHelpers.GetPropertyValue(style, nameof(MarkersPosition), AxisProperties.MarkersPositionPropertyDescriptive.DefaultValue);
            MarkersDiameter = StyleHelpers.GetPropertyValue(style, nameof(MarkersDiameter), AxisProperties.MarkersDiameterPropertyDescriptive.DefaultValue);
            MarkersCount = StyleHelpers.GetPropertyValue(style, nameof(MarkersCount), AxisProperties.MarkersCountPropertyDescriptive.DefaultValue);
            BottomFractureOffset = StyleHelpers.GetPropertyValue(style, nameof(BottomFractureOffset), AxisProperties.BottomFractureOffsetPropertyDescriptive.DefaultValue);
            FirstMarkerType = StyleHelpers.GetPropertyValue(style, nameof(FirstMarkerType), AxisProperties.FirstMarkerTypePropertyDescriptive.DefaultValue);
            SecondMarkerType = StyleHelpers.GetPropertyValue(style, nameof(SecondMarkerType), AxisProperties.SecondMarkerTypePropertyDescriptive.DefaultValue);
            ThirdMarkerType = StyleHelpers.GetPropertyValue(style, nameof(ThirdMarkerType), AxisProperties.ThirdMarkerTypePropertyDescriptive.DefaultValue);
            TopFractureOffset = StyleHelpers.GetPropertyValue(style, nameof(TopFractureOffset), AxisProperties.TopFractureOffsetPropertyDescriptive.DefaultValue);
            TextHeight = StyleHelpers.GetPropertyValue(style, nameof(TextHeight), AxisProperties.TextHeightPropertyDescriptive.DefaultValue);
            Scale = MainStaticSettings.Settings.UseScaleFromStyle
                ? StyleHelpers.GetPropertyValue(style, nameof(Scale), AxisProperties.ScalePropertyDescriptive.DefaultValue)
                : AcadHelpers.Database.Cannoscale;
            LineTypeScale = StyleHelpers.GetPropertyValue(style, nameof(LineTypeScale), AxisProperties.LineTypeScalePropertyDescriptive.DefaultValue);
            // set layer
            var layerName = StyleHelpers.GetPropertyValue(style, AxisProperties.LayerName.Name, AxisProperties.LayerName.DefaultValue);
            AcadHelpers.SetLayerByName(BlockId, layerName, style.LayerXmlData);
            // set line type
            var lineType = StyleHelpers.GetPropertyValue(style, AxisProperties.LineTypePropertyDescriptive.Name, AxisProperties.LineTypePropertyDescriptive.DefaultValue);
            AcadHelpers.SetLineType(BlockId, lineType);
            // set text style
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

        #endregion

        #region Entities
        /// <summary>Установка свойств для примитивов, которые не меняются</summary>
        /// <param name="entity">Примитив автокада</param>
        private void SetPropertiesToCadEntity(Entity entity)
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
            }
        }
        /// <summary>Обновление (перерисовка) базовых примитивов</summary>
        public void UpdateEntities()
        {
            try
            {
                var length = EndPointOCS.DistanceTo(InsertionPointOCS);
                var scale = GetScale();
                if (EndPointOCS.Equals(Point3d.Origin))
                {
                    // Задание точки вставки (т.е. второй точки еще нет)
                    MakeSimplyEntity(UpdateVariant.SetInsertionPoint);
                }
                else if (length < AxisMinLength * scale)
                {
                    // Задание второй точки - случай когда расстояние между точками меньше минимального
                    MakeSimplyEntity(UpdateVariant.SetEndPointMinLength);
                }
                else
                {
                    // Задание второй точки
                    SetEntitiesPoints(InsertionPointOCS, EndPointOCS, BottomMarkerPointOCS, TopMarkerPointOCS);
                }
                UpdateTextEntities();
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
        /// <summary>
        /// Построение "базового" простого варианта СПДС примитива
        /// Тот вид, который висит на мышке при создании и указании точки вставки
        /// </summary>
        private void MakeSimplyEntity(UpdateVariant variant)
        {
            var scale = GetScale();
            // Создание вершин полилинии
            if (variant == UpdateVariant.SetInsertionPoint)
            {
                /* Изменение базовых примитивов в момент указания второй точки при условии второй точки нет
                 * Примерно аналогично созданию, только точки не создаются, а меняются
                */
                var tmpEndPoint = new Point3d(InsertionPointOCS.X, InsertionPointOCS.Y - AxisMinLength * scale, InsertionPointOCS.Z);
                var tmpBottomMarkerPoint = new Point3d(tmpEndPoint.X, tmpEndPoint.Y - Fracture * scale, tmpEndPoint.Z);
                var tmpTopMarkerPoint = new Point3d(InsertionPointOCS.X, InsertionPointOCS.Y + Fracture * scale, InsertionPointOCS.Z);

                SetEntitiesPoints(InsertionPointOCS, tmpEndPoint, tmpBottomMarkerPoint, tmpTopMarkerPoint);
            }
            else if (variant == UpdateVariant.SetEndPointMinLength) // изменение вершин полилинии
            {
                /* Изменение базовых примитивов в момент указания второй точки
                * при условии что расстояние от второй точки до первой больше минимального допустимого
                */
                var tmpEndPoint = GeometryHelpers.Point3dAtDirection(InsertionPoint, EndPoint, InsertionPointOCS, AxisMinLength * scale);
                SetEntitiesPoints(InsertionPointOCS, tmpEndPoint, BottomMarkerPointOCS, TopMarkerPointOCS);
                EndPoint = tmpEndPoint.TransformBy(BlockTransform);
            }
        }
        /// <summary>Изменение примитивов по точкам</summary>
        private void SetEntitiesPoints(Point3d insertionPoint, Point3d endPoint,
            Point3d bottomMarkerPoint, Point3d topMarkerPoint)
        {
            var scale = GetScale();
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
                _bottomMarkerLine.Value.StartPoint = bottomLineStartPoint;
                _bottomMarkerLine.Value.EndPoint = bottomLineStartPoint + markerLineVector.GetNormal() * (markerLineVector.Length - MarkersDiameter * scale / 2.0);
                // markers
                _bottomFirstMarker.Value.Center = firstMarkerCenter;
                _bottomFirstMarker.Value.Diameter = MarkersDiameter * scale;
                // text
                if (string.IsNullOrEmpty(FirstTextPrefix) && string.IsNullOrEmpty(FirstText) &&
                    string.IsNullOrEmpty(FirstTextSuffix))
                    BottomFirstDBText.Visible = false;
                else
                {
                    BottomFirstDBText.Position = firstMarkerCenter;
                    BottomFirstDBText.AlignmentPoint = firstMarkerCenter;
                }
                // Второй кружок первого маркера
                if (FirstMarkerType == 1)
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
                    _bottomSecondMarker.Value.Center = secontMarkerCenter;
                    _bottomSecondMarker.Value.Diameter = MarkersDiameter * scale;
                    // второй кружок второго маркера
                    if (SecondMarkerType == 1)
                    {
                        _bottomSecondMarkerType2.Value.Center = secontMarkerCenter;
                        _bottomSecondMarkerType2.Value.Diameter = (MarkersDiameter - 2) * scale;
                    }
                    else _bottomSecondMarkerType2.Value.Visible = false;
                    // Если количество маркеров больше двух, тогда рисую 3-ий маркер
                    if (MarkersCount > 2)
                    {
                        var thirdMarkerCenter = secontMarkerCenter + mainVector.GetNormal() * MarkersDiameter * scale;
                        _bottomThirdMarker.Value.Center = thirdMarkerCenter;
                        _bottomThirdMarker.Value.Diameter = MarkersDiameter * scale;
                        // второй кружок третьего маркера
                        if (ThirdMarkerType == 1)
                        {
                            _bottomThirdMarkerType2.Value.Center = thirdMarkerCenter;
                            _bottomThirdMarkerType2.Value.Diameter = (MarkersDiameter - 2) * scale;
                        }
                        else _bottomThirdMarkerType2.Value.Visible = false;
                    }
                    else _bottomThirdMarker.Value.Visible = false;
                }
                else
                {
                    _bottomSecondMarker.Value.Visible = false;
                    _bottomThirdMarker.Value.Visible = false;
                }

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
                _topMarkerLine.Value.StartPoint = topLineStartPoint;
                _topMarkerLine.Value.EndPoint = topLineStartPoint + markerLineVector.GetNormal() * (markerLineVector.Length - MarkersDiameter * scale / 2.0);
                // markers
                _topFirstMarker.Value.Center = firstMarkerCenter;
                _topFirstMarker.Value.Diameter = MarkersDiameter * scale;
                // Второй кружок первого маркера
                if (FirstMarkerType == 1)
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
                    _topSecondMarker.Value.Center = secontMarkerCenter;
                    _topSecondMarker.Value.Diameter = MarkersDiameter * scale;
                    // второй кружок второго маркера
                    if (SecondMarkerType == 1)
                    {
                        _topSecondMarkerType2.Value.Center = secontMarkerCenter;
                        _topSecondMarkerType2.Value.Diameter = (MarkersDiameter - 2) * scale;
                    }
                    else _topSecondMarkerType2.Value.Visible = false;
                    // Если количество маркеров больше двух, тогда рисую 3-ий маркер
                    if (MarkersCount > 2)
                    {
                        var thirdMarkerCenter = secontMarkerCenter - mainVector.GetNormal() * MarkersDiameter * scale;
                        _topThirdMarker.Value.Center = thirdMarkerCenter;
                        _topThirdMarker.Value.Diameter = MarkersDiameter * scale;
                        // второй кружок третьего маркера
                        if (ThirdMarkerType == 1)
                        {
                            _topThirdMarkerType2.Value.Center = thirdMarkerCenter;
                            _topThirdMarkerType2.Value.Diameter = (MarkersDiameter - 2) * scale;
                        }
                        else _topThirdMarkerType2.Value.Visible = false;
                    }
                    else _topThirdMarker.Value.Visible = false;
                }
                else
                {
                    _topThirdMarker.Value.Visible = false;
                    _topSecondMarker.Value.Visible = false;
                }
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
            }
            #endregion
        }

        public void UpdateTextEntities()
        {
            BottomFirstDBText.TextString = FirstTextPrefix + FirstText + FirstTextSuffix;
        }
        #endregion

        public ResultBuffer GetParametersForXData()
        {
            try
            {
                // ReSharper disable once UseObjectOrCollectionInitializer
                var resBuf = new ResultBuffer();
                // 1001 - DxfCode.ExtendedDataRegAppName. AppName
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, AxisFunction.MPCOEntName));
                // 1010
                // Вектор от конечной точки до начальной с учетом масштаба блока и трансформацией блока
                var vector = EndPointOCS - InsertionPointOCS;
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataXCoordinate, new Point3d(vector.X, vector.Y, vector.Z))); //0
                // Текстовые значения (код 1000)
                // Стиль
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, StyleGuid)); // 0
                // Позиция маркеров
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, MarkersPosition.ToString())); // 1
                // scale
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, Scale.Name)); // 2
                // text style
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, TextStyle)); // 3
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, FirstText)); // 4
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, FirstTextPrefix)); // 5
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, FirstTextSuffix)); // 6
                // Целочисленные значения (код 1070)
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, MarkersDiameter)); // 0
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, Fracture)); // 1
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, MarkersCount)); // 2
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, BottomFractureOffset)); // 3
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, TopFractureOffset)); // 4
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, FirstMarkerType)); // 5
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, SecondMarkerType)); // 6
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, ThirdMarkerType)); // 7
                // Значения типа double (dxfCode 1040)
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, LineTypeScale)); // 0
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, BottomLineAngle)); // 1
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, TopLineAngle)); // 2
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, TextHeight)); // 3

                return resBuf;
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
                return null;
            }
        }

        public void GetParametersFromResBuf(ResultBuffer resBuf)
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
                                if (index1000 == 0) // 0 - это идентификатор стиля
                                    StyleGuid = typedValue.Value.ToString();
                                if (index1000 == 1) // 1 - breakline type
                                    MarkersPosition = AxisPropertiesHelpers.GetAxisMarkersPositionFromString(typedValue.Value.ToString());
                                if (index1000 == 2) // 2 - scale
                                    Scale = AcadHelpers.GetAnnotationScaleByName(typedValue.Value.ToString());
                                if (index1000 == 3) // 3 - TextStyle
                                    TextStyle = typedValue.Value.ToString();
                                if (index1000 == 4) // 4 - FirstText
                                    FirstText = typedValue.Value.ToString();
                                if (index1000 == 5) // 5 - FirstTextPrefix
                                    FirstTextPrefix = typedValue.Value.ToString();
                                if (index1000 == 6) // 6 - FirstTextSuffix
                                    FirstTextSuffix = typedValue.Value.ToString();
                                // index
                                index1000++;
                                break;
                            }
                        case DxfCode.ExtendedDataInteger16:
                            {
                                if (index1070 == 0) // 0 - MarkersDiameter
                                    MarkersDiameter = (Int16)typedValue.Value;
                                if (index1070 == 1) // 1- Fracture
                                    Fracture = (Int16)typedValue.Value;
                                if (index1070 == 2) // 2 - MarkersCount
                                    MarkersCount = (Int16)typedValue.Value;
                                if (index1070 == 3) // 3 - BottomFractureOffset
                                    BottomFractureOffset = (Int16)typedValue.Value;
                                if (index1070 == 4) // 4 - TopFractureOffset
                                    TopFractureOffset = (Int16)typedValue.Value;
                                if (index1070 == 5) // 5 - FirstMarkerType
                                    FirstMarkerType = (Int16)typedValue.Value;
                                if (index1070 == 6) // 6 - SecondMarkerType
                                    SecondMarkerType = (Int16)typedValue.Value;
                                if (index1070 == 7) // 7 - ThirdMarkerType
                                    ThirdMarkerType = (Int16)typedValue.Value;
                                //index
                                index1070++;
                                break;
                            }
                        case DxfCode.ExtendedDataReal:
                            {
                                if (index1040 == 0) // 0 - LineTypeScale
                                    LineTypeScale = (double)typedValue.Value;
                                if (index1040 == 1) // 1 - BottomLineAngle
                                    BottomLineAngle = (double)typedValue.Value;
                                if (index1040 == 2) // 2 - TopLineAngle
                                    TopLineAngle = (double)typedValue.Value;
                                if (index1040 == 3) // 3 - TextHeight
                                    TextHeight = (double)typedValue.Value;
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

        enum UpdateVariant
        {
            SetInsertionPoint,
            SetEndPointMinLength
        }
    }

    /// <summary>Вспомогательный класс для работы с XData</summary>
    public static class AxisXDataHelper
    {
        public static bool SaveToEntity(DBObject dbObject, Axis axis)
        {
            try
            {
                dbObject.UpgradeOpen();
                using (ResultBuffer resBuf = axis.GetParametersForXData())
                {
                    dbObject.XData = resBuf;
                }
                dbObject.DowngradeOpen();
                return true;
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
                return false;
            }
        }
        /// <summary>Создание экземпляра СПДС примитива по данным блока</summary>
        /// <param name="ent">блок (примитив автокада)</param>
        /// <returns></returns>
        public static Axis GetAxisFromEntity(Entity ent)
        {
            using (ResultBuffer resBuf = ent.GetXDataForApplication(AxisFunction.MPCOEntName))
            {
                // В случае команды ОТМЕНА может вернуть null
                if (resBuf == null) return null;
                Axis axis = new Axis(ent.ObjectId);
                // Получаем параметры из самого блока
                // ОБЯЗАТЕЛЬНО СНАЧАЛА ИЗ БЛОКА!!!!!!
                axis.GetParametersFromEntity(ent);
                // Получаем параметры из XData
                axis.GetParametersFromResBuf(resBuf);

                return axis;
            }
        }
    }
}
