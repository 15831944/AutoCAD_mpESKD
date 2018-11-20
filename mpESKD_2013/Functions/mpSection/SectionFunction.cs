namespace mpESKD.Functions.mpSection
{
    using System.Linq;
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

    public class SectionFunction : IIntellectualEntityFunction
    {
        public void Initialize()
        {
            //todo add overrules
            Overrule.Overruling = true;
        }

        public void Terminate()
        {
            //todo add overrules
        }

        [CommandMethod("ModPlus", "mpSection", CommandFlags.Modal)]
        public void CreateSectionCommand()
        {
            CreateSection(false);
        }

        [CommandMethod("ModPlus", "mpSectionSimply", CommandFlags.Modal)]
        public void CreateSimplySectionCommand()
        {
            CreateSection(true);
        }

        [CommandMethod("ModPlus", "mpSectionFromPolyline", CommandFlags.Modal)]
        public void CreateSectionFromPolylineCommand()
        {
            //todo release
        }

        private void CreateSection(bool isSimple)
        {
            // send statistic
            Statistic.SendCommandStarting(SectionInterface.Name, MpVersionData.CurCadVers);

            try
            {
                Overrule.Overruling = false;

                /* Регистрация ЕСКД приложения должна запускаться при запуске
                 * функции, т.к. регистрация происходит в текущем документе
                 * При инициализации плагина регистрации нет!
                 */
                ExtendedDataHelpers.AddRegAppTableRecord(SectionInterface.Name);

                var style = StyleManager.GetCurrentStyle(typeof(Section));
                var section = new Section();

                var blockReference = MainFunction.CreateBlock(section);
                section.ApplyStyle(style, true);

                var entityJig = new DefaultEntityJig(
                    section,
                    blockReference,
                    new Point3d(20, 0, 0),
                    Language.GetItem(MainFunction.LangItem, "msg5"));
                do
                {
                    var status = AcadHelpers.Editor.Drag(entityJig).Status;
                    if (status == PromptStatus.OK)
                    {
                        entityJig.JigState = JigState.PromptNextPoint;
                        if (entityJig.PreviousPoint == null)
                        {
                            entityJig.PreviousPoint = section.MiddlePoints.Any()
                                ? section.MiddlePoints.Last()
                                : section.InsertionPoint;
                        }
                        else
                        {
                            section.RebasePoints();
                            entityJig.PreviousPoint = section.MiddlePoints.Last();
                        }
                    }
                    else
                    {
                        if (section.MiddlePoints.Any())
                        {
                            section.EndPoint = section.MiddlePoints.Last();
                            section.MiddlePoints.RemoveAt(section.MiddlePoints.Count - 1);
                            section.UpdateEntities();
                            section.BlockRecord.UpdateAnonymousBlocks();
                        }
                        else
                        {
                            // if no middle points - remove entity
                            using (AcadHelpers.Document.LockDocument())
                            {
                                using (var tr = AcadHelpers.Document.TransactionManager.StartTransaction())
                                {
                                    var obj = (BlockReference)tr.GetObject(blockReference.Id, OpenMode.ForWrite);
                                    obj.Erase(true);
                                    tr.Commit();
                                }
                            }
                        }

                        break;
                    }
                } while (true);

                if (!section.BlockId.IsErased)
                {
                    using (var tr = AcadHelpers.Database.TransactionManager.StartTransaction())
                    {
                        var ent = tr.GetObject(section.BlockId, OpenMode.ForWrite);
                        ent.XData = section.GetDataForXData();
                        tr.Commit();
                    }
                }
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
    }
}
