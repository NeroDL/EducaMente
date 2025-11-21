namespace EducaMente.DTO
{
    public class GeminiAnalisisPerfilResponseDTO
    {
        // Indica si la llamada a Gemini fue exitosa.
        public bool Exito { get; set; }
        // Observación redactada por Gemini siguiendo el PROMT.
        public string Observacion { get; set; } = string.Empty;
        // Mensaje informativo o de error.
        public string Mensaje { get; set; } = string.Empty;
        // (Opcional) JSON crudo devuelto por Gemini para auditoría/debug.
        public string JsonCompleto { get; set; } = "{}";
        // (Opcional) Nombre del modelo usado, por ejemplo: gemini-2.5-pro.
        public string? ModeloUsado { get; set; }
    }
}
