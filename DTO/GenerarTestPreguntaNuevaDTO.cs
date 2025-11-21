using System.Text.Json.Serialization;

namespace EducaMente.DTO
{
    public class GenerarTestPreguntaNuevaDTO
    {
        // Texto de la nueva pregunta
        [JsonPropertyName("texto")]
        public string Texto { get; set; } = string.Empty;

        // Dimensión (Ansiedad, Estres, Motivacion, Autoestima, Proposito)
        [JsonPropertyName("dimension")]
        public string Dimension { get; set; } = string.Empty;

        // Id de la escala Likert (L1..L8)
        [JsonPropertyName("scaleId")]
        public string ScaleId { get; set; } = string.Empty;
    }
}
