using System.ComponentModel.DataAnnotations;

namespace EducaMente.Models
{
    public class PromtModel
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Codigo { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Descripcion { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Promt { get; set; }
    }
}
