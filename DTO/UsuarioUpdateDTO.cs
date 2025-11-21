using EducaMente.DataAnotations;
using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class UsuarioUpdateDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [MaxLength(15, ErrorMessage = "El campo {0} requiere un tamaño de maximo {1} de caracteres")]
        public string Documento { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string TipoUsuId { get; set; }

        [EmailValidation]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Correo { get; set; }

        [CelularValidation]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Celular { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Estado { get; set; } = "Activo";
    }
}
