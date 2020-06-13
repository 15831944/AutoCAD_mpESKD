namespace mpESKD.Base
{
    using System.Collections.Generic;

    /// <summary>
    /// Различные постоянные значения
    /// </summary>
    public static class Invariables
    {
        /// <summary>
        /// Идентификатор функции для поиска локализованных значений в методе <see cref="ModPlusAPI.Language.GetItem"/>
        /// </summary>
        public static string LangItem = "mpESKD";

        /// <summary>
        /// Допустимые буквенные значения для координационных осей согласно п.5.4, 5.5 ГОСТ 21.101-97
        /// <summary>Данные значения также актуальны для обозначений видов, разрезов и т.п.</summary>
        /// </summary>
        public static List<string> AxisRusAlphabet = new List<string>
        {
            "А", "Б", "В", "Г", "Д", "Е", "Ж", "И", "К", "Л", "М", "Н", "П", "Р", "С", "Т", "У", "Ф", "Ш", "Э", "Ю", "Я",
            "АА", "ББ", "ВВ", "ГГ", "ДД", "ЕЕ", "ЖЖ", "ИИ", "КК", "ЛЛ", "ММ", "НН", "ПП", "РР", "СС", "ТТ", "УУ", "ФФ", "ШШ", "ЭЭ", "ЮЮ", "ЯЯ"
        };
    }
}
