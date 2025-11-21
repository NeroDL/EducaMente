using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class PsicoTestPreguntaAddDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string TestId { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string PreguntaId { get; set; }

    }
}
