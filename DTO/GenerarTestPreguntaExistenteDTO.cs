using System.Text.Json.Serialization;

namespace EducaMente.DTO
{
    public class GenerarTestPreguntaExistenteDTO
    {
        [JsonPropertyName("preguntaId")]
        public string PreguntaId { get; set; } = string.Empty;
    }
}
