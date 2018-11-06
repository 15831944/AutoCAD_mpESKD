using System.Globalization;
using System.Windows.Controls;
using ModPlusAPI;

namespace mpESKD.Base.Properties.Converters
{
    public class DoubleValidationRule : ValidationRule
    {
        public override ValidationResult Validate
            (object value, CultureInfo cultureInfo)
        {
            double res;
            if (string.IsNullOrEmpty(value.ToString()))
                return new ValidationResult(false, Language.GetItem(MainFunction.LangItem, "err3")); // Значение не может быть пустым!
            else if (!double.TryParse((string)value, out res))
            {
                return new ValidationResult(false, Language.GetItem(MainFunction.LangItem, "err4")); // Недопустимое значение! Введите число!
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
    public class IntValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int res;
            if (string.IsNullOrEmpty(value.ToString()))
                return new ValidationResult(false, Language.GetItem(MainFunction.LangItem, "err3")); // Значение не может быть пустым!
            else if (!int.TryParse((string)value, out res))
            {
                return new ValidationResult(false, Language.GetItem(MainFunction.LangItem, "err4")); // Недопустимое значение! Введите число!
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
