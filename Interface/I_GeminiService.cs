using EducaMente.DTO;

namespace EducaMente.Interface
{
    public interface I_GeminiService
    {
        Task<GeminiAnalisisPerfilResponseDTO> GenerarObservacionPerfilDesdeTestAsync(string usuarioTestId);
        Task<GeminiGenerarTestServiceResponseDTO> GenerarTestPersonalizadoDesdeTestAsync(string usuarioTestId);
    }
}
