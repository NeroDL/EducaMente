using EducaMente.DTO;
using EducaMente.Models;

namespace EducaMente.Interface
{
    public interface I_PsicoPreguntaBank
    {
        Task<Response1StringDTO> AddAsync(PsicoPreguntaBankAddDTO dto);
        Task<IEnumerable<PsicoPreguntaBankModel>> SearchByTextAsync(string valortxt, int pageNumber, int pageSize);
        Task<IEnumerable<LikertScaleDTO>> GetAllAsync();
        Task<PsicoPreguntaBankModel?> GetByIdAsync(string preguntaId);
        Task<string> UpdateAsync(PsicoPreguntaBankUpdateDTO dto);
    }
}
