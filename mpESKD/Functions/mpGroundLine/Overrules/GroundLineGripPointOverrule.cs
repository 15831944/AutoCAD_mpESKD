namespace mpESKD.Functions.mpGroundLine.Overrules
{
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Helpers;
    using Grips;
    using ModPlusAPI.Windows;

    public class GroundLineGripPointOverrule : GripOverrule
    {
        private static GroundLineGripPointOverrule _instance;

        public static GroundLineGripPointOverrule Instance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = new GroundLineGripPointOverrule();

            // Фильтр "отлова" примитива по расширенным данным. Работает лучше, чем проверка вручную!
            _instance.SetXDataFilter(GroundLineDescriptor.Instance.Name);
            return _instance;
        }

        public override void GetGripPoints(Entity entity, GripDataCollection grips, double curViewUnitSize, int gripSize, Vector3d curViewDir,
            GetGripPointsFlags bitFlags)
        {
            try
            {
                if (IsApplicable(entity))
                {
                    // Удаляю все ручки - это удалит ручку вставки блока
                    grips.Clear();

                    var groundLine = EntityReaderFactory.Instance.GetFromEntity<GroundLine>(entity);
                    if (groundLine != null)
                    {
                        // Если средних точек нет, значит линия состоит всего из двух точек
                        // в этом случае не нужно добавлять точки удаления крайних вершин

                        // insertion (start) grip
                        var vertexGrip = new GroundLineVertexGrip(groundLine, 0)
                        {
                            GripPoint = groundLine.InsertionPoint
                        };
                        grips.Add(vertexGrip);

                        if (groundLine.MiddlePoints.Any())
                        {
                            var removeVertexGrip = new GroundLineRemoveVertexGrip(groundLine, 0)
                            {
                                GripPoint = groundLine.InsertionPoint - (Vector3d.YAxis * 20 * curViewUnitSize)
                            };
                            grips.Add(removeVertexGrip);
                        }

                        // middle points
                        for (var index = 0; index < groundLine.MiddlePoints.Count; index++)
                        {
                            vertexGrip = new GroundLineVertexGrip(groundLine, index + 1)
                            {
                                GripPoint = groundLine.MiddlePoints[index]
                            };
                            grips.Add(vertexGrip);

                            var removeVertexGrip = new GroundLineRemoveVertexGrip(groundLine, index + 1)
                            {
                                GripPoint = groundLine.MiddlePoints[index] - (Vector3d.YAxis * 20 * curViewUnitSize)
                            };
                            grips.Add(removeVertexGrip);
                        }

                        // end point
                        vertexGrip = new GroundLineVertexGrip(groundLine, groundLine.MiddlePoints.Count + 1)
                        {
                            GripPoint = groundLine.EndPoint
                        };
                        grips.Add(vertexGrip);

                        if (groundLine.MiddlePoints.Any())
                        {
                            var removeVertexGrip = new GroundLineRemoveVertexGrip(groundLine, groundLine.MiddlePoints.Count + 1)
                            {
                                GripPoint = groundLine.EndPoint - (Vector3d.YAxis * 20 * curViewUnitSize)
                            };
                            grips.Add(removeVertexGrip);
                        }

                        #region AddVertex grips

                        // add vertex grips
                        for (var i = 0; i < groundLine.MiddlePoints.Count; i++)
                        {
                            if (i == 0)
                            {
                                var addVertexGrip = new GroundLineAddVertexGrip(
                                    groundLine,
                                    groundLine.InsertionPoint, groundLine.MiddlePoints[i])
                                {
                                    GripPoint = GeometryHelpers.GetMiddlePoint3d(groundLine.InsertionPoint, groundLine.MiddlePoints[i])
                                };
                                grips.Add(addVertexGrip);
                            }
                            else
                            {
                                var addVertexGrip = new GroundLineAddVertexGrip(
                                    groundLine,
                                    groundLine.MiddlePoints[i - 1], groundLine.MiddlePoints[i])
                                {
                                    GripPoint = GeometryHelpers.GetMiddlePoint3d(groundLine.MiddlePoints[i - 1], groundLine.MiddlePoints[i])
                                };
                                grips.Add(addVertexGrip);
                            }

                            // last segment
                            if (i == groundLine.MiddlePoints.Count - 1)
                            {
                                var addVertexGrip = new GroundLineAddVertexGrip(
                                    groundLine,
                                    groundLine.MiddlePoints[i], groundLine.EndPoint)
                                {
                                    GripPoint = GeometryHelpers.GetMiddlePoint3d(groundLine.MiddlePoints[i], groundLine.EndPoint)
                                };
                                grips.Add(addVertexGrip);
                            }
                        }

                        {
                            if (groundLine.MiddlePoints.Any())
                            {
                                var addVertexGrip = new GroundLineAddVertexGrip(groundLine, groundLine.EndPoint, null)
                                {
                                    GripPoint = groundLine.EndPoint +
                                                ((groundLine.EndPoint - groundLine.MiddlePoints.Last()).GetNormal() * 20 * curViewUnitSize)
                                };
                                grips.Add(addVertexGrip);

                                addVertexGrip = new GroundLineAddVertexGrip(groundLine, null, groundLine.InsertionPoint)
                                {
                                    GripPoint = groundLine.InsertionPoint +
                                                ((groundLine.InsertionPoint - groundLine.MiddlePoints.First()).GetNormal() * 20 * curViewUnitSize)
                                };
                                grips.Add(addVertexGrip);
                            }
                            else
                            {
                                var addVertexGrip = new GroundLineAddVertexGrip(groundLine, groundLine.EndPoint, null)
                                {
                                    GripPoint = groundLine.EndPoint +
                                                ((groundLine.InsertionPoint - groundLine.EndPoint).GetNormal() * 20 * curViewUnitSize)
                                };
                                grips.Add(addVertexGrip);

                                addVertexGrip = new GroundLineAddVertexGrip(groundLine, null, groundLine.EndPoint)
                                {
                                    GripPoint = groundLine.InsertionPoint +
                                                ((groundLine.EndPoint - groundLine.InsertionPoint).GetNormal() * 20 * curViewUnitSize)
                                };
                                grips.Add(addVertexGrip);

                                addVertexGrip = new GroundLineAddVertexGrip(groundLine, groundLine.InsertionPoint, groundLine.EndPoint)
                                {
                                    GripPoint = GeometryHelpers.GetMiddlePoint3d(groundLine.InsertionPoint, groundLine.EndPoint)
                                };
                                grips.Add(addVertexGrip);
                            }
                        }

                        #endregion

                        var reverseGrip = new GroundLineReverseGrip(groundLine)
                        {
                            GripPoint = groundLine.InsertionPoint + (Vector3d.YAxis * 20 * curViewUnitSize)
                        };
                        grips.Add(reverseGrip);
                        reverseGrip = new GroundLineReverseGrip(groundLine)
                        {
                            GripPoint = groundLine.EndPoint + (Vector3d.YAxis * 20 * curViewUnitSize)
                        };
                        grips.Add(reverseGrip);
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception.ErrorStatus != ErrorStatus.NotAllowedForThisProxy)
                    ExceptionBox.Show(exception);
            }
        }

        public override void MoveGripPointsAt(Entity entity, GripDataCollection grips, Vector3d offset, MoveGripPointsFlags bitFlags)
        {
            try
            {
                if (IsApplicable(entity))
                {
                    foreach (GripData gripData in grips)
                    {
                        if (gripData is GroundLineVertexGrip vertexGrip)
                        {
                            if (vertexGrip.GripIndex == 0)
                            {
                                ((BlockReference)entity).Position = vertexGrip.GripPoint + offset;
                                vertexGrip.GroundLine.InsertionPoint = vertexGrip.GripPoint + offset;
                            }
                            else if (vertexGrip.GripIndex == vertexGrip.GroundLine.MiddlePoints.Count + 1)
                            {
                                vertexGrip.GroundLine.EndPoint = vertexGrip.GripPoint + offset;
                            }
                            else
                            {
                                vertexGrip.GroundLine.MiddlePoints[vertexGrip.GripIndex - 1] =
                                    vertexGrip.GripPoint + offset;
                            }

                            // Вот тут происходит перерисовка примитивов внутри блока
                            vertexGrip.GroundLine.UpdateEntities();
                            vertexGrip.GroundLine.BlockRecord.UpdateAnonymousBlocks();
                        }
                        else if (gripData is GroundLineAddVertexGrip addVertexGrip)
                        {
                            addVertexGrip.NewPoint = addVertexGrip.GripPoint + offset;
                        }
                        else
                        {
                            base.MoveGripPointsAt(entity, grips, offset, bitFlags);
                        }
                    }
                }
                else
                {
                    base.MoveGripPointsAt(entity, grips, offset, bitFlags);
                }
            }
            catch (Exception exception)
            {
                if (exception.ErrorStatus != ErrorStatus.NotAllowedForThisProxy)
                    ExceptionBox.Show(exception);
            }
        }

        // Проверка поддерживаемости примитива
        // Проверка происходит по наличию XData с определенным AppName
        public override bool IsApplicable(RXObject overruledSubject)
        {
            return ExtendedDataHelpers.IsApplicable(overruledSubject, GroundLineDescriptor.Instance.Name);
        }
    }
}
