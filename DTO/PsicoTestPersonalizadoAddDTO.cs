using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class PsicoTestPersonalizadoAddDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string TargetUsuarioId { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string? Descripcion { get; set; }
    }
}
