using System;
using System.Xml.Linq;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using ModPlusAPI.Windows;

namespace mpESKD.Base.Helpers
{
    public static class LayerHelper
    {
        /// <summary>Проверка наличия слоя в документе по имени</summary>
        /// <param name="layerName">Имя слоя</param>
        public static bool HasLayer(string layerName)
        {
            using (AcadHelpers.Document.LockDocument())
            {
                using (OpenCloseTransaction tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    LayerTable lt = tr.GetObject(AcadHelpers.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                    if (lt != null)
                        if (lt.Has(layerName))
                            return true;
                }
            }
            return false;
        }
        /// <summary>Получение LayerTableRecord по имени слоя</summary>
        /// <param name="layerName">Имя слоя</param>
        public static LayerTableRecord GetLayerTableRecordByLayerName(string layerName)
        {
            using (AcadHelpers.Document.LockDocument())
            {
                using (OpenCloseTransaction tr = AcadHelpers.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    LayerTable lt = tr.GetObject(AcadHelpers.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                    if (lt != null)
                        foreach (ObjectId layerId in lt)
                        {
                            var layer = tr.GetObject(layerId, OpenMode.ForWrite) as LayerTableRecord;
                            if (layer != null && layer.Name.Equals(layerName))
                                return layer;
                        }
                }
            }
            return null;
        }
        /// <summary>Создание слоя в текущем документе по данным, сохраненным в файле стилей</summary>
        /// <param name="layerXElement"></param>
        public static bool AddLayerFromXelement(XElement layerXElement)
        {
            var layer = GetLayerFromXml(layerXElement);
            var layerCreated = false;
            if (layer != null)
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var tr = AcadHelpers.Database.TransactionManager.StartTransaction())
                    {
                        using (var lyrTbl = tr.GetObject(AcadHelpers.Database.LayerTableId, OpenMode.ForWrite) as LayerTable)
                        {
                            if (lyrTbl != null)
                            {
                                if (!lyrTbl.Has(layer.Name))
                                {
                                    lyrTbl.Add(layer);
                                    tr.AddNewlyCreatedDBObject(layer, true);
                                    layerCreated = true;
                                }
                                else layerCreated = true;
                            }
                        }
                        tr.Commit();
                    }
                }
            return layerCreated;
        }
        // save layers to xml
        public static XElement SetLayerXml(LayerTableRecord layerTableRecord)
        {
            if (layerTableRecord == null) return new XElement("LayerTableRecord");
            using (AcadHelpers.Document.LockDocument())
            {
                var layerTableRecordXml = new XElement("LayerTableRecord");
                layerTableRecordXml.SetAttributeValue("Name", layerTableRecord.Name); //string
                layerTableRecordXml.SetAttributeValue("IsFrozen", layerTableRecord.IsFrozen); //bool
                layerTableRecordXml.SetAttributeValue("IsHidden", layerTableRecord.IsHidden); //bool
                layerTableRecordXml.SetAttributeValue("Description", layerTableRecord.Description); //string
                layerTableRecordXml.SetAttributeValue("IsLocked", layerTableRecord.IsLocked); //bool
                layerTableRecordXml.SetAttributeValue("IsOff", layerTableRecord.IsOff); //bool
                layerTableRecordXml.SetAttributeValue("IsPlottable", layerTableRecord.IsPlottable); //bool
                layerTableRecordXml.SetAttributeValue("Color", layerTableRecord.Color.ColorIndex); //color
                layerTableRecordXml.SetAttributeValue("Linetype", GetLineTypeName(layerTableRecord.LinetypeObjectId)); //ObjectId
                layerTableRecordXml.SetAttributeValue("LineWeight", layerTableRecord.LineWeight); //LineWeight
                layerTableRecordXml.SetAttributeValue("ViewportVisibilityDefault", layerTableRecord.ViewportVisibilityDefault); //bool

                return layerTableRecordXml;
            }
        }
        // get layers from xml
        private static LayerTableRecord GetLayerFromXml(XElement layerXElement)
        {
            var layerTblR = new LayerTableRecord();
            var nameAttr = layerXElement?.Attribute("Name");
            // string
            if (nameAttr != null)
            {
                layerTblR.Name = nameAttr.Value;
                layerTblR.Description = layerXElement.Attribute("Description")?.Value;
                // bool
                layerTblR.IsFrozen = bool.TryParse(layerXElement.Attribute("IsFrozen")?.Value, out bool b) && b;
                layerTblR.IsHidden = bool.TryParse(layerXElement.Attribute("IsHidden")?.Value, out b) && b;
                layerTblR.IsLocked = bool.TryParse(layerXElement.Attribute("IsLocked")?.Value, out b) && b;
                layerTblR.IsOff = bool.TryParse(layerXElement.Attribute("IsOff")?.Value, out b) && b;
                layerTblR.IsPlottable = bool.TryParse(layerXElement.Attribute("IsPlottable")?.Value, out b) && b;
                layerTblR.ViewportVisibilityDefault =
                    bool.TryParse(layerXElement.Attribute("ViewportVisibilityDefault")?.Value, out b) && b;
                // color
                layerTblR.Color = Color.FromColorIndex(ColorMethod.ByAci,
                    short.TryParse(layerXElement.Attribute("Color")?.Value, out short sh) ? sh : short.Parse("7"));
                // linetype
                layerTblR.LinetypeObjectId = GetLineTypeObjectId(layerXElement.Attribute("Linetype")?.Value);
                //LineWeight
                layerTblR.LineWeight =
                    Enum.TryParse(layerXElement.Attribute("LineWeight")?.Value, out LineWeight lw)
                        ? lw
                        : LineWeight.ByLineWeightDefault;

                return layerTblR;
            }
            return null;
        }
        private static string GetLineTypeName(ObjectId ltid)
        {

            var lt = "Continuous";
            if (ltid == ObjectId.Null) return lt;

            using (AcadHelpers.Document.LockDocument())
            {
                using (var tr = AcadHelpers.Database.TransactionManager.StartTransaction())
                {
                    var linetype = tr.GetObject(ltid, OpenMode.ForRead) as LinetypeTableRecord;
                    lt = linetype?.Name;
                    tr.Commit();
                }
            }
            return lt;
        }
        private static ObjectId GetLineTypeObjectId(string ltname)
        {
            var ltid = ObjectId.Null;
            if (string.IsNullOrEmpty(ltname)) return ObjectId.Null;

            using (AcadHelpers.Document.LockDocument())
            {
                using (var tr = AcadHelpers.Database.TransactionManager.StartTransaction())
                {
                    var lttbl = tr.GetObject(AcadHelpers.Database.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
                    if (lttbl != null)
                        if (lttbl.Has(ltname))
                        {
                            ltid = lttbl[ltname];
                        }
                        else
                        {
                            try
                            {
                                var path = HostApplicationServices.Current.FindFile(
                                    "acad.lin", AcadHelpers.Database, FindFileHint.Default);
                                AcadHelpers.Database.LoadLineTypeFile(ltname, path);
                                ltid = lttbl[ltname];
                            }
                            catch
                            {
                                MessageBox.Show("Не удалось загрузить тип линий: " + ltname, MessageBoxIcon.Close);
                            }
                        }
                    tr.Commit();
                }
            }
            return ltid;
        }
    }
}
