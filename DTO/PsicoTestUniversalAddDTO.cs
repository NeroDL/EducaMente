using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class PsicoTestUniversalAddDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string? Descripcion { get; set; }
    }
}
