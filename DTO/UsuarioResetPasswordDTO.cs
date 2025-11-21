using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class UsuarioResetPasswordDTO
    {
        [Required(ErrorMessage = "El token de recuperación es obligatorio.")]
        public Guid Token { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La nueva contraseña debe tener al menos {1} caracteres.")]
        public string NuevaPassword { get; set; }

        [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
        [Compare("NuevaPassword", ErrorMessage = "La confirmación no coincide con la nueva contraseña.")]
        public string ConfirmacionPassword { get; set; }
    }
}
