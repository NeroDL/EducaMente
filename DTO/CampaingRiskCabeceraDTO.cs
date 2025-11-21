namespace EducaMente.DTO
{
    public class CampaingRiskCabeceraDTO
    {
        public string Id { get; set; } = string.Empty;
        public string ResponsableId { get; set; } = string.Empty;
        public string NombreResponsable { get; set; } = string.Empty;
        public string? FechaInicio { get; set; }
        public string? FechaFin { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int TotalEstudiantes { get; set; }
        public int EstudiantesCompletados { get; set; }
    }
}
