using System.ComponentModel.DataAnnotations;

namespace EducaMente.DataAnotations
{
    public class CelularValidation : ValidationAttribute
    {
        private static readonly HashSet<string> PrefijosValidos = new()
        {
        "300", "301", "302", "303", "304", "305", "310", "311", "312", "313", "314",
        "315", "316", "317", "318", "319", "320", "321", "322", "323", "324",
        "350", "351", "333"
        };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var celular = value as string;

            if (string.IsNullOrWhiteSpace(celular))
                return ValidationResult.Success; // No se valida si es null o vacío

            if (celular.Length != 10)
                return new ValidationResult("El número de celular debe tener exactamente 10 dígitos.");

            if (!celular.All(char.IsDigit))
                return new ValidationResult("El número de celular solo debe contener números.");

            var prefijo = celular.Substring(0, 3);
            if (!PrefijosValidos.Contains(prefijo))
                return new ValidationResult($"El prefijo {prefijo} no es válido en Colombia.");

            return ValidationResult.Success;
        }
    }
}
