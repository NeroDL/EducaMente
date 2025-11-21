namespace EducaMente.DTO
{
    public class PerfilPsicoHistDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string TestId { get; set; } = string.Empty;
        public string UsuarioTestId { get; set; } = string.Empty;
        public string CodigoTest { get; set; } = string.Empty;
        public string? DescripcionTest { get; set; }
        public string FechaGeneracion { get; set; } = string.Empty;
        public decimal AnsiedadScore { get; set; }
        public decimal EstresScore { get; set; }
        public decimal MotivacionScore { get; set; }
        public decimal AutoestimaScore { get; set; }
        public decimal PropositoScore { get; set; }
        public string NivelRiesgo { get; set; } = string.Empty;
        public string EstadoEmocional { get; set; } = string.Empty;
        public string? Observacion { get; set; }
    }
}
