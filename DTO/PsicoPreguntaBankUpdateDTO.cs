using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class PsicoPreguntaBankUpdateDTO
    {
        [Required(ErrorMessage = "El Id de la pregunta es obligatorio.")]
        public string Id { get; set; } = string.Empty;
        [MaxLength(600)]
        public string? Texto { get; set; }
        [MaxLength(30)]
        public string? Dimension { get; set; }
        [MaxLength(50)]
        public string? ScaleId { get; set; }
        [MaxLength(20)]
        public string? Estado { get; set; }
        [MaxLength(20)]
        public string? Fuente { get; set; }
    }
}
