namespace mpESKD.Functions.mpGroundLine
{
    using System.Diagnostics.CodeAnalysis;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Runtime;
    using Base;
    using Base.Helpers;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using Styles;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GroundLineFunction : IMPCOEntityFunction
    {
        /// <summary>Имя примитива, помещаемое в XData</summary>
        public static readonly string MPCOEntName = GroundLineInterface.Name; // mpGroundLine

        /// <summary>Отображаемое имя примитива</summary>
        public static string MPCOEntDisplayName = GroundLineInterface.LName; // Линия грунта

        public void Initialize()
        {
            //// TODO Release it!!!
        }

        public void Terminate()
        {
            //// TODO Release it!!!
        }
    }

    public class GroundLineCommands
    {
        [CommandMethod("ModPlus", "mpGroundLine", CommandFlags.Modal)]
        public void CreateGroundLineCommand()
        {
            CreateGroundLine();
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
                var style = GroundLineStyleManager.
                //// TODO Release it!!!
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
