namespace mpESKD.Functions.mpBreakLine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Base.Helpers;
    using mpESKD.Base.Styles;
    using Properties;
    using Styles;
    using ModPlus.Helpers;
    using ModPlusAPI.Windows;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class BreakLine : MPCOEntity
    {
        /// <summary>Инициализация экземпляра класса для BreakLine без заполнения данными
        /// В данном случае уже все данные получены и нужно только "построить" 
        /// базовые примитивы</summary>
        public BreakLine(ObjectId blockId)
        {
            BlockId = blockId;
        }
        /// <summary>Инициализация экземпляра класса для BreakLine для создания</summary>
        public BreakLine(BreakLineStyle style)
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
        
        // Основные свойства  примитива

        /// <summary>Средняя точка. Нужна для перемещения  примитива</summary>
        public Point3d MiddlePoint => new Point3d
        (
            (InsertionPoint.X + EndPoint.X) / 2,
            (InsertionPoint.Y + EndPoint.Y) / 2,
            (InsertionPoint.Z + EndPoint.Z) / 2
        );

        /// <summary>Вторая (конечная) точка примитива в мировой системе координат</summary>
        public Point3d EndPoint { get; set; } = Point3d.Origin;
        
        // Получение управляющих точек в системе координат блока для отрисовки содержимого
        private Point3d InsertionPointOCS => InsertionPoint.TransformBy(BlockTransform.Inverse());
        
        private Point3d EndPointOCS => EndPoint.TransformBy(BlockTransform.Inverse());

        /// <summary>Выступ линии обрыва за граници "обрываемого" объекта</summary>
        public int Overhang { get; set; } = BreakLineProperties.Overhang.DefaultValue;
        
        /// <summary>Ширина Обрыва для линейного обрыва</summary>
        public int BreakWidth { get; set; } = BreakLineProperties.BreakWidth.DefaultValue;
        
        /// <summary>Длина обрыва для линейного обрыва</summary>
        public int BreakHeight { get; set; } = BreakLineProperties.BreakHeight.DefaultValue;
        
        /// <summary>Тип линии обрыва: линейный, криволинейный, цилиндрический</summary>
        public BreakLineType BreakLineType { get; set; } = BreakLineProperties.BreakLineType.DefaultValue;

        #region Базовые примитивы ЕСКД объекта
        
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
                //yield return other entities
            }
        }

        /// <summary>Минимальная длина линии обрыва от точки вставки до конечной точки</summary>
        public double BreakLineMinLength
        {
            get
            {
                if (BreakLineType == BreakLineType.Linear)
                    return 15.0;
                if (BreakLineType == BreakLineType.Curvilinear)
                    return 1.0;
                if (BreakLineType == BreakLineType.Cylindrical)
                    return 1.0;
                return 15.0;
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
                else if (length < BreakLineMinLength * scale)
                {
                    // Задание второй точки - случай когда расстояние между точками меньше минимального
                    MakeSimplyEntity(UpdateVariant.SetEndPointMinLength, scale);
                }
                else
                {
                    // Задание второй точки
                    var pts = PointsToCreatePolyline(scale, InsertionPointOCS, EndPointOCS, out List<double> bulges);
                    FillMainPolylineWithPoints(pts, bulges);
                }
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
            List<double> bulges;
            if (variant == UpdateVariant.SetInsertionPoint)
            {
                /* Изменение базовых примитивов в момент указания второй точки при условии второй точки нет
                 * Примерно аналогично созданию, только точки не создаются, а меняются
                */
                var tmpEndPoint = new Point3d(InsertionPointOCS.X + BreakLineMinLength * scale, InsertionPointOCS.Y, InsertionPointOCS.Z);

                var pts = PointsToCreatePolyline(scale, InsertionPointOCS, tmpEndPoint, out bulges);
                FillMainPolylineWithPoints(pts, bulges);
            }
            else if (variant == UpdateVariant.SetEndPointMinLength) // изменение вершин полилинии
            {
                /* Изменение базовых примитивов в момент указания второй точки
                * при условии что расстояние от второй точки до первой больше минимального допустимого
                */
                var tmpEndPoint = GeometryHelpers.Point3dAtDirection(InsertionPoint, EndPoint, InsertionPointOCS, BreakLineMinLength * scale /** BlockTransform.GetScale()*/);
                var pts = PointsToCreatePolyline(scale, InsertionPointOCS, tmpEndPoint, out bulges);
                FillMainPolylineWithPoints(pts, bulges);
                EndPoint = tmpEndPoint.TransformBy(BlockTransform);
            }
        }

        /// <summary>
        /// Получение точек для построения базовой полилинии
        /// </summary>
        /// <param name="scale">Масштабный коэффициент</param>
        /// <param name="insertionPoint">Первая точка (точка вставки)</param>
        /// <param name="endPoint">Вторая (конечная) точка</param>
        /// <param name="bulges">Список выпуклостей</param>
        /// <returns></returns>
        private Point2dCollection PointsToCreatePolyline(double scale, Point3d insertionPoint, Point3d endPoint, out List<double> bulges)
        {
            var length = endPoint.DistanceTo(insertionPoint);
            bulges = new List<double>();
            var pts = new Point2dCollection();
            if (BreakLineType == BreakLineType.Linear)
            {
                // точки
                if (Overhang > 0)
                {
                    pts.Add(GeometryHelpers.Point2dAtDirection(endPoint, insertionPoint, insertionPoint, Overhang * scale));
                    bulges.Add(0.0);
                }
                // Первая точка, соответствующая ручке
                pts.Add(GeometryHelpers.ConvertPoint3dToPoint2d(insertionPoint));
                bulges.Add(0.0);
                pts.Add(GeometryHelpers.GetPointToExtendLine(insertionPoint, endPoint, length / 2 - BreakWidth / 2.0 * scale));
                bulges.Add(0.0);
                pts.Add(GeometryHelpers.GetPerpendicularPoint2d(
                    insertionPoint,
                    GeometryHelpers.ConvertPoint2DToPoint3D(GeometryHelpers.GetPointToExtendLine(insertionPoint, endPoint, length / 2 - BreakWidth / 4.0 * scale)),
                    BreakHeight / 2.0 * scale));
                bulges.Add(0.0);
                pts.Add(GeometryHelpers.GetPerpendicularPoint2d(insertionPoint, GeometryHelpers.ConvertPoint2DToPoint3D(
                    GeometryHelpers.GetPointToExtendLine(insertionPoint, endPoint, length / 2 + BreakWidth / 4.0 * scale)), -BreakHeight / 2.0 * scale));
                bulges.Add(0.0);
                pts.Add(GeometryHelpers.GetPointToExtendLine(insertionPoint, endPoint, length / 2 + BreakWidth / 2.0 * scale));
                bulges.Add(0.0);
                // Конечная точка, соответствующая ручке
                pts.Add(GeometryHelpers.Point2dAtDirection(insertionPoint, endPoint, insertionPoint, length));
                bulges.Add(0.0);
                if (Overhang > 0)
                {
                    pts.Add(GeometryHelpers.Point2dAtDirection(insertionPoint, endPoint, insertionPoint, length + Overhang * scale));
                    bulges.Add(0.0);
                }
            }
            if (BreakLineType == BreakLineType.Curvilinear)
            {
                if (Overhang > 0)
                {
                    pts.Add(GeometryHelpers.GetPerpendicularPoint2d(
                        insertionPoint,
                        GeometryHelpers.Point3dAtDirection(endPoint, insertionPoint, insertionPoint, Overhang / 100.0 * length),
                        -Overhang / 200.0 * length
                    ));
                    bulges.Add(length / 10 / length / 4 * 2);
                }
                // Первая точка, соответствующая ручке
                pts.Add(GeometryHelpers.ConvertPoint3dToPoint2d(insertionPoint));
                bulges.Add(length / 10 / length / 2 * 4);

                // Средняя точка
                pts.Add(GeometryHelpers.Point2dAtDirection(insertionPoint, endPoint, insertionPoint, length / 2));
                bulges.Add(-length / 10 / length / 2 * 4);
                // Конечная точка, соответствующая ручке
                pts.Add(GeometryHelpers.Point2dAtDirection(insertionPoint, endPoint, insertionPoint, length));
                bulges.Add(0);
                if (Overhang > 0)
                {
                    pts.Add(GeometryHelpers.GetPerpendicularPoint2d(
                        insertionPoint,
                        GeometryHelpers.Point3dAtDirection(insertionPoint, endPoint, endPoint, Overhang / 100.0 * length),
                        -Overhang / 200.0 * length
                    ));
                    bulges.Add(length / 10 / length / 4 * 2);
                }
            }
            if (BreakLineType == BreakLineType.Cylindrical)
            {
                // first
                pts.Add(GeometryHelpers.ConvertPoint3dToPoint2d(insertionPoint));
                bulges.Add(-0.392699081698724);
                pts.Add(GeometryHelpers.GetPerpendicularPoint2d(
                    insertionPoint,
                    GeometryHelpers.Point3dAtDirection(insertionPoint, endPoint, insertionPoint, length / 10.0),
                    length / 10
                ));
                bulges.Add(-length / 10 / length / 2 * 3);
                //center
                pts.Add(GeometryHelpers.Point2dAtDirection(insertionPoint, endPoint, insertionPoint, length / 2));
                bulges.Add(length / 10 / length / 2 * 3);
                pts.Add(GeometryHelpers.GetPerpendicularPoint2d(
                    insertionPoint,
                    GeometryHelpers.Point3dAtDirection(insertionPoint, endPoint, insertionPoint, length - (length / 10.0)),
                    -length / 10
                    ));
                bulges.Add(0.392699081698724);
                // endpoint
                pts.Add(GeometryHelpers.Point2dAtDirection(insertionPoint, endPoint, insertionPoint, length));
                bulges.Add(0.392699081698724);
                pts.Add(GeometryHelpers.GetPerpendicularPoint2d(
                    insertionPoint,
                    GeometryHelpers.Point3dAtDirection(insertionPoint, endPoint, insertionPoint, length - (length / 10.0)),
                    length / 10
                ));
                bulges.Add(length / 10 / length / 2 * 3);
                pts.Add(GeometryHelpers.Point2dAtDirection(insertionPoint, endPoint, insertionPoint, length / 2));
                bulges.Add(0.0);
            }
            return pts;
        }
        
        /// <summary>Изменение точек полилинии</summary>
        /// <param name="pts">Коллекция 2Д точек</param>
        /// <param name="bulges">Список выпуклостей</param>
        private void FillMainPolylineWithPoints(Point2dCollection pts, IList<double> bulges)
        {
            // Если количество точек совпадает, тогда просто их меняем
            if (pts.Count == MainPolyline.NumberOfVertices)
            {
                for (var i = 0; i < pts.Count; i++)
                {
                    MainPolyline.SetPointAt(i, pts[i]);
                    MainPolyline.SetBulgeAt(i, bulges[i]);
                }
            }
            else // иначе создаем заново
            {
                for (var i = 0; i < MainPolyline.NumberOfVertices; i++)
                    MainPolyline.RemoveVertexAt(i);
                for (var i = 0; i < pts.Count; i++)
                    MainPolyline.AddVertexAt(i, pts[i], bulges[i], 0.0, 0.0);
            }
        }
        #endregion

        #region Style

        /// <summary>Идентификатор стиля</summary>
        public string StyleGuid { get; set; } = "00000000-0000-0000-0000-000000000000";

        /// <summary>Применение стиля по сути должно переопределять текущие параметры</summary>
        public void ApplyStyle(BreakLineStyle style)
        {
            // apply settings from style
            Overhang = StyleHelpers.GetPropertyValue(style, nameof(Overhang), BreakLineProperties.Overhang.DefaultValue);
            BreakHeight = StyleHelpers.GetPropertyValue(style, nameof(BreakHeight), BreakLineProperties.BreakHeight.DefaultValue);
            BreakWidth = StyleHelpers.GetPropertyValue(style, nameof(BreakWidth), BreakLineProperties.BreakWidth.DefaultValue);
            Scale = MainStaticSettings.Settings.UseScaleFromStyle 
                ? StyleHelpers.GetPropertyValue(style, nameof(Scale), BreakLineProperties.Scale.DefaultValue) 
                : AcadHelpers.Database.Cannoscale;
            LineTypeScale = StyleHelpers.GetPropertyValue(style, nameof(LineTypeScale), BreakLineProperties.LineTypeScale.DefaultValue);
            // set layer
            var layerName = StyleHelpers.GetPropertyValue(style, BreakLineProperties.LayerName.Name,
                BreakLineProperties.LayerName.DefaultValue);
            AcadHelpers.SetLayerByName(BlockId, layerName, style.LayerXmlData);
        }
        #endregion

        public override ResultBuffer GetParametersForXData()
        {
            try
            {
                // ReSharper disable once UseObjectOrCollectionInitializer
                var resBuf = new ResultBuffer();
                // 1001 - DxfCode.ExtendedDataRegAppName. AppName
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, BreakLineFunction.MPCOEntName));
                // Вектор от конечной точки до начальной с учетом масштаба блока и трансформацией блока
                var vector = EndPointOCS - InsertionPointOCS;
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataXCoordinate, new Point3d(vector.X, vector.Y, vector.Z))); //1010
                // Текстовые значения (код 1000)
                // Стиль
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, StyleGuid)); // 0
                // Тип разрыва
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, BreakLineType.ToString())); // 1
                // scale
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, Scale.Name)); // 2
                // Целочисленные значения (код 1070)
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, Overhang)); // 0
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, BreakHeight)); // 1
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, BreakWidth)); // 2
                // Значения типа double (dxfCode 1040)
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataReal, LineTypeScale)); // 0

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
                 * Для каждого значения с повторяющимся кодом назначен свой индек (см. метод GetParametersForXData)
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
                                        BreakLineType = BreakLinePropertiesHelpers.GetBreakLineTypeFromString(typedValue.Value.ToString());
                                        break;
                                    case 2:
                                        Scale = AcadHelpers.GetAnnotationScaleByName(typedValue.Value.ToString());
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
                                        Overhang = (Int16)typedValue.Value;
                                        break;
                                    case 1:
                                        BreakHeight = (Int16)typedValue.Value;
                                        break;
                                    case 2:
                                        BreakWidth = (Int16)typedValue.Value;
                                        break;
                                }
                                //index
                                index1070++;
                                break;
                            }
                        case DxfCode.ExtendedDataReal:
                            {
                                if (index1040 == 0) // 0 - LineTypeScale
                                    LineTypeScale = (double)typedValue.Value;
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

        internal enum UpdateVariant
        {
            SetInsertionPoint,
            SetEndPointMinLength
        }

        public static BreakLine GetBreakLineFromEntity(Entity ent)
        {
            using (ResultBuffer resBuf = ent.GetXDataForApplication(BreakLineFunction.MPCOEntName))
            {
                // В случае команды ОТМЕНА может вернуть null
                if (resBuf == null) return null;
                BreakLine breakLine = new BreakLine(ent.ObjectId);
                // Получаем параметры из самого блока
                // ОБЯЗАТЕЛЬНО СНАЧАЛА ИЗ БЛОКА!!!!!!
                breakLine.GetParametersFromEntity(ent);
                // Получаем параметры из XData
                breakLine.GetParametersFromResBuf(resBuf);

                return breakLine;
            }
        }
    }
}
