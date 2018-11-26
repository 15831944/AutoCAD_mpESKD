namespace mpESKD.Functions.mpAxis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    using Exception = Autodesk.AutoCAD.Runtime.Exception;

    public class AxisFunction : IIntellectualEntityFunction
    {
        /// <inheritdoc />
        public void Initialize()
        {
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), AxisGripPointsOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), AxisOsnapOverrule.Instance(), true);
            Overrule.AddOverrule(RXObject.GetClass(typeof(BlockReference)), AxisObjectOverrule.Instance(), true);
        }

        /// <inheritdoc />
        public void Terminate()
        {
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), AxisGripPointsOverrule.Instance());
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), AxisOsnapOverrule.Instance());
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(BlockReference)), AxisObjectOverrule.Instance());
        }

        public void CreateAnalog(IntellectualEntity sourceEntity)
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

                var axisLastHorizontalValue = string.Empty;
                var axisLastVerticalValue = string.Empty;
                FindLastAxisValues(ref axisLastHorizontalValue, ref axisLastVerticalValue);
                var axis = new Axis(axisLastHorizontalValue, axisLastVerticalValue);

                var blockReference = MainFunction.CreateBlock(axis);
                
                axis.SetPropertiesFromIntellectualEntity(sourceEntity);

                // Отключаю видимость кружков направления
                axis.TopOrientMarkerVisible = false;
                axis.BottomOrientMarkerVisible = false;
                
                InsertAxisWithJig(axis, blockReference);
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

                var axisLastHorizontalValue = string.Empty;
                var axisLastVerticalValue = string.Empty;
                FindLastAxisValues(ref axisLastHorizontalValue, ref axisLastVerticalValue);
                var axis = new Axis(axisLastHorizontalValue, axisLastVerticalValue);

                var blockReference = MainFunction.CreateBlock(axis);
                axis.ApplyStyle(style, true);

                InsertAxisWithJig(axis, blockReference);
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

        private static void InsertAxisWithJig(Axis axis, BlockReference blockReference)
        {
            var entityJig = new DefaultEntityJig(
                axis,
                blockReference,
                new Point3d(0, -1, 0),
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

            if (!axis.BlockId.IsErased)
            {
                using (var tr = AcadHelpers.Database.TransactionManager.StartTransaction())
                {
                    var ent = tr.GetObject(axis.BlockId, OpenMode.ForWrite);
                    ent.XData = axis.GetDataForXData();
                    tr.Commit();
                }
            }
        }

        /// <summary>
        /// Поиск последних цифровых и буквенных значений осей на текущем виде
        /// </summary>
        /// <param name="axisLastHorizontalValue"></param>
        /// <param name="axisLastVerticalValue"></param>
        private static void FindLastAxisValues(ref string axisLastHorizontalValue, ref string axisLastVerticalValue)
        {
            if (MainStaticSettings.Settings.AxisSaveLastTextAndContinueNew)
            {
                List<int> allIntegerValues = new List<int>();
                List<string> allLetterValues = new List<string>();
                AcadHelpers.GetAllIntellectualEntitiesInCurrentSpace<Axis>(typeof(Axis)).ForEach(a =>
                {
                    var s = a.FirstText;
                    if (int.TryParse(s, out var i))
                        allIntegerValues.Add(i);
                    else allLetterValues.Add(s);
                });
                if (allIntegerValues.Any())
                {
                    allIntegerValues.Sort();
                    axisLastVerticalValue = allIntegerValues.Last().ToString();
                }

                if (allLetterValues.Any())
                {
                    allLetterValues.Sort();
                    axisLastHorizontalValue = allLetterValues.Last();
                }
            }
        }

        public static void DoubleClickEdit(BlockReference blockReference, Point3d location, Transaction tr)
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
                MessageBox.Show(Language.GetItem(Invariables.LangItem, "msg4"));
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
                axis.UpdateEntities();
                axis.BlockRecord.UpdateAnonymousBlocks();
                using (var resBuf = axis.GetDataForXData())
                {
                    blockReference.XData = resBuf;
                }
            }
            axis.Dispose();
        }
    }
}
