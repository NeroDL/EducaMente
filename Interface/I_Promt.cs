using EducaMente.DTO;
using EducaMente.Models;

namespace EducaMente.Interface
{
    public interface I_Promt
    {
        Task<Response1StringDTO> AddAsync(PromtModel promt);
        Task<string> UpdateAsync(PromtModel parametro);
        Task<IEnumerable<PromtModel>> SearchByTextAsync(string? valorTxt, int pageNumber, int pageSize);
        Task<string> GetPromptByCodigoAsync(string codigo);
    }
}
