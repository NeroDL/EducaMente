using EducaMente.Domain;
using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class WebServiceAddDTO
    {
        public int? EntidadId { get; set; }

        [Required(ErrorMessage = "La API Key es obligatoria")]
        public required string ApiKey { get; set; }

        public int? TipoEnvio { get; set; }

        [Required(ErrorMessage = "El tipo de servicio web es obligatorio")]
        public TipoWebService Tipo { get; set; }

        public string? Servicio { get; set; }

        public string? Descripcion { get; set; }
    }
}
