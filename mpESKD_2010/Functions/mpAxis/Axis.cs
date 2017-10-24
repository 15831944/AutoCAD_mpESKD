using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry;
using mpESKD.Base;
using mpESKD.Base.Helpers;
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
            // Устанавливаю текущий масштаб
            Scale = AcadHelpers.Database.Cannoscale;
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
                return _bottomMarkerPoint;
            }
            set
            {
                _bottomMarkerPoint = value;
                BottomLineAngle = (EndPoint - InsertionPoint).GetAngleTo(value - EndPoint, Vector3d.ZAxis);
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
                    InsertionPoint.X + topLineLength * GetScale() * Math.Cos(angleA + TopLineAngle),
                    InsertionPoint.Y + topLineLength * GetScale() * Math.Sin(angleA + TopLineAngle),
                    InsertionPoint.Z);
                return _topMarkerPoint;
            }
            set
            {
                _topMarkerPoint = value;
                TopLineAngle = (InsertionPoint - EndPoint).GetAngleTo(value - InsertionPoint, Vector3d.ZAxis);
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
            if (new MainSettings().UseScaleFromStyle)
                Scale = StyleHelpers.GetPropertyValue(style, nameof(Scale), AxisProperties.ScalePropertyDescriptive.DefaultValue);
            LineTypeScale = StyleHelpers.GetPropertyValue(style, nameof(LineTypeScale), AxisProperties.LineTypeScalePropertyDescriptive.DefaultValue);
        }

        #endregion

        #region Entities
        private readonly Lazy<Line> _mainLine = new Lazy<Line>(() => new Line());
        /// <summary>Средняя (основная) линия оси</summary>
        public Line MainLine
        {
            get
            {
                _mainLine.Value.Color = Color.FromColorIndex(ColorMethod.ByBlock, 1);
                _mainLine.Value.LineWeight = LineWeight.ByBlock;
                _mainLine.Value.Linetype = "ByBlock";
                _mainLine.Value.LinetypeScale = LineTypeScale;
                return _mainLine.Value;
            }
        }
        private readonly Lazy<Line> _bottomMarkerLine = new Lazy<Line>(() => new Line());
        /// <summary>"Палочка" от конечной точки до кружка (маркера)</summary>
        public Line BottomMarkerLine
        {
            get
            {
                _bottomMarkerLine.Value.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
                _bottomMarkerLine.Value.LineWeight = LineWeight.ByBlock;
                _bottomMarkerLine.Value.Linetype = "Continuous";
                _bottomMarkerLine.Value.LinetypeScale = 1.0;
                return _bottomMarkerLine.Value;
            }
        }
        private readonly Lazy<Line> _topMarkerLine = new Lazy<Line>(() => new Line());
        /// <summary>Палочка от точки вставки до кружка (маркера)</summary>
        public Line TopMarkerLine
        {
            get
            {
                _topMarkerLine.Value.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
                _topMarkerLine.Value.LineWeight = LineWeight.ByBlock;
                _topMarkerLine.Value.Linetype = "Continuous";
                _topMarkerLine.Value.LinetypeScale = 1.0;
                return _topMarkerLine.Value;
            }
        }

        #region Circles

        
        private readonly Lazy<Circle> _bottomFirstMarker = new Lazy<Circle>(() => new Circle());
        public Circle BottomFirstCircle
        {
            get
            {
                _bottomFirstMarker.Value.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
                _bottomFirstMarker.Value.LineWeight = LineWeight.ByBlock;
                _bottomFirstMarker.Value.Linetype = "Continuous";
                _bottomFirstMarker.Value.LinetypeScale = 1.0;
                return _bottomFirstMarker.Value;
            }
        }

        private readonly Lazy<Circle> _bottomSecondMarker = new Lazy<Circle>(() => new Circle());
        public Circle BottomSecondCircle
        {
            get
            {
                _bottomSecondMarker.Value.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
                _bottomSecondMarker.Value.LineWeight = LineWeight.ByBlock;
                _bottomSecondMarker.Value.Linetype = "Continuous";
                _bottomSecondMarker.Value.LinetypeScale = 1.0;
                return _bottomSecondMarker.Value;
            }
        }

        private readonly Lazy<Circle> _bottomThirdMarker = new Lazy<Circle>(() => new Circle());
        public Circle BottomThirdCircle
        {
            get
            {
                _bottomThirdMarker.Value.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
                _bottomThirdMarker.Value.LineWeight = LineWeight.ByBlock;
                _bottomThirdMarker.Value.Linetype = "Continuous";
                _bottomThirdMarker.Value.LinetypeScale = 1.0;
                return _bottomThirdMarker.Value;
            }
        }

        private readonly Lazy<Circle> _topFirstMarker = new Lazy<Circle>(() => new Circle());

        public Circle TopFirstCircle
        {
            get
            {
                _topFirstMarker.Value.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
                _topFirstMarker.Value.LineWeight = LineWeight.ByBlock;
                _topFirstMarker.Value.Linetype = "Continuous";
                _topFirstMarker.Value.LinetypeScale = 1.0;
                return _topFirstMarker.Value;
            }
        }
        private readonly Lazy<Circle> _topSecondMarker = new Lazy<Circle>(() => new Circle());

        public Circle TopSecondCircle
        {
            get
            {
                _topSecondMarker.Value.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
                _topSecondMarker.Value.LineWeight = LineWeight.ByBlock;
                _topSecondMarker.Value.Linetype = "Continuous";
                _topSecondMarker.Value.LinetypeScale = 1.0;
                return _topSecondMarker.Value;
            }
        }
        private readonly Lazy<Circle> _topThirdMarker = new Lazy<Circle>(() => new Circle());

        public Circle TopThirdCircle
        {
            get
            {
                _topThirdMarker.Value.Color = Color.FromColorIndex(ColorMethod.ByBlock, 0);
                _topThirdMarker.Value.LineWeight = LineWeight.ByBlock;
                _topThirdMarker.Value.Linetype = "Continuous";
                _topThirdMarker.Value.LinetypeScale = 1.0;
                return _topThirdMarker.Value;
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
                yield return BottomSecondCircle;
                yield return BottomThirdCircle;
                yield return TopFirstCircle;
                yield return TopSecondCircle;
                yield return TopThirdCircle;
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
            // Bottom
            if (MarkersPosition == AxisMarkersPosition.Both ||
                MarkersPosition == AxisMarkersPosition.Bottom)
            {
                // bottom line
                _bottomMarkerLine.Value.StartPoint = endPoint;
                _bottomMarkerLine.Value.EndPoint = bottomMarkerPoint;
                // markers
                var firstMarkerCenter = bottomMarkerPoint + mainVector.GetNormal() * MarkersDiameter / 2 * scale;
                _bottomFirstMarker.Value.Center = firstMarkerCenter;
                _bottomFirstMarker.Value.Diameter = MarkersDiameter * scale;
                if (MarkersCount > 1)
                {
                    var secontMarkerCenter = firstMarkerCenter + mainVector.GetNormal() * MarkersDiameter * scale;
                    _bottomSecondMarker.Value.Center = secontMarkerCenter;
                    _bottomSecondMarker.Value.Diameter = MarkersDiameter * scale;

                    if (MarkersCount > 2)
                    {
                        var thirdMarkerCenter = secontMarkerCenter + mainVector.GetNormal() * MarkersDiameter * scale;
                        _bottomThirdMarker.Value.Center = thirdMarkerCenter;
                        _bottomThirdMarker.Value.Diameter = MarkersDiameter * scale;
                    }
                    else _bottomThirdMarker.Value.Visible = false;
                }
                else _bottomSecondMarker.Value.Visible = false;
                
            }
            else
            {
                _bottomMarkerLine.Value.Visible = false;
                _bottomFirstMarker.Value.Visible = false;
                _bottomSecondMarker.Value.Visible = false;
                _bottomThirdMarker.Value.Visible = false;
            }
            // Top
            if (MarkersPosition == AxisMarkersPosition.Both ||
                MarkersPosition == AxisMarkersPosition.Top)
            {
                // top line
                _topMarkerLine.Value.StartPoint = insertionPoint;
                _topMarkerLine.Value.EndPoint = topMarkerPoint;
                // markers
                var firstMarkerCenter = topMarkerPoint - mainVector.GetNormal() * MarkersDiameter / 2 * scale;
                _topFirstMarker.Value.Center = firstMarkerCenter;
                _topFirstMarker.Value.Diameter = MarkersDiameter * scale;
                if (MarkersCount > 1)
                {
                    var secontMarkerCenter = firstMarkerCenter - mainVector.GetNormal() * MarkersDiameter * scale;
                    _topSecondMarker.Value.Center = secontMarkerCenter;
                    _topSecondMarker.Value.Diameter = MarkersDiameter * scale;

                    if (MarkersCount > 2)
                    {
                        var thirdMarkerCenter = secontMarkerCenter - mainVector.GetNormal() * MarkersDiameter * scale;
                        _topThirdMarker.Value.Center = thirdMarkerCenter;
                        _topThirdMarker.Value.Diameter = MarkersDiameter * scale;
                    }
                    else _topThirdMarker.Value.Visible = false;
                }
                else _topSecondMarker.Value.Visible = false;
            }
            else
            {
                _topMarkerLine.Value.Visible = false;
                _topFirstMarker.Value.Visible = false;
                _topSecondMarker.Value.Visible = false;
                _topThirdMarker.Value.Visible = false;
            }
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
                // Целочисленные значения (код 1070)
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, MarkersDiameter)); // 0
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, Fracture)); // 1
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, MarkersCount)); // 2
                // Значения типа double (dxfCode 1040)
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, LineTypeScale)); // 0
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, BottomLineAngle)); // 1
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, TopLineAngle)); // 2

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
