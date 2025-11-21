namespace EducaMente.DTO
{
    public class GeminiGenerarTestServiceResponseDTO
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string? ModeloUsado { get; set; }
        public string? RawResponse { get; set; }
        public GenerarTestResponseDTO? Contenido { get; set; }
    }
}
