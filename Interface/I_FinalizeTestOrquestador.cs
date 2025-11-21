using EducaMente.DTO;

namespace EducaMente.Interface
{
    public interface I_FinalizeTestOrquestador
    {
        Task<UsuarioTestResultadoDTO> EndTestAsync(UsuarioTestFinalizadoDTO dto);
    }
}
