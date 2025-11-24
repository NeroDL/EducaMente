using EducaMente.DTO;
using Microsoft.AspNetCore.Mvc;

namespace EducaMente.Interface
{
    public interface I_ExcelExporterService
    {
        Task<FileStreamResult> ExportarPerfilesAsync();
        Task<FileStreamResult> ExportarPerfilesHistAsync(string usuarioId);
    }
}
