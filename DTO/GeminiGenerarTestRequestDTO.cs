namespace EducaMente.DTO
{
    public class GeminiGenerarTestRequestDTO
    {
        // Prompt base (plantilla del JSON que tú agregas manualmente)
        public string PromptBase { get; set; } = string.Empty;

        // JSON ya serializado del perfil psicológico
        public string PerfilJSON { get; set; } = string.Empty;

        // JSON del detalle del test con respuestas
        public string TestJSON { get; set; } = string.Empty;

        // JSON del banco de preguntas disponible
        public string PreguntasBancoJSON { get; set; } = string.Empty;
    }
}
