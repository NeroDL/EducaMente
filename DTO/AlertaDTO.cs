namespace EducaMente.DTO
{
    public class AlertaDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string UsuarioTestId { get; set; } = string.Empty;
        public string FechaCreacion { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string MensajeAlerta { get; set; } = string.Empty;
    }
}
