namespace mpESKD.Functions.mpSection
{
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Helpers;
    using ModPlusAPI;
    using ModPlusAPI.Windows;

    public class SectionFunction : IIntellectualEntityFunction
    {
        public void Initialize()
        {
            //todo add overrules
            Overrule.Overruling = true;
        }

        public void Terminate()
        {
            //todo add overrules
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
            Statistic.SendCommandStarting(SectionInterface.Name, MpVersionData.CurCadVers);

            try
            {
                Overrule.Overruling = false;

                /* Регистрация ЕСКД приложения должна запускаться при запуске
                 * функции, т.к. регистрация происходит в текущем документе
                 * При инициализации плагина регистрации нет!
                 */
                ExtendedDataHelpers.AddRegAppTableRecord(SectionInterface.Name);


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
    }
}
