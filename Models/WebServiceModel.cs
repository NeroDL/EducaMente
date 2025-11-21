using EducaMente.Domain;

namespace EducaMente.Models
{
    public class WebServiceModel
    {
        public int Id { get; set; }
        public int? EntidadId { get; set; }
        public required string ApiKey { get; set; }
        public int? TipoEnvio { get; set; }
        public TipoWebService? Tipo { get; set; }
        public string? Servicio { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }

        // Nuevas propiedades para sistema Multi-IA
        public string? ModeloPorDefecto { get; set; }
        public string? EndpointBase { get; set; }
        public int? MaxTokens { get; set; }
        public decimal? Temperature { get; set; }
    }
}
