namespace EducaMente.DTO
{
    public class UsuarioTestConRespuestasDTO
    {
        public string UsuarioTestId { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string TestId { get; set; } = string.Empty;
        public string DescripcionTest { get; set; } = string.Empty;
        public string Alcance { get; set; } = string.Empty;
        public string TargetUsuarioId { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string FechaInicio { get; set; } = string.Empty;
        public string FechaFin { get; set; } = string.Empty;
        public List<UsuarioTestItemConRespuestasDTO> Items { get; set; } = new();
    }
}
