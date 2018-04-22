using System;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using mpESKD.Base.Helpers;
using mpESKD.Functions.mpBreakLine.Styles;

// ReSharper disable InconsistentNaming
#pragma warning disable CS0618

namespace mpESKD.Functions.mpBreakLine.Properties
{
    public class BreakLinePropertiesData
    {
        private ObjectId _blkRefObjectId;

        private string _style;

        public string Style
        {
            get => _style;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var breakLine = BreakLineXDataHelper.GetBreakLineFromEntity(blkRef))
                        {
                            var style = BreakLineStylesManager.Styles.FirstOrDefault(s => s.Name.Equals(value));
                            if (style != null)
                            {
                                breakLine.StyleGuid = style.Guid;
                                breakLine.ApplyStyle(style);
                                breakLine.UpdateEntities();
                                breakLine.GetBlockTableRecordWithoutTransaction(blkRef);
                                using (var resBuf = breakLine.GetParametersForXData())
                                {
                                    if (blkRef != null) blkRef.XData = resBuf;
                                }
                            }
                        }
                        if (blkRef != null) blkRef.ResetBlock();
                    }
                }
                Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
            }
        }

        private int _overgang;
        public int Overhang
        {
            get => _overgang;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var breakLine = BreakLineXDataHelper.GetBreakLineFromEntity(blkRef))
                        {
                            breakLine.Overhang = value;
                            breakLine.UpdateEntities();
                            breakLine.GetBlockTableRecordWithoutTransaction(blkRef);
                            using (var resBuf = breakLine.GetParametersForXData())
                            {
                                if (blkRef != null) blkRef.XData = resBuf;
                            }
                        }
                        if (blkRef != null) blkRef.ResetBlock();
                    }
                }
                Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
            }
        }

        private int _breakHeight;
        public int BreakHeight
        {
            get => _breakHeight;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var breakLine = BreakLineXDataHelper.GetBreakLineFromEntity(blkRef))
                        {
                            breakLine.BreakHeight = value;
                            breakLine.UpdateEntities();
                            breakLine.GetBlockTableRecordWithoutTransaction(blkRef);
                            using (var resBuf = breakLine.GetParametersForXData())
                            {
                                if (blkRef != null) blkRef.XData = resBuf;
                            }
                        }
                        if (blkRef != null) blkRef.ResetBlock();
                    }
                }
                Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
            }
        }

        private int _breakWidth;
        public int BreakWidth
        {
            get => _breakWidth;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var breakLine = BreakLineXDataHelper.GetBreakLineFromEntity(blkRef))
                        {
                            breakLine.BreakWidth = value;
                            breakLine.UpdateEntities();
                            breakLine.GetBlockTableRecordWithoutTransaction(blkRef);
                            using (var resBuf = breakLine.GetParametersForXData())
                            {
                                if (blkRef != null) blkRef.XData = resBuf;
                            }
                        }
                        if (blkRef != null) blkRef.ResetBlock();
                    }
                }
                Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
            }
        }

        private string _breakLineType;
        public string BreakLineType
        {
            get => _breakLineType;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var breakLine = BreakLineXDataHelper.GetBreakLineFromEntity(blkRef))
                        {
                            breakLine.BreakLineType = BreakLinePropertiesHelpers.GetBreakLineTypeByLocalName(value);
                            breakLine.UpdateEntities();
                            breakLine.GetBlockTableRecordWithoutTransaction(blkRef);
                            using (var resBuf = breakLine.GetParametersForXData())
                            {
                                if (blkRef != null) blkRef.XData = resBuf;
                            }
                        }
                        if (blkRef != null) blkRef.ResetBlock();
                    }
                }
                Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
            }
        }

        #region General
        
        private string _scale;
        public string Scale
        {
            get => _scale;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var breakLine = BreakLineXDataHelper.GetBreakLineFromEntity(blkRef))
                        {
                            breakLine.Scale = AcadHelpers.GetAnnotationScaleByName(value);
                            breakLine.UpdateEntities();
                            breakLine.GetBlockTableRecordWithoutTransaction(blkRef);
                            using (var resBuf = breakLine.GetParametersForXData())
                            {
                                if (blkRef != null) blkRef.XData = resBuf;
                            }
                        }
                        if (blkRef != null) blkRef.ResetBlock();
                    }
                }
                Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
            }
        }

        private double _lineTypeScale;
        /// <summary>Масштаб типа линии</summary>
        public double LineTypeScale
        {
            get => _lineTypeScale;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        using (var breakLine = BreakLineXDataHelper.GetBreakLineFromEntity(blkRef))
                        {
                            breakLine.LineTypeScale = value;
                            breakLine.UpdateEntities();
                            breakLine.GetBlockTableRecordWithoutTransaction(blkRef);
                            using (var resBuf = breakLine.GetParametersForXData())
                            {
                                if (blkRef != null) blkRef.XData = resBuf;
                            }
                        }
                        if (blkRef != null) blkRef.ResetBlock();
                    }
                }
                Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
            }
        }

        private string _layerName;
        /// <summary>Слой</summary>
        public string LayerName
        {
            get => _layerName;
            set
            {
                using (AcadHelpers.Document.LockDocument())
                {
                    using (var blkRef = _blkRefObjectId.Open(OpenMode.ForWrite) as BlockReference)
                    {
                        if (blkRef != null) blkRef.Layer = value;
                    }
                }
                Autodesk.AutoCAD.Internal.Utils.FlushGraphics();
            }
        }

        #endregion

        public bool IsValid { get; set; }

        public BreakLinePropertiesData(ObjectId blkRefObjectId)
        {
            if (Verify(blkRefObjectId))
            {
                IsValid = true;
                _blkRefObjectId = blkRefObjectId;
                using (BlockReference blkRef = blkRefObjectId.Open(OpenMode.ForRead, false, true) as BlockReference)
                {
                    if (blkRef != null)
                    {
                        blkRef.Modified += BlkRef_Modified;
                        Update(blkRef);
                    }
                }
            }
            else IsValid = false;
        }

        private void BlkRef_Modified(object sender, EventArgs e)
        {
            BlockReference blkRef = sender as BlockReference;
            if (blkRef != null)
                Update(blkRef);
        }

        private void Update(BlockReference blkReference)
        {
            if (blkReference == null)
            {
                _blkRefObjectId = ObjectId.Null;
                return;
            }
            var breakLine = BreakLineXDataHelper.GetBreakLineFromEntity(blkReference);
            if (breakLine != null)
            {
                _style = BreakLineStylesManager.Styles.FirstOrDefault(s => s.Guid.Equals(breakLine.StyleGuid))?.Name;
                _overgang = breakLine.Overhang;
                _breakHeight = breakLine.BreakHeight;
                _breakWidth = breakLine.BreakWidth;
                _breakLineType = BreakLinePropertiesHelpers.GetLocalBreakLineTypeName(breakLine.BreakLineType);
                _scale = breakLine.Scale.Name;
                _layerName = blkReference.Layer;
                _lineTypeScale = breakLine.LineTypeScale;
                AnyPropertyChangedReise();
            }
        }

        private static bool Verify(ObjectId breakLineObjectId)
        {
            return !breakLineObjectId.IsNull &&
                   breakLineObjectId.IsValid &
                   !breakLineObjectId.IsErased &
                   !breakLineObjectId.IsEffectivelyErased;
        }

        public event EventHandler AnyPropertyChanged;
        /// <summary>Вызов события изменения какого-либо свойства</summary>
        protected void AnyPropertyChangedReise()
        {
            AnyPropertyChanged?.Invoke(this, null);
        }
    }
}
