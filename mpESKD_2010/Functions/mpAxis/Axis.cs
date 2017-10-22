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

        public double BottomLineLength { get; set; } = 10.0;
        public double BottomLineAngle { get; set; } = 0.0;

        private Point3d _bottomMarkerPoint;
        /// <summary>Нижняя точка расположения маркеров</summary>  
        public Point3d BottomMarkerPoint
        {
            get
            {
                var baseVector = new Vector3d(1.0, 0.0, 0.0);
                var angleA = baseVector.GetAngleTo(EndPoint - InsertionPoint, Vector3d.ZAxis);
                _bottomMarkerPoint = new Point3d(
                    EndPoint.X + BottomLineLength * Math.Cos(angleA + BottomLineAngle),
                    EndPoint.Y + BottomLineLength * Math.Sin(angleA + BottomLineAngle),
                    EndPoint.Z);
                return _bottomMarkerPoint;
            }
            set
            {
                _bottomMarkerPoint = value;
                BottomLineLength = (value - EndPoint).Length;
                BottomLineAngle = (EndPoint - InsertionPoint).GetAngleTo(value - EndPoint, Vector3d.ZAxis);
            }
        }

        public double TopLineLength { get; set; } = 10.0;
        public double TopLineAngle { get; set; } = 0.0;

        private Point3d _topMarkerPoint;
        /// <summary>Верхняя точка расположения маркеров</summary>
        public Point3d TopMarkerPoint
        {
            get
            {
                var baseVector = new Vector3d(1.0, 0.0, 0.0);
                var angleA = baseVector.GetAngleTo(InsertionPoint - EndPoint, Vector3d.ZAxis);
                _topMarkerPoint = new Point3d(
                    InsertionPoint.X + TopLineLength * Math.Cos(angleA + TopLineAngle),
                    InsertionPoint.Y + TopLineLength * Math.Sin(angleA + TopLineAngle),
                    InsertionPoint.Z);
                return _topMarkerPoint;
            }
            set
            {
                _topMarkerPoint = value;
                TopLineLength = (value - InsertionPoint).Length;
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

        public AnnotationScale Scale { get; set; } //= BreakLineProperties.ScalePropertyDescriptive.DefaultValue;
        /// <summary>Масштаб типа линии для входящей полилинии</summary>
        public double LineTypeScale { get; set; } //= BreakLineProperties.LineTypeScalePropertyDescriptive.DefaultValue;
        /// <summary>Текущий масштаб</summary>
        public double GetScale()
        {
            return Scale.DrawingUnits / Scale.PaperUnits;
        }
        /// <summary>Минимальная длина от точки вставки до конечной точки</summary>
        public double AxisMinLength => 1.0;

        #endregion

        #region Axis Properties
        /// <summary>Диаметр маркеров</summary>
        public double MarkerDiameter { get; set; } = AxisProperties.MarkerDiameterPropertyDescriptive.DefaultValue;

        /// <summary>Положение маркеров</summary>
        public AxisMarkersPosition MarkersPosition { get; set; } = AxisProperties.MarkersPositionPropertyDescriptive.DefaultValue;

        #endregion

        #region Style

        /// <summary>Идентификатор стиля</summary>
        public string StyleGuid { get; set; } = "00000000-0000-0000-0000-000000000000";

        /// <summary>Применение стиля по сути должно переопределять текущие параметры</summary>
        public void ApplyStyle(AxisStyle style)
        {
            // apply settings from style
            //Overhang = StyleHelpers.GetPropertyValue(style, nameof(Overhang), BreakLineProperties.OverhangPropertyDescriptive.DefaultValue);
            //BreakHeight = StyleHelpers.GetPropertyValue(style, nameof(BreakHeight), BreakLineProperties.BreakHeightPropertyDescriptive.DefaultValue);
            //BreakWidth = StyleHelpers.GetPropertyValue(style, nameof(BreakWidth), BreakLineProperties.BreakWidthPropertyDescriptive.DefaultValue);
            MarkersPosition = StyleHelpers.GetPropertyValue(style, nameof(MarkersPosition), AxisProperties.MarkersPositionPropertyDescriptive.DefaultValue);
            MarkerDiameter = StyleHelpers.GetPropertyValue(style, nameof(MarkerDiameter), AxisProperties.MarkerDiameterPropertyDescriptive.DefaultValue);
            if (new MainSettings().UseScaleFromStyle)
                Scale = StyleHelpers.GetPropertyValue(style, nameof(Scale), AxisProperties.ScalePropertyDescriptive.DefaultValue);
            LineTypeScale = StyleHelpers.GetPropertyValue(style, nameof(LineTypeScale), AxisProperties.LineTypeScalePropertyDescriptive.DefaultValue);
        }

        #endregion

        #region Entities
        private Lazy<Line> _mainLine = new Lazy<Line>(() => new Line());
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
        private Lazy<Line> _bottomMarkerLine = new Lazy<Line>(() => new Line());
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
        private Lazy<Line> _topMarkerLine = new Lazy<Line>(() => new Line());
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
        public override IEnumerable<Entity> Entities
        {
            get
            {
                yield return MainLine;
                yield return BottomMarkerLine;
                yield return TopMarkerLine;
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
                var tmpBottomMarkerPoint = new Point3d(tmpEndPoint.X, tmpEndPoint.Y - 10.0 * scale, tmpEndPoint.Z);
                var tmpTopMarkerPoint = new Point3d(InsertionPointOCS.X, InsertionPointOCS.Y + 10.0 * scale, InsertionPointOCS.Z);

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
            if (_mainLine == null) _mainLine = new Lazy<Line>(() => new Line());
            // main line
            _mainLine.Value.StartPoint = insertionPoint;
            _mainLine.Value.EndPoint = endPoint;
            if (MarkersPosition == AxisMarkersPosition.Both ||
                MarkersPosition == AxisMarkersPosition.Bottom)
            {
                // bottom line
                _bottomMarkerLine.Value.StartPoint = endPoint;
                _bottomMarkerLine.Value.EndPoint = bottomMarkerPoint;
            }
            else
            {
                _bottomMarkerLine.Value.Visible = false;
            }
            if (MarkersPosition == AxisMarkersPosition.Both ||
                MarkersPosition == AxisMarkersPosition.Top)
            {
                // top line
                _topMarkerLine.Value.StartPoint = insertionPoint;
                _topMarkerLine.Value.EndPoint = topMarkerPoint;
            }
            else
            {
                _topMarkerLine.Value.Visible = false;
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
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, MarkerDiameter)); // 0
                //resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, BreakHeight)); // 1
                //resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, BreakWidth)); // 2
                // Значения типа double (dxfCode 1040)
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, LineTypeScale)); // 0
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, BottomLineAngle)); // 1
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, BottomLineLength)); // 2
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, TopLineAngle)); // 3
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, TopLineLength)); // 4

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
                                if (index1070 == 0) // 0 - MarkerDiameter
                                    MarkerDiameter = (Int16)typedValue.Value;
                                //if (index1070 == 1) // 1- breakHeight
                                //    BreakHeight = (Int16)typedValue.Value;
                                //if (index1070 == 2) // 2 - breakWidth
                                //    BreakWidth = (Int16)typedValue.Value;
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
                                if (index1040 == 2) // 2 - BottomLineLenght
                                    BottomLineLength = (double)typedValue.Value;
                                if (index1040 == 3) // 3 - TopLineAngle
                                    TopLineAngle = (double)typedValue.Value;
                                if (index1040 == 4) // 4 - TopLineLenght
                                    TopLineLength = (double)typedValue.Value;
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
