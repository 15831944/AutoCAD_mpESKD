﻿namespace mpESKD.Functions.mpAxis
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Enums;
    using Base.Helpers;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using Base.Styles;
    using Overrules;

    public class AxisFunction : IIntellectualEntityFunction
    {
        public void Initialize()
        {
            // Включение работы переопределения ручек (нужна регенерация в конце метода (?))
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), AxisGripPointsOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), AxisOsnapOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), AxisObjectOverrule.Instance(), true);
        }

        public static void DoubleClickEdit(BlockReference blockReference, Autodesk.AutoCAD.Geometry.Point3d location, Transaction tr)
        {
            BeditCommandWatcher.UseBedit = false;
            var axis = EntityReaderFactory.Instance.GetFromEntity<Axis>(blockReference);
            axis.UpdateEntities();
            bool saveBack = false;
            if (MainStaticSettings.Settings.AxisUsePluginTextEditor)
            {
                AxisValueEditor axisValueEditor = new AxisValueEditor { Axis = axis };
                if (axisValueEditor.ShowDialog() == true)
                    saveBack = true;
            }
            else
            {
                MessageBox.Show(Language.GetItem(MainFunction.LangItem, "msg4"));
                //var v = axis.BottomFirstDBText.Position.TransformBy(axis.BlockTransform);
                //AcadHelpers.WriteMessageInDebug("\nLocation:");

                //// точка двойного клика:
                //AcadHelpers.WriteMessageInDebug("\nStandard: " + location); 
                ////AcadHelpers.WriteMessageInDebug("\nTransform: " + location.TransformBy(axis.BlockTransform));
                ////AcadHelpers.WriteMessageInDebug("\nTransform inverse: " + location.TransformBy(axis.BlockTransform.Inverse()));
                //AcadHelpers.WriteMessageInDebug("\nTexr point:");
                //AcadHelpers.WriteMessageInDebug("\nStandard:" + axis.BottomFirstDBText.Position);

                //// точка текста, трансформируемая в координаты блока
                //AcadHelpers.WriteMessageInDebug("\nTransform:" + axis.BottomFirstDBText.Position.TransformBy(axis.BlockTransform));
                ////AcadHelpers.WriteMessageInDebug("\n" + (v - location).Length.ToString(CultureInfo.InvariantCulture));

                //var displMat = Matrix3d.Displacement(blockReference.Position - Point3d.Origin);
                //var btr = (BlockTableRecord)tr.GetObject(blockReference.BlockTableRecord, OpenMode.ForWrite);
                //foreach (ObjectId objectId in btr)
                //{
                //    var ent = tr.GetObject(objectId, OpenMode.ForWrite);
                //    if (ent is DBText text && text.Visible)
                //    {
                //        AcadHelpers.WriteMessageInDebug("text position: " + text.Position);
                //        //var text = axis.BottomFirstDBText;
                //        text.Visible = false;
                //        blockReference.RecordGraphicsModified(true);
                //        AcadHelpers.Document.TransactionManager.FlushGraphics();
                //        text.TransformBy(displMat);
                //        ObjectId[] ids = new ObjectId[0];
                //        InplaceTextEditor.Invoke(text, ref ids);
                //        if (text.IsModified)
                //        {
                //            // 
                //        }
                //        text.TransformBy(displMat.Inverse());
                //        text.Visible = true;
                //        blockReference.RecordGraphicsModified(true);
                //        AcadHelpers.Document.TransactionManager.FlushGraphics();
                //    }
                //}
            }
            if (saveBack)
            {
                using (var resBuf = axis.GetDataForXData())
                {
                    blockReference.XData = resBuf;
                }
                axis.UpdateEntities();
                axis.BlockRecord.UpdateAnonymousBlocks();
            }
            axis.Dispose();
        }

        public void Terminate()
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
            Statistic.SendCommandStarting(AxisDescriptor.Instance.Name, MpVersionData.CurCadVers);
            try
            {
                Overrule.Overruling = false;
                
                /* Регистрация ЕСКД приложения должна запускаться при запуске
                 * функции, т.к. регистрация происходит в текущем документе
                 * При инициализации плагина регистрации нет!
                 */
                ExtendedDataHelpers.AddRegAppTableRecord(AxisDescriptor.Instance.Name);

                var style = StyleManager.GetCurrentStyle(typeof(Axis));

                // add layer from style
                var axisLastHorizontalValue = string.Empty;
                var axisLastVerticalValue = string.Empty;
                if (MainStaticSettings.Settings.AxisSaveLastTextAndContinueNew)
                {
                    axisLastHorizontalValue = ModPlus.Helpers.XDataHelpers.GetStringXData("AxisLastValueForHorizontal");
                    axisLastVerticalValue = ModPlus.Helpers.XDataHelpers.GetStringXData("AxisLastValueForVertical");
                }
                var axis = new Axis(axisLastHorizontalValue, axisLastVerticalValue);

                var blockReference = MainFunction.CreateBlock(axis);
                axis.ApplyStyle(style, true);
               
                    var entityJig = new DefaultEntityJig(
                        axis,
                        blockReference,
                        new Point3d(0, -1, 0),
                        Language.GetItem(MainFunction.LangItem, "msg2"));
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
                            using (AcadHelpers.Document.LockDocument())
                            {
                                using (var tr = AcadHelpers.Document.TransactionManager.StartTransaction())
                                {
                                    var obj = (BlockReference)tr.GetObject(blockReference.Id, OpenMode.ForWrite);
                                    obj.Erase(true);
                                    tr.Commit();
                                }
                            }
                            break;
                        }
                    } while (true);
                
                if (!axis.BlockId.IsErased)
                {
                    using (var tr = AcadHelpers.Database.TransactionManager.StartTransaction())
                    {
                        var ent = tr.GetObject(axis.BlockId, OpenMode.ForWrite);
                        ent.XData = axis.GetDataForXData();
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
    }
}
