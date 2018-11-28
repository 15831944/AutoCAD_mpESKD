namespace mpESKD.Functions.mpBreakLine
{
    using Autodesk.AutoCAD.Runtime;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Base;
    using Base.Enums;
    using Overrules;
    using Base.Helpers;
    using Base.Styles;
    using ModPlusAPI;
    using ModPlusAPI.Windows;

    public class BreakLineFunction : IIntellectualEntityFunction
    {
        /// <inheritdoc />
        public void Initialize()
        {
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), BreakLineGripPointsOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), BreakLineOsnapOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), BreakLineObjectOverrule.Instance(), true);
        }

        /// <inheritdoc />
        public void Terminate()
        {
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), BreakLineGripPointsOverrule.Instance());
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), BreakLineOsnapOverrule.Instance());
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), BreakLineObjectOverrule.Instance());
        }

        /// <inheritdoc />
        public void CreateAnalog(IntellectualEntity sourceEntity, bool copyLayer)
        {
            // send statistic
            Statistic.SendCommandStarting(BreakLineDescriptor.Instance.Name, MpVersionData.CurCadVers);
            try
            {
                Overrule.Overruling = false;
                /* Регистрация ЕСКД приложения должна запускаться при запуске
                 * функции, т.к. регистрация происходит в текущем документе
                 * При инициализации плагина регистрации нет!
                 */
                ExtendedDataHelpers.AddRegAppTableRecord(BreakLineDescriptor.Instance.Name);
                
                var breakLine = new BreakLine();
                var blockReference = MainFunction.CreateBlock(breakLine);
                
                breakLine.SetPropertiesFromIntellectualEntity(sourceEntity, copyLayer);

                InsertBreakLineWithJig(breakLine, blockReference);
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

        [CommandMethod("ModPlus", "mpBreakLine", CommandFlags.Modal)]
        public void CreateLinearBreakLine()
        {
            CreateBreakLine(BreakLineType.Linear);
        }
        [CommandMethod("ModPlus", "mpBreakLineCurve", CommandFlags.Modal)]
        public void CreateCurvilinearBreakLine()
        {
            CreateBreakLine(BreakLineType.Curvilinear);
        }
        [CommandMethod("ModPlus", "mpBreakLineCylinder", CommandFlags.Modal)]
        public void CreateCylindricalBreakLine()
        {
            CreateBreakLine(BreakLineType.Cylindrical);
        }

        private static void CreateBreakLine(BreakLineType breakLineType)
        {
            // send statistic
            Statistic.SendCommandStarting(BreakLineDescriptor.Instance.Name, MpVersionData.CurCadVers);
            try
            {
                Overrule.Overruling = false;
                /* Регистрация ЕСКД приложения должна запускаться при запуске
                 * функции, т.к. регистрация происходит в текущем документе
                 * При инициализации плагина регистрации нет!
                 */
                ExtendedDataHelpers.AddRegAppTableRecord(BreakLineDescriptor.Instance.Name);
                var style = StyleManager.GetCurrentStyle(typeof(BreakLine));
                var breakLine = new BreakLine();

                var blockReference = MainFunction.CreateBlock(breakLine);
                breakLine.ApplyStyle(style, true);
                breakLine.BreakLineType = breakLineType;

                InsertBreakLineWithJig(breakLine, blockReference);
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

        private static void InsertBreakLineWithJig(BreakLine breakLine, BlockReference blockReference)
        {
            var entityJig = new DefaultEntityJig(
                breakLine,
                blockReference,
                new Point3d(15, 0, 0),
                Language.GetItem(Invariables.LangItem, "msg2"));
            do
            {
                var status = AcadHelpers.Editor.Drag(entityJig).Status;
                if (status == PromptStatus.OK)
                {
                    if (entityJig.JigState == JigState.PromptInsertPoint)
                        entityJig.JigState = JigState.PromptNextPoint;
                    else break;
                }
                else
                {
                    // mark to remove
                    using (AcadHelpers.Document.LockDocument())
                    {
                        using (var tr = AcadHelpers.Document.TransactionManager.StartTransaction())
                        {
                            var obj = (BlockReference) tr.GetObject(blockReference.Id, OpenMode.ForWrite);
                            obj.Erase(true);
                            tr.Commit();
                        }
                    }

                    break;
                }
            } while (true);

            if (!breakLine.BlockId.IsErased)
            {
                using (var tr = AcadHelpers.Database.TransactionManager.StartTransaction())
                {
                    var ent = tr.GetObject(breakLine.BlockId, OpenMode.ForWrite);
                    ent.XData = breakLine.GetDataForXData();
                    tr.Commit();
                }
            }
        }
    }
}