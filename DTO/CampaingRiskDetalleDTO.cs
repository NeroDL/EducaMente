namespace EducaMente.DTO
{
    public class CampaingRiskDetalleDTO
    {
        public CampaingRiskCabeceraDTO Cabecera { get; set; } = new();
        public List<CampaingRiskEstudianteDTO> Estudiantes { get; set; } = new();
    }
}
