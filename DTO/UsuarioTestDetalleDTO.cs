namespace EducaMente.DTO
{
    public class UsuarioTestDetalleDTO
    {
        public string UsuarioTestId { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string TestId { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string FechaInicio { get; set; } = string.Empty;
        public string? FechaFin { get; set; } = string.Empty;
        public string? DescripcionTest { get; set; }
        public string Alcance { get; set; } = string.Empty;
        public string? TargetUsuarioId { get; set; }
        public int ConteoPreguntas { get; set; }
        public List<UsuarioTestItemDTO> Items { get; set; } = new();
    }
}
