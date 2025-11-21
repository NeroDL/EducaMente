namespace EducaMente.DTO
{
    public class CampaingRiskEstudianteDTO
    {
        public string CampaingStudentId { get; set; } = string.Empty;
        public string CampaingId { get; set; } = string.Empty;
        public string EstudianteId { get; set; } = string.Empty;
        public string NombreEstudiante { get; set; } = string.Empty;
        public string CorreoEstudiante { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? UltimoUsuarioTestId { get; set; }
        public string? FechaUltimoTest { get; set; }
    }
}
