namespace mpESKD.Base
{
    using System.Linq;
    using AcDd = Autodesk.AutoCAD.DatabaseServices;
    using Functions.mpAxis;
    using Functions.mpBreakLine;
    using Functions.mpGroundLine;
    using Functions.mpSection;
    using ModPlusAPI.Annotations;

    public class EntityReaderFactory
    {
        private static EntityReaderFactory _entityReaderFactory;
        
        public static EntityReaderFactory Instance => _entityReaderFactory ?? (_entityReaderFactory = new EntityReaderFactory());

        [CanBeNull]
        public IntellectualEntity GetFromEntity(AcDd.Entity entity)
        {
            var applicableCommands = TypeFactory.Instance.GetEntityCommandNames();
            var appName = entity.XData.AsArray()
                .FirstOrDefault(tv => tv.TypeCode == (int)AcDd.DxfCode.ExtendedDataRegAppName && applicableCommands.Contains(tv.Value.ToString()))
                .Value.ToString();

            return GetFromEntity(entity, appName);
        }

        [CanBeNull]
        public IntellectualEntity GetFromEntity(AcDd.Entity entity, string appName)
        {
            switch (appName)
            {
                case "mpBreakLine":
                    return GetBreakLineFromEntity(entity);
                case "mpAxis":
                    return GetAxisFromEntity(entity);
                case "mpGroundLine":
                    return GetGroundLineFromEntity(entity);
                case "mpSection":
                    return GetSectionFromEntity(entity);
            }

            return null;
        }

        [CanBeNull]
        public T GetFromEntity<T>(AcDd.Entity entity) where T : IntellectualEntity
        {
            return GetFromEntity(entity, "mp" + typeof(T).Name) as T;
        }

        private BreakLine GetBreakLineFromEntity(AcDd.Entity ent)
        {
            using (AcDd.ResultBuffer resBuf = ent.GetXDataForApplication(BreakLineInterface.Name))
            {
                // В случае команды ОТМЕНА может вернуть null
                if (resBuf == null) return null;
                BreakLine breakLine = new BreakLine(ent.ObjectId);
                // Получаем параметры из самого блока
                // ОБЯЗАТЕЛЬНО СНАЧАЛА ИЗ БЛОКА!!!!!!
                breakLine.GetParametersFromEntity(ent);
                // Получаем параметры из XData
                breakLine.GetParametersFromResBuf(resBuf);

                return breakLine;
            }
        }

        private Axis GetAxisFromEntity(AcDd.Entity ent)
        {
            using (AcDd.ResultBuffer resBuf = ent.GetXDataForApplication(AxisInterface.Name))
            {
                // В случае команды ОТМЕНА может вернуть null
                if (resBuf == null) return null;
                Axis axis = new Axis(ent.ObjectId);
                // Получаем параметры из самого блока
                // ОБЯЗАТЕЛЬНО СНАЧАЛА ИЗ БЛОКА!!!!!!
                axis.GetParametersFromEntity(ent);
                // Получаем параметры из XData
                axis.GetParametersFromResBuf(resBuf);

                return axis;
            }
        }

        private GroundLine GetGroundLineFromEntity(AcDd.Entity ent)
        {
            using (AcDd.ResultBuffer resBuf = ent.GetXDataForApplication(GroundLineInterface.Name))
            {
                // В случае команды ОТМЕНА может вернуть null
                if (resBuf == null)
                    return null;
                GroundLine groundLine = new GroundLine(ent.ObjectId);
                // Получаем параметры из самого блока
                // ОБЯЗАТЕЛЬНО СНАЧАЛА ИЗ БЛОКА!!!!!!
                groundLine.GetParametersFromEntity(ent);
                // Получаем параметры из XData
                groundLine.GetParametersFromResBuf(resBuf);

                return groundLine;
            }
        }

        private Section GetSectionFromEntity(AcDd.Entity ent)
        {
            using (AcDd.ResultBuffer resBuf = ent.GetXDataForApplication(GroundLineInterface.Name))
            {
                // В случае команды ОТМЕНА может вернуть null
                if (resBuf == null)
                    return null;
                Section section = new Section(ent.ObjectId);
                // Получаем параметры из самого блока
                // ОБЯЗАТЕЛЬНО СНАЧАЛА ИЗ БЛОКА!!!!!!
                section.GetParametersFromEntity(ent);
                // Получаем параметры из XData
                section.GetParametersFromResBuf(resBuf);

                return section;
            }
        }
    }
}
