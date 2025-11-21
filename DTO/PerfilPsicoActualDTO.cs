namespace EducaMente.DTO
{
    public class PerfilPsicoActualDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string FechaEvaluacion { get; set; } = string.Empty;
        public decimal AnsiedadScore { get; set; }
        public decimal EstresScore { get; set; }
        public decimal MotivacionScore { get; set; }
        public decimal AutoestimaScore { get; set; }
        public decimal PropositoScore { get; set; }
        public string NivelRiesgo { get; set; } = string.Empty;
        public string EstadoEmocional { get; set; } = string.Empty;
        public string UltimoTestId { get; set; } = string.Empty;
        public string UsuarioTestId { get; set; } = string.Empty;
        public string? Observacion { get; set; }
    }
}
