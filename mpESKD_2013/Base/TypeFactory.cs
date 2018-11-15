namespace mpESKD.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Functions.mpAxis;
    using Functions.mpBreakLine;
    using Functions.mpGroundLine;

    public class TypeFactory
    {
        private static TypeFactory _instance;

        public static TypeFactory Instance => _instance ?? (_instance = new TypeFactory());

        /// <summary>
        /// Возвращает список типов примитивов
        /// </summary>
        public List<Type> GetEntityTypes()
        {
            return new List<Type>
            {
                typeof(BreakLine),
                typeof(Axis),
                typeof(GroundLine)
            };
        }

        /// <summary>
        /// Возвращает список экземпляров функций, реализующих интерфейс <see cref="IIntellectualEntityFunction"/>
        /// </summary>
        public List<IIntellectualEntityFunction> GetEntityFunctionTypes()
        {
            return new List<IIntellectualEntityFunction>
            {
                //todo uncomment
                //new BreakLineFunction(),
                //new AxisFunction(),
                new GroundLineFunction()
            };
        }

        /// <summary>
        /// Возвращает список имен команд. Имя команды - это имя типа примитива с приставкой "mp"
        /// <remarks>Используется в расширенных данных (XData) блоков</remarks>
        /// </summary>
        public List<string> GetEntityCommandNames()
        {
            return GetEntityTypes().Select(t => $"mp{t.Name}").ToList();
        }

        public string GetSystemStyleLocalizedDescription(Type entityType)
        {
            switch (entityType.Name)
            {
                case nameof(BreakLine):
                    return TryGetLocalizationValue("h53");
                case nameof(Axis):
                    return TryGetLocalizationValue("h68");
                case nameof(GroundLine):
                    return TryGetLocalizationValue("h78");
            }

            return string.Empty;
        }

        private string TryGetLocalizationValue(string key)
        {
            try
            {
                return ModPlusAPI.Language.GetItem(MainFunction.LangItem, key);
            }
            catch
            {
                // ignore
            }

            return string.Empty;
        }
    }
}
