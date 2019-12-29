namespace mpESKD.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Functions.mpAxis;
    using Functions.mpBreakLine;
    using Functions.mpGroundLine;
    using Functions.mpSection;

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
                typeof(GroundLine),
                typeof(Section)
            };
        }

        /// <summary>
        /// Возвращает дескриптор для примитива по типу примитива
        /// </summary>
        /// <param name="entityType">Тип примитива</param>
        /// <returns></returns>
        public IIntellectualEntityDescriptor GetDescriptor(Type entityType)
        {
            if (entityType == typeof(BreakLine))
            {
                return BreakLineDescriptor.Instance;
            }

            if (entityType == typeof(Axis))
            {
                return AxisDescriptor.Instance;
            }

            if (entityType == typeof(GroundLine))
            {
                return GroundLineDescriptor.Instance;
            }

            if (entityType == typeof(Section))
            {
                return SectionDescriptor.Instance;
            }

            return null;
        }

        /// <summary>
        /// Возвращает список экземпляров функций, реализующих интерфейс <see cref="IIntellectualEntityFunction"/>
        /// </summary>
        public List<IIntellectualEntityFunction> GetEntityFunctionTypes()
        {
            return new List<IIntellectualEntityFunction>
            {
                new BreakLineFunction(),
                new AxisFunction(),
                new GroundLineFunction(),
                new SectionFunction()
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
                case nameof(Section):
                    return TryGetLocalizationValue("h96");
            }

            return string.Empty;
        }

        private string TryGetLocalizationValue(string key)
        {
            try
            {
                return ModPlusAPI.Language.GetItem(Invariables.LangItem, key);
            }
            catch
            {
                // ignore
            }

            return string.Empty;
        }
    }
}
