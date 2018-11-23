namespace mpESKD.Functions.mpSection
{
    using System.Collections.Generic;
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
    using Overrules;

    public class SectionFunction : IIntellectualEntityFunction
    {
        /// <inheritdoc />
        public void Initialize()
        {
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), SectionGripPointOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), SectionOsnapOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), SectionObjectOverrule.Instance(), true);
        }

        /// <inheritdoc />
        public void Terminate()
        {
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), SectionGripPointOverrule.Instance());
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), SectionOsnapOverrule.Instance());
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), SectionObjectOverrule.Instance());
        }

        /// <inheritdoc />
        public void CreateAnalog(IntellectualEntity sourceEntity)
        {
            //todo release
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
            Statistic.SendCommandStarting(SectionDescriptor.Instance.Name, MpVersionData.CurCadVers);

            try
            {
                Overrule.Overruling = false;

                /* Регистрация ЕСКД приложения должна запускаться при запуске
                 * функции, т.к. регистрация происходит в текущем документе
                 * При инициализации плагина регистрации нет!
                 */
                ExtendedDataHelpers.AddRegAppTableRecord(SectionDescriptor.Instance.Name);

                var style = StyleManager.GetCurrentStyle(typeof(Section));
                var sectionLastLetterValue = string.Empty;
                var sectionLastIntegerValue = string.Empty;
                FindLastSectionValues(ref sectionLastLetterValue, ref sectionLastIntegerValue);
                var section = new Section(sectionLastIntegerValue, sectionLastLetterValue);

                var blockReference = MainFunction.CreateBlock(section);
                section.ApplyStyle(style, true);

                //todo implement isSimple variable
                var entityJig = new DefaultEntityJig(
                    section,
                    blockReference,
                    new Point3d(20, 0, 0),
                    Language.GetItem(Invariables.LangItem, "msg5"));
                do
                {
                    var status = AcadHelpers.Editor.Drag(entityJig).Status;
                    if (status == PromptStatus.OK)
                    {
                        if (isSimple)
                        {
                            if (entityJig.JigState == JigState.PromptInsertPoint)
                                entityJig.JigState = JigState.PromptNextPoint;
                            else break;
                        }
                        else
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

        /// <summary>
        /// Поиск последних цифровых и буквенных значений разрезов на текущем виде
        /// </summary>
        /// <param name="sectionLastLetterValue"></param>
        /// <param name="sectionLastIntegerValue"></param>
        private static void FindLastSectionValues(ref string sectionLastLetterValue, ref string sectionLastIntegerValue)
        {
            //todo implement settings
            //if (MainStaticSettings.Settings.SectionSaveLastTextAndContinueNew)
            //{
            //    List<int> allIntegerValues = new List<int>();
            //    List<string> allLetterValues = new List<string>();
            //    AcadHelpers.GetAllIntellectualEntitiesInCurrentSpace<Section>(typeof(Section)).ForEach(a =>
            //    {
            //        var s = a.Designation;
            //        if(int.TryParse(s, out var i))
            //            allIntegerValues.Add(i);
            //        else allLetterValues.Add(s);
            //    });
            //    if (allIntegerValues.Any())
            //    {
            //        allIntegerValues.Sort();
            //        sectionLastIntegerValue = allIntegerValues.Last().ToString();
            //    }

            //    if (allLetterValues.Any())
            //    {
            //        allLetterValues.Sort();
            //        sectionLastLetterValue = allLetterValues.Last();
            //    }
            //}
        }
    }
}
