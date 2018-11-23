namespace mpESKD.Functions.mpGroundLine
{
    using System.Linq;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using Base.Styles;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using Overrules;

    public class GroundLineFunction : IIntellectualEntityFunction
    {
        /// <inheritdoc />
        public void Initialize()
        {
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), GroundLineGripPointOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), GroundLineOsnapOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), GroundLineObjectOverrule.Instance(), true);
        }

        /// <inheritdoc />
        public void Terminate()
        {
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), GroundLineGripPointOverrule.Instance());
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), GroundLineOsnapOverrule.Instance());
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), GroundLineObjectOverrule.Instance());
        }

        /// <inheritdoc />
        public void CreateAnalog(IntellectualEntity sourceEntity)
        {
            // send statistic
            Statistic.SendCommandStarting(GroundLineDescriptor.Instance.Name, MpVersionData.CurCadVers);

            try
            {
                Overrule.Overruling = false;

                /* Регистрация ЕСКД приложения должна запускаться при запуске
                 * функции, т.к. регистрация происходит в текущем документе
                 * При инициализации плагина регистрации нет!
                 */
                ExtendedDataHelpers.AddRegAppTableRecord(GroundLineDescriptor.Instance.Name);
                
                var groundLine = new GroundLine();
                var blockReference = MainFunction.CreateBlock(groundLine);

                groundLine.SetPropertiesFromIntellectualEntity(sourceEntity);

                InsertGroundLineWithJig(groundLine, blockReference);
            }
            catch (System.Exception exception)
            {
                ExceptionBox.Show(exception);
            }
            finally
            {
                Overrule.Overruling = true;
            }
        }

        [CommandMethod("ModPlus", "mpGroundLine", CommandFlags.Modal)]
        public void CreateGroundLineCommand()
        {
            CreateGroundLine();
        }

        [CommandMethod("ModPlus", "mpGroundLineFromPolyline", CommandFlags.Modal)]
        public void CreateGroundLineFromPolylineCommand()
        {
            CreateGroundLineFromPolyline();
        }

        private void CreateGroundLine()
        {
            // send statistic
            Statistic.SendCommandStarting(GroundLineDescriptor.Instance.Name, MpVersionData.CurCadVers);

            try
            {
                Overrule.Overruling = false;

                /* Регистрация ЕСКД приложения должна запускаться при запуске
                 * функции, т.к. регистрация происходит в текущем документе
                 * При инициализации плагина регистрации нет!
                 */
                ExtendedDataHelpers.AddRegAppTableRecord(GroundLineDescriptor.Instance.Name);
                
                var style = StyleManager.GetCurrentStyle(typeof(GroundLine));
                var groundLine = new GroundLine();

                var blockReference = MainFunction.CreateBlock(groundLine);
                groundLine.ApplyStyle(style, true);

                InsertGroundLineWithJig(groundLine, blockReference);
            }
            catch (System.Exception exception)
            {
                ExceptionBox.Show(exception);
            }
            finally
            {
                Overrule.Overruling = true;
            }
        }

        private static void InsertGroundLineWithJig(GroundLine groundLine, BlockReference blockReference)
        {
            var entityJig = new DefaultEntityJig(
                groundLine,
                blockReference,
                new Point3d(20, 0, 0),
                Language.GetItem(Invariables.LangItem, "msg5"));
            do
            {
                var status = AcadHelpers.Editor.Drag(entityJig).Status;
                if (status == PromptStatus.OK)
                {
                    entityJig.JigState = JigState.PromptNextPoint;
                    if (entityJig.PreviousPoint == null)
                    {
                        entityJig.PreviousPoint = groundLine.MiddlePoints.Any()
                            ? groundLine.MiddlePoints.Last()
                            : groundLine.InsertionPoint;
                    }
                    else
                    {
                        groundLine.RebasePoints();
                        entityJig.PreviousPoint = groundLine.MiddlePoints.Last();
                    }
                }
                else
                {
                    if (groundLine.MiddlePoints.Any())
                    {
                        groundLine.EndPoint = groundLine.MiddlePoints.Last();
                        groundLine.MiddlePoints.RemoveAt(groundLine.MiddlePoints.Count - 1);
                        groundLine.UpdateEntities();
                        groundLine.BlockRecord.UpdateAnonymousBlocks();
                    }
                    else
                    {
                        // if no middle points - remove entity
                        using (AcadHelpers.Document.LockDocument())
                        {
                            using (var tr = AcadHelpers.Document.TransactionManager.StartTransaction())
                            {
                                var obj = (BlockReference) tr.GetObject(blockReference.Id, OpenMode.ForWrite);
                                obj.Erase(true);
                                tr.Commit();
                            }
                        }
                    }

                    break;
                }
            } while (true);

            if (!groundLine.BlockId.IsErased)
            {
                using (var tr = AcadHelpers.Database.TransactionManager.StartTransaction())
                {
                    var ent = tr.GetObject(groundLine.BlockId, OpenMode.ForWrite);
                    ent.XData = groundLine.GetDataForXData();
                    tr.Commit();
                }
            }
        }

        private void CreateGroundLineFromPolyline()
        {
            // send statistic
            Statistic.SendCommandStarting("mpGroundLineFromPolyline", MpVersionData.CurCadVers);
            try
            {
                var peo = new PromptEntityOptions("\n" + Language.GetItem(Invariables.LangItem, "msg6"))
                {
                    AllowNone = false,
                    AllowObjectOnLockedLayer = true
                };
                peo.SetRejectMessage("\n" + Language.GetItem(Invariables.LangItem, "wrong"));
                peo.AddAllowedClass(typeof(Polyline), true);

                var per = AcadHelpers.Editor.GetEntity(peo);
                if (per.Status != PromptStatus.OK) return;

                /* Регистрация ЕСКД приложения должна запускаться при запуске
                 * функции, т.к. регистрация происходит в текущем документе
                 * При инициализации плагина регистрации нет!
                 */
                ExtendedDataHelpers.AddRegAppTableRecord(GroundLineDescriptor.Instance.Name);
                //style
                var style = StyleManager.GetCurrentStyle(typeof(GroundLine));
                var groundLine = new GroundLine();

                MainFunction.CreateBlock(groundLine);
                groundLine.ApplyStyle(style, true);

                var plineId = per.ObjectId;

                using (AcadHelpers.Document.LockDocument(DocumentLockMode.ProtectedAutoWrite, null, null, true))
                {
                    using (var tr = AcadHelpers.Document.TransactionManager.StartOpenCloseTransaction())
                    {
                        var dbObj = tr.GetObject(plineId, OpenMode.ForRead);
                        if (dbObj is Polyline pline)
                        {
                            for (int i = 0; i < pline.NumberOfVertices; i++)
                            {
                                if (i == 0)
                                    groundLine.InsertionPoint = pline.GetPoint3dAt(i);
                                else if (i == pline.NumberOfVertices - 1)
                                    groundLine.EndPoint = pline.GetPoint3dAt(i);
                                else
                                    groundLine.MiddlePoints.Add(pline.GetPoint3dAt(i));
                            }

                            groundLine.UpdateEntities();
                            groundLine.BlockRecord.UpdateAnonymousBlocks();

                            var ent = (BlockReference)tr.GetObject(groundLine.BlockId, OpenMode.ForWrite);
                            ent.Position = pline.GetPoint3dAt(0);
                            ent.XData = groundLine.GetDataForXData();
                        }
                        tr.Commit();
                    }

                    AcadHelpers.Document.TransactionManager.QueueForGraphicsFlush();
                    AcadHelpers.Document.TransactionManager.FlushGraphics();

                    // "Удалить исходную полилинию?"
                    if (MessageBox.ShowYesNo(Language.GetItem(Invariables.LangItem, "msg7"), MessageBoxIcon.Question))
                    {
                        using (var tr = AcadHelpers.Document.TransactionManager.StartTransaction())
                        {
                            var dbObj = tr.GetObject(plineId, OpenMode.ForWrite);
                            dbObj.Erase(true);
                            tr.Commit();
                        }
                    }
                }
            }
            catch (System.Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
    }
}