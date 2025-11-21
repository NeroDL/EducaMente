using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class PsicoTestCambiarEstadoDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string TestId { get; set; } = string.Empty;
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string NuevoEstado { get; set; } = string.Empty;
    }
}
