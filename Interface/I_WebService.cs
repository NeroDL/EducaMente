using EducaMente.Domain;
using EducaMente.DTO;
using EducaMente.Models;

namespace EducaMente.Interface
{
    public interface I_WebService
    {
        Task<string> AddAsync(WebServiceAddDTO webServiceDTO);
        Task<WebServiceModel> GetItemAsync(int id);
        Task<WebServiceModel> GetByTipoAsync(TipoWebService tipo);
        Task<string> UpdateAsync(WebServiceUpdateDTO webServiceDTO);
    }
}
