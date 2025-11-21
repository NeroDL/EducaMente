using EducaMente.Domain;
using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class WebServiceUpdateDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int EntidadId { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string ApiKey { get; set; } = string.Empty;
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int TipoEnvio { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public TipoWebService Tipo { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Servicio { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Descripcion { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string ModeloPorDefecto { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string EndpointBase { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int MaxTokens { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public decimal Temperature { get; set; }
    }
}
