using EducaMente.DTO;

namespace EducaMente.Interface
{
    public interface I_CampainRisk
    {
        Task<string> StartCampaingAsync(string resposableId);
        Task<IEnumerable<CampaingRiskResumenDTO>> GetCampaingsByResponsableAsync(string responsableId);
        Task<CampaingRiskDetalleDTO?> GetCampaingDetalleAsync(string campaingId);
    }
}
