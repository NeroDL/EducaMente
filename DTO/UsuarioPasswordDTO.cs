using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class UsuarioPasswordDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string PasswordAntigua { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [MinLength(8, ErrorMessage = "El campo {0} requiere un tamaño de minimo {1} de caracteres")]
        public string PasswordNueva { get; set; }
        [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
        [Compare("PasswordNueva", ErrorMessage = "La confirmación de la contraseña no coincide con la nueva contraseña.")]
        public string PasswordConfirmacion { get; set; }
    }
}
