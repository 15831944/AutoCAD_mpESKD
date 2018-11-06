namespace mpESKD.Functions.mpGroundLine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Base.Helpers;
    using Base.Styles;
    using ModPlus.Helpers;
    using ModPlusAPI.Windows;
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

        #region Grips

        /// <summary>Первая ручка. Равна точке вставки</summary>
        public Point3d StartGrip => InsertionPoint;

        public List<Point3d> MiddleGrips => MiddlePoints;

        /// <summary>Конечная ручка. Равна конечной точке</summary>
        public Point3d EndGrip => EndPoint;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Минимальная длина линии грунта
        /// </summary>
        public double GroundLineMinLength => 10.0;


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

        public override IEnumerable<Entity> Entities
        {
            get
            {
                yield return MainPolyline;
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
                var tmpEndPoint = GeometryHelpers.Point3dAtDirection(InsertionPoint, EndPoint, InsertionPointOCS, GroundLineMinLength * scale);
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
        private void CreateEntities(Point3d insertionPoint, List<Point3d> middlePoints, Point3d endPoint, double scale)
        {
            var points = GetPointsForMainPolyline(insertionPoint, middlePoints, endPoint);
            // Если количество точек совпадает, то просто их меняем
            if (points.Count == MainPolyline.NumberOfVertices)
            {
                for (var i = 0; i < points.Count; i++)
                {
                    MainPolyline.SetPointAt(i, points[i]);
                    MainPolyline.SetBulgeAt(i, 0.0);
                }
            }
            // Иначе создаем заново
            else
            {
                for (var i = 0; i < MainPolyline.NumberOfVertices; i++)
                    MainPolyline.RemoveVertexAt(i);
                for (var i = 0; i < points.Count; i++)
                    MainPolyline.AddVertexAt(i, points[i], 0.0, 0.0, 0.0);
            }
        }

        private Point2dCollection GetPointsForMainPolyline(Point3d insertionPoint, List<Point3d> middlePoints, Point3d endPoint)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var points = new Point2dCollection();

            points.Add(GeometryHelpers.ConvertPoint3dToPoint2d(insertionPoint));
            middlePoints.ForEach(p => points.Add(GeometryHelpers.ConvertPoint3dToPoint2d(p)));
            points.Add(GeometryHelpers.ConvertPoint3dToPoint2d(endPoint));

            return points;
        }

        #endregion

        #region Style

        /// <summary>Идентификатор стиля</summary>
        public string StyleGuid { get; set; } = "00000000-0000-0000-0000-000000000000";

        /// <summary>Применение стиля по сути должно переопределять текущие параметры</summary>
        public void ApplyStyle(GroundLineStyle style)
        {
            // apply settings from style

            ////Scale = MainStaticSettings.Settings.UseScaleFromStyle
            ////    ? StyleHelpers.GetPropertyValue(style, nameof(Scale), GroundLineProperties.Scale.DefaultValue)
            ////    : AcadHelpers.Database.Cannoscale;
            ////LineTypeScale = StyleHelpers.GetPropertyValue(style, nameof(LineTypeScale), AxisProperties.LineTypeScale.DefaultValue);
            ////// set layer
            ////var layerName = StyleHelpers.GetPropertyValue(style, AxisProperties.LayerName.Name, AxisProperties.LayerName.DefaultValue);
            ////AcadHelpers.SetLayerByName(BlockId, layerName, style.LayerXmlData);
            ////// set line type
            ////var lineType = StyleHelpers.GetPropertyValue(style, AxisProperties.LineType.Name, AxisProperties.LineType.DefaultValue);
            ////AcadHelpers.SetLineType(BlockId, lineType);
        }

        #endregion

        public override ResultBuffer GetParametersForXData()
        {
            try
            {
                // ReSharper disable once UseObjectOrCollectionInitializer
                var resBuf = new ResultBuffer();
                // 1001 - DxfCode.ExtendedDataRegAppName. AppName
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, GroundLineFunction.MPCOEntName));
                // Вектор от конечной точки до начальной с учетом масштаба блока и трансформацией блока
                var vector = EndPointOCS - InsertionPointOCS;
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataXCoordinate, new Point3d(vector.X, vector.Y, vector.Z))); //0

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
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        internal enum UpdateVariant
        {
            SetInsertionPoint,
            SetEndPointMinLength
        }
    }
}
