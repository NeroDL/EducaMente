using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class TipoUsuarioDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MaxLength(100, ErrorMessage = "El campo {0} requiere un tamaño de maximo {1} de caracteres")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MaxLength(50, ErrorMessage = "El campo {0} tiene un tamaño de maximo {1} de caracteres")]
        public string Rol { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public short Estado { get; set; }
    }
}
