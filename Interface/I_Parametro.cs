using EducaMente.DTO;

namespace EducaMente.Interface
{
    public interface I_Parametro
    {
        Task<Response1StringDTO> AddFromFormAsync(ParametroAddFormDTO parametro);
        Task<Response1StringDTO> AddAsync(ParametroAddDTO parametro);
        Task<string> UpdateAsync(ParametroAddFormDTO parametro);
        Task<IEnumerable<ParametroAddDTO>> GetAllAsync();
        Task<List<CheckConstraintDTO>> GetDominioAll();
        Task<List<string>> GetDominioByNombreAsync(string constraintName);
        Task<ParametroAddDTO> GetItemAsync(string codigo);
        Task<IEnumerable<ParametroAddDTO>> SearchByTextAsync(string? valorTxt, int pageNumber, int pageSize);
        Task<string> GetHtmlByCodigoAsync(string codigo);
    }
}
