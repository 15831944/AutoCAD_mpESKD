using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using mpESKD.Base.Helpers;
using mpESKD.Functions.mpAxis.Properties;
using mpESKD.Functions.mpAxis.Styles;
using ModPlusAPI;
using ModPlusAPI.Windows;
using mpESKD.Base.Styles;
using mpESKD.Functions.mpAxis.Overrules;

namespace mpESKD.Functions.mpAxis
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class AxisFunction
    {
        /// <summary>Имя примитива, помещаемое в XData</summary>
        public const string MPCOEntName = "mpAxis";
        /// <summary>Отображаемое имя примитива</summary>
        public const string MPCOEntDisplayName = "Прямая ось";

        public static void Initialize()
        {
            // Включение работы переопределения ручек (нужна регенерация в конце метода (?))
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), AxisGripPointsOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), AxisOsnapOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), AxisObjectOverrule.Instance(), true);
            Overrule.Overruling = true;
            //// создание файла хранения стилей, если отсутсвует
            AxisStyleManager.CheckStylesFile();
        }
        public static void Terminate()
        {
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), AxisGripPointsOverrule.Instance());
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), AxisOsnapOverrule.Instance());
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), AxisObjectOverrule.Instance());
        }
        public static List<string> AxisRusAlphabet = new List<string>
        {
            "А","Б","В","Г","Д","Е","Ж","И","К","Л","М","Н","П","Р","С","Т","У","Ф","Ш","Э","Ю","Я",
            "АА","ББ","ВВ","ГГ","ДД","ЕЕ","ЖЖ","ИИ","КК","ЛЛ","ММ","НН","ПП","РР","СС","ТТ","УУ","ФФ","ШШ","ЭЭ","ЮЮ","ЯЯ"
        };
    }

    public class AxisCommands
    {
        [CommandMethod("ModPlus", "mpAxis", CommandFlags.Modal)]
        public void CreateAxisCommand()
        {
            CreateAxis();
        }

        private static void CreateAxis()
        {
            // send statistic
            Statistic.SendCommandStarting(AxisFunction.MPCOEntName, MpVersionData.CurCadVers);
            try
            {
                Overrule.Overruling = false;
                /* Регистрация СПДС приложения должна запускаться при запуске
                 * функции, т.к. регистрация происходит в текущем документе
                 * При инициализации плагина регистрации нет!
                 */
                ExtendedDataHelpers.AddRegAppTableRecord(AxisFunction.MPCOEntName);
                // add layer from style
                var style = AxisStyleManager.GetCurrentStyle();
                var layerName = StyleHelpers.GetPropertyValue(style, AxisProperties.LayerName.Name,
                    AxisProperties.LayerName.DefaultValue);
                var axisLastHorizontalValue = string.Empty;
                var axisLastVerticalValue = string.Empty;
                if (MainStaticSettings.Settings.AxisSaveLastTextAndContinueNew)
                {
                    axisLastHorizontalValue = ModPlus.Helpers.XDataHelpers.GetStringXData("AxisLastValueForHorizontal");
                    axisLastVerticalValue = ModPlus.Helpers.XDataHelpers.GetStringXData("AxisLastValueForVertical");
                }
                var axis = new Axis(style, axisLastHorizontalValue, axisLastVerticalValue);
                var blockReference = CreateAxisBlock(ref axis);
                // set layer
                AcadHelpers.SetLayerByName(blockReference.ObjectId, layerName, style.LayerXmlData);
                // set linetype
                var lineType = StyleHelpers.GetPropertyValue(style, AxisProperties.LineType.Name,
                    AxisProperties.LineType.DefaultValue);
                AcadHelpers.SetLineType(blockReference.ObjectId, lineType);


                var breakLoop = false;
                while (!breakLoop)
                {
                    var axisJig = new AxisJig(axis, blockReference);
                    do
                    {
                        label0:
                        var status = AcadHelpers.Editor.Drag(axisJig).Status;
                        if (status == PromptStatus.OK)
                        {
                            if (axisJig.JigState != AxisJigState.PromptInsertPoint)
                            {
                                breakLoop = true;
                                status = PromptStatus.Other;
                            }
                            else
                            {
                                axisJig.JigState = AxisJigState.PromptEndPoint;
                                goto label0;
                            }
                        }
                        else if (status != PromptStatus.Other)
                        {
                            using (AcadHelpers.Document.LockDocument())
                            {
                                using (var tr = AcadHelpers.Document.TransactionManager.StartTransaction())
                                {
                                    var obj = (BlockReference)tr.GetObject(blockReference.Id, OpenMode.ForWrite);
                                    obj.Erase(true);
                                    tr.Commit();
                                }
                            }
                            breakLoop = true;
                        }
                        else
                        {
                            axis.UpdateEntities();
                            axis.BlockRecord.UpdateAnonymousBlocks();
                        }
                    } while (!breakLoop);
                }
                if (!axis.BlockId.IsErased)
                {
                    using (var tr = AcadHelpers.Database.TransactionManager.StartTransaction())
                    {
                        var ent = tr.GetObject(axis.BlockId, OpenMode.ForWrite);
                        ent.XData = axis.GetParametersForXData();
                        tr.Commit();
                    }
                    // save first marker value to doc
                    var v = (axis.EndPoint - axis.InsertionPoint).GetNormal();
                    if ((v.X > 0.5 || v.X < -0.5) && (v.Y < 0.5 || v.Y > -0.5))
                        ModPlus.Helpers.XDataHelpers.SetStringXData("AxisLastValueForHorizontal", axis.FirstText);
                    else
                        ModPlus.Helpers.XDataHelpers.SetStringXData("AxisLastValueForVertical", axis.FirstText);

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
        private static BlockReference CreateAxisBlock(ref Axis axis)
        {
            BlockReference blockReference;
            using (AcadHelpers.Document.LockDocument())
            {
                ObjectId objectId;
                using (var transaction = AcadHelpers.Document.TransactionManager.StartTransaction())
                {
                    using (var blockTable = AcadHelpers.Database.BlockTableId.Write<BlockTable>())
                    {
                        var blockTableRecordObjectId = blockTable.Add(axis.BlockRecord);
                        blockReference = new BlockReference(axis.InsertionPoint, blockTableRecordObjectId);
                        using (var blockTableRecord = AcadHelpers.Database.CurrentSpaceId.Write<BlockTableRecord>())
                        {
                            blockTableRecord.BlockScaling = BlockScaling.Uniform;
                            objectId = blockTableRecord.AppendEntity(blockReference);
                        }
                        transaction.AddNewlyCreatedDBObject(blockReference, true);
                        transaction.AddNewlyCreatedDBObject(axis.BlockRecord, true);
                    }
                    transaction.Commit();
                }
                axis.BlockId = objectId;
            }
            return blockReference;
        }
    }
}
