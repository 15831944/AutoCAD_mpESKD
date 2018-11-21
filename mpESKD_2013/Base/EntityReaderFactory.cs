namespace mpESKD.Base
{
    using System;
    using System.Linq;
    using AcDd = Autodesk.AutoCAD.DatabaseServices;
    using ModPlusAPI.Annotations;

    public class EntityReaderFactory
    {
        private static EntityReaderFactory _entityReaderFactory;

        public static EntityReaderFactory Instance => _entityReaderFactory ?? (_entityReaderFactory = new EntityReaderFactory());


        [CanBeNull]
        public IntellectualEntity GetFromEntity(AcDd.Entity entity)
        {
            var applicableCommands = TypeFactory.Instance.GetEntityCommandNames();
            if (entity.XData == null) 
                return null;
            var typedValue = entity.XData.AsArray()
                .FirstOrDefault(tv => tv.TypeCode == (int)AcDd.DxfCode.ExtendedDataRegAppName && applicableCommands.Contains(tv.Value.ToString()));
            if (typedValue.Value != null)
            {
                var appName = typedValue.Value.ToString();
                Type entityType = TypeFactory.Instance.GetEntityTypes().FirstOrDefault(t => t.Name == appName.Substring(2));
                if (entityType != null)
                    return GetEntity(entity, entityType, appName);
            }

            return null;
        }

        [CanBeNull]
        public T GetFromEntity<T>(AcDd.Entity entity) where T : IntellectualEntity
        {
            return (T)GetFromEntity(entity);
        }

        private IntellectualEntity GetEntity(AcDd.Entity ent, Type type, string appName)
        {
            using (AcDd.ResultBuffer resBuf = ent.GetXDataForApplication(appName))
            {
                // В случае команды ОТМЕНА может вернуть null
                if (resBuf == null) 
                    return null;
                var entity = (IntellectualEntity)Activator.CreateInstance(type);
                entity.BlockId = ent.ObjectId;
                // Получаем параметры из самого блока
                // ОБЯЗАТЕЛЬНО СНАЧАЛА ИЗ БЛОКА!!!!!!
                entity.GetParametersFromEntity(ent);
                // Получаем параметры из XData
                entity.SetPropertiesValuesFromXData(resBuf);

                return entity;
            }
        }
    }
}