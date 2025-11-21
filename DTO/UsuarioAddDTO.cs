using EducaMente.DataAnotations;
using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class UsuarioAddDTO
    {
        [EmailValidation]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Correo { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [MaxLength(15, ErrorMessage = "El campo {0} requiere un tamaño de maximo {1} de caracteres")]
        public string Documento { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string TipoUsuId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [MinLength(8, ErrorMessage = "El campo {0} requiere un tamaño de minimo {1} de caracteres")]
        public string Password { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string VerifyPassword { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [CelularValidation]
        public string Celular { get; set; }
    }
}
