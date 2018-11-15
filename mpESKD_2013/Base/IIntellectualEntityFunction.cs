namespace mpESKD.Base
{
    /// <summary>
    /// Интерфейс функции примитива
    /// </summary>
    public interface IIntellectualEntityFunction
    {
        /// <summary>
        /// Метод, вызываемый при загрузке AutoCAD
        /// </summary>
        void Initialize();

        /// <summary>
        /// Метод, вызываемый при закрытии AutoCAD
        /// </summary>
        void Terminate();
    }
}
