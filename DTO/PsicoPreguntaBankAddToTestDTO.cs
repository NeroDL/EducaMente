using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class PsicoPreguntaBankAddToTestDTO
    {
        [Required(ErrorMessage = "El texto de la pregunta es obligatorio.")]
        [MaxLength(600)]
        public string Texto { get; set; } = string.Empty;
        [Required(ErrorMessage = "La dimensión es obligatoria.")]
        [MaxLength(30)]
        public string Dimension { get; set; } = string.Empty;
        [Required(ErrorMessage = "La escala (ScaleId) es obligatoria.")]
        [MaxLength(50)]
        public string ScaleId { get; set; } = string.Empty;
        [Required(ErrorMessage = "El TestId es obligatorio")]
        public string TestId { get; set; }
    }
}
