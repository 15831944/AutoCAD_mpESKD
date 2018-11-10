namespace mpESKD.Functions.mpGroundLine
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Helpers;
    using Base.Styles;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using Overrules;
    using Properties;
    using Styles;
    using Exception = Autodesk.AutoCAD.Runtime.Exception;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GroundLineFunction : IMPCOEntityFunction
    {
        /// <summary>Имя примитива, помещаемое в XData</summary>
        public static readonly string MPCOEntName = GroundLineInterface.Name; // mpGroundLine

        /// <summary>Отображаемое имя примитива</summary>
        public static string MPCOEntDisplayName = GroundLineInterface.LName; // Линия грунта

        public void Initialize()
        {
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), GroundLineGripPointOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), GroundLineOsnapOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), GroundLineObjectOverrule.Instance(), true);

            GroundLineStyleManager.CheckStylesFile();
        }

        public void Terminate()
        {
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), GroundLineGripPointOverrule.Instance());
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), GroundLineOsnapOverrule.Instance());
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), GroundLineObjectOverrule.Instance());
        }
    }

    public class GroundLineCommands
    {
        [CommandMethod("ModPlus", "mpGroundLine", CommandFlags.Modal)]
        public void CreateGroundLineCommand()
        {
            CreateGroundLine();
        }

        [CommandMethod("ModPlus", "mpGroundLineFromPolyline", CommandFlags.Modal)]
        public void CreateGroundLineFromPolyline()
        {
            MessageBox.Show("YES!");
        }

        private void CreateGroundLine()
        {
            // send statistic
            Statistic.SendCommandStarting(GroundLineFunction.MPCOEntName, MpVersionData.CurCadVers);
            try
            {
                Overrule.Overruling = false;

                /* Регистрация ЕСКД приложения должна запускаться при запуске
                 * функции, т.к. регистрация происходит в текущем документе
                 * При инициализации плагина регистрации нет!
                 */
                ExtendedDataHelpers.AddRegAppTableRecord(GroundLineFunction.MPCOEntName);
                var style = GroundLineStyleManager.GetCurrentStyle();
                var layerName = StyleHelpers.GetPropertyValue(style, GroundLineProperties.LayerName.Name,
                    GroundLineProperties.LayerName.DefaultValue);
                var groundLine = new GroundLine(style);
                var blockReference = MainFunction.CreateBlock(groundLine);

                // set layer
                AcadHelpers.SetLayerByName(blockReference.ObjectId, layerName, style.LayerXmlData);

                var breakLoop = false;
                while (!breakLoop)
                {
                    var breakLineJig = new GroundLineJig(groundLine, blockReference);
                    do
                    {
                        var status = AcadHelpers.Editor.Drag(breakLineJig).Status;
                        if (status == PromptStatus.OK)
                        {
                            breakLineJig.JigState = GroundLineJigState.PromptNextPoint;
                            if (breakLineJig.PreviousPoint == null)
                            {
                                breakLineJig.PreviousPoint = groundLine.MiddlePoints.Any()
                                    ? groundLine.MiddlePoints.Last() 
                                    : groundLine.InsertionPoint;
                            }
                            else
                            {
                                groundLine.RebasePoints();
                                breakLineJig.PreviousPoint = groundLine.MiddlePoints.Last();
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
                                        var obj = (BlockReference)tr.GetObject(blockReference.Id, OpenMode.ForWrite);
                                        obj.Erase(true);
                                        tr.Commit();
                                    }
                                }
                            }
                            breakLoop = true;
                        }
                    } while (!breakLoop);
                }

                if (!groundLine.BlockId.IsErased)
                {
                    using (var tr = AcadHelpers.Database.TransactionManager.StartTransaction())
                    {
                        var ent = tr.GetObject(groundLine.BlockId, OpenMode.ForWrite);
                        ent.XData = groundLine.GetParametersForXData();
                        tr.Commit();
                    }
                }
            }
            catch (Exception exception)
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
