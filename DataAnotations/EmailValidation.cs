using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace EducaMente.DataAnotations
{
    public class EmailValidation : ValidationAttribute
    {
        private static readonly string[] DominiosNoPermitidos = { "mailinator.com", "tempmail.com", "example.com" };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var email = value as string;

            if (string.IsNullOrWhiteSpace(email))
                return ValidationResult.Success; // Usa [Required] si quieres forzar

            try
            {
                var mail = new MailAddress(email);
                var host = mail.Host;

                // Validar que el dominio contenga al menos un punto
                if (!host.Contains('.'))
                    return new ValidationResult("El dominio del correo debe contener al menos un punto ('.').");

                // Validar que no contenga patrones inválidos como .com.com o múltiples puntos seguidos
                if (host.Contains("..") || host.Contains(".com.com", StringComparison.OrdinalIgnoreCase))
                    return new ValidationResult("El dominio del correo es inválido.");

                //Validar dominios bloqueados
                //if (DominiosNoPermitidos.Any(d => host.EndsWith(d, StringComparison.OrdinalIgnoreCase)))
                //    return new ValidationResult("El dominio del correo no está permitido.");

                return ValidationResult.Success;
            }
            catch
            {
                return new ValidationResult("El formato del correo electrónico es inválido.");
            }
        }
    }
}
