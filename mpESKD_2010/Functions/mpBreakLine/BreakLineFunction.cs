using System.Diagnostics.CodeAnalysis;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using mpESKD.Functions.mpBreakLine.Overrules;
using mpESKD.Base.Helpers;
using mpESKD.Functions.mpBreakLine.Properties;
using mpESKD.Functions.mpBreakLine.Styles;
using ModPlusAPI;
using ModPlusAPI.Windows;

namespace mpESKD.Functions.mpBreakLine
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class BreakLineFunction
    {
        /// <summary>Имя примитива, помещаемое в XData</summary>
        public const string MPCOEntName = "mpBreakLine";
        /// <summary>Отображаемое имя примитива</summary>
        public const string MPCOEntDisplayName = "Линия обрыва";
        
        public static void Initialize()
        {
            // Включение работы переопределения ручек (нужна регенерация в конце метода (?))
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), BreakLineGripPointsOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), BreakLineOsnapOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), BreakLineObjectOverrule.Instance(), true);
            Overrule.Overruling = true;
            // создание файла хранения стилей, если отсутсвует
            BreakLineStylesManager.CheckStylesFile();
        }
        public static void Terminate()
        {
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), BreakLineGripPointsOverrule.Instance());
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), BreakLineOsnapOverrule.Instance());
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), BreakLineObjectOverrule.Instance());
        }
    }

    public class BreakLineCommands
    {
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
            Statistic.SendCommandStarting(BreakLineFunction.MPCOEntName, MpVersionData.CurCadVers);
            try
            {
                Overrule.Overruling = false;
                /* Регистрация СПДС приложения должна запускаться при запуске
                 * функции, т.к. регистрация происходит в текущем документе
                 * При инициализации плагина регистрации нет!
                 */
                ExtendedDataHelpers.AddRegAppTableRecord(BreakLineFunction.MPCOEntName);
                //
                var breakLine = new BreakLine(BreakLineStylesManager.GetCurrentStyle())
                {
                    BreakLineType = breakLineType
                };
                var blockReference = CreateBreakLineBlock(ref breakLine);
                var breakLoop = false;
                while (!breakLoop)
                {
                    var breakLineJig = new BreakLineJig(breakLine, blockReference);
                    do
                    {
                        label0:
                        var status = AcadHelpers.Editor.Drag(breakLineJig).Status;
                        if (status == PromptStatus.OK)
                        {
                            if (breakLineJig.JigState != BreakLineJigState.PromptInsertPoint)
                            {
                                breakLoop = true;
                                status = PromptStatus.Other;
                            }
                            else
                            {
                                breakLineJig.JigState = BreakLineJigState.PromptEndPoint;
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
                            breakLine.UpdateEntities();
                            breakLine.BlockRecord.UpdateAnonymousBlocks();
                        }
                    } while (!breakLoop);
                }
                if (!breakLine.BlockId.IsErased)
                {
                    using (var tr = AcadHelpers.Database.TransactionManager.StartTransaction())
                    {
                        var ent = tr.GetObject(breakLine.BlockId, OpenMode.ForWrite);
                        ent.XData = breakLine.GetParametersForXData();
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
        private static BlockReference CreateBreakLineBlock(ref BreakLine breakLine)
        {
            BlockReference blockReference;
            using (AcadHelpers.Document.LockDocument())
            {
                ObjectId objectId;
                using (var transaction = AcadHelpers.Document.TransactionManager.StartTransaction())
                {
                    using (var blockTable = AcadHelpers.Database.BlockTableId.Write<BlockTable>())
                    {
                        var blockTableRecordObjectId = blockTable.Add(breakLine.BlockRecord);
                        blockReference = new BlockReference(breakLine.InsertionPoint, blockTableRecordObjectId);
                        using (var blockTableRecord = AcadHelpers.Database.CurrentSpaceId.Write<BlockTableRecord>())
                        {
                            blockTableRecord.BlockScaling = BlockScaling.Uniform;
                            objectId = blockTableRecord.AppendEntity(blockReference);
                        }
                        transaction.AddNewlyCreatedDBObject(blockReference, true);
                        transaction.AddNewlyCreatedDBObject(breakLine.BlockRecord, true);
                    }
                    transaction.Commit();
                }
                breakLine.BlockId = objectId;
            }
            return blockReference;
        }
    }

}
