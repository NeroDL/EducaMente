using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class PsicoPreguntaBankAddDTO
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
        [Required(ErrorMessage = "La fuente es obligatoria.")]
        [MaxLength(20)]
        public string Fuente { get; set; } = string.Empty;
    }
}
