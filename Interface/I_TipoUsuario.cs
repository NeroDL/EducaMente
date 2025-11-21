using EducaMente.DTO;
using EducaMente.Models;

namespace EducaMente.Interface
{
    public interface I_TipoUsuario
    {
        Task<string> AddAsync(TipoUsuarioAddDTO tipousuarioDTO);
        Task<string> UpdateAsync(TipoUsuarioDTO tipousuarioDTO);
        Task<IEnumerable<TipoUsuarioModel>> GetAllAsync();
        Task<TipoUsuarioModel> GetItemAsync(string id);
    }
}
