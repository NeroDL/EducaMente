namespace EducaMente.DTO
{
    public class UsuarioTestResultadoDTO
    {
        public string UsuarioTestId { get; set; } = string.Empty;
        public decimal AnsiedadScore { get; set; }
        public decimal EstresScore { get; set; }
        public decimal MotivacionScore { get; set; }
        public decimal AutoestimaScore { get; set; }
        public decimal PropositoScore { get; set; }
        public string NivelRiesgo { get; set; } = string.Empty;
        public string EstadoEmocional { get; set; } = string.Empty;
    }
}
