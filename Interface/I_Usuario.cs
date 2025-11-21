using EducaMente.DTO;
using EducaMente.Models;

namespace EducaMente.Interface
{
    public interface I_Usuario
    {
        Task<Response1StringDTO> AddAsync(UsuarioAddDTO usuarioDTO);
        Task<IEnumerable<UsuarioModel>> GetAllAsync();
        Task<UsuarioModel> GetItemAsync(string id);
        Task<IEnumerable<AlertaDTO>> GetAlertasByResponsableAsync(string responsableId);
        Task<string> UpdateAsync(UsuarioUpdateDTO usuarioDTO);
        Task<string> UpdatePasswordAsync(UsuarioPasswordDTO usuario);
        Task<string> ForgotPasswordAsync(string email);
        Task<string> ResetPasswordAsync(UsuarioResetPasswordDTO dto);
        Task<PerfilPsicoActualDTO?> GetPerfilPsicoActualAsync(string usuarioId);
        Task<IEnumerable<PerfilPsicoActualDTO?>> GetAllPerfilPsicoActualAsync();
        Task<IEnumerable<PerfilPsicoHistDTO>> GetPerfilPsicoHistAsync(string usuarioId);
        Task<UsuarioLogueadoDTO> Login(UsuarioLoginDTO usuario);
        Task<string> ResetFirstTestDone(string usuarioId);
    }
}
