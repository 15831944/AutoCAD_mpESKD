namespace mpESKD
{
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using Base.Helpers;

    /// <summary>
    /// Слежение за командой "редактор блоков" автокада
    /// </summary>
    public class BeditCommandWatcher
    {
        /// <summary>True - использовать редактор блоков. False - не использовать</summary>
        public static bool UseBedit = true;

        public static void Initialize()
        {
            Application.DocumentManager.DocumentLockModeChanged += DocumentManager_DocumentLockModeChanged;
        }

        private static void DocumentManager_DocumentLockModeChanged(
            object sender, Autodesk.AutoCAD.ApplicationServices.DocumentLockModeChangedEventArgs e)
        {
            try
            {
                if (!UseBedit)
                {
                    if (e.GlobalCommandName == "BEDIT")
                    {
                        e.Veto();
                    }
                }
            }
            catch (System.Exception exception)
            {
                AcadHelpers.WriteMessageInDebug($"\nException {exception.Message}");
            }
        }
    }
}