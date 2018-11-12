namespace mpESKD.Base.Styles
{
    using System;
    using System.IO;
    using System.Xml.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Functions.mpAxis.Styles;
    using Functions.mpBreakLine.Styles;
    using Functions.mpGroundLine.Styles;

    public static class StyleManager
    {
        /// <summary>Проверка и создание в случае необходимости файла стилей</summary>
        /// todo look here!!!
        public static void CheckStylesFile<T>()where T:new()
        {
            var ttt = new T();
            bool needToCreate;
            var stylesFile = Path.Combine(MainFunction.StylesPath, GetStylesFileName(typeof(T)));
            if (File.Exists(stylesFile))
            {
                try
                {
                    XElement.Load(stylesFile);
                    needToCreate = false;
                }
                catch
                {
                    needToCreate = true;
                }
            }
            else needToCreate = true;

            if (needToCreate)
            {
                XElement fXel = new XElement("Styles");
                fXel.Save(stylesFile);
            }
        }

        private static string GetStylesFileName(Type style)
        {
            if (style == typeof(AxisStyle))
                return "AxisStyles.xml";
            if (style == typeof(GroundLineStyle))
                return "GroundLineStyles.xml";
            if(style == typeof(BreakLineStyle))
                return "BreakLineStyles.xml";

            return string.Empty;
        }
    }
}
