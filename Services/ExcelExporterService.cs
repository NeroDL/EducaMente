using ClosedXML.Excel;
using EducaMente.AccessData;
using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace EducaMente.Services
{
    public class ExcelExporterService : I_ExcelExporterService
    {
        private readonly AccesoData accessData;
        private readonly I_Usuario _usuarioRepos;

        public ExcelExporterService(AccesoData accessData, I_Usuario usuarioRepos)
        {
            this.accessData = accessData;
            _usuarioRepos = usuarioRepos;
        }

        public async Task<FileStreamResult> ExportarPerfilesAsync()
        {
            // Obtener todos los perfiles psicológicos de los estudiantes
            var perfiles = await _usuarioRepos.GetAllPerfilPsicoActualAsync();

            // Mapear los perfiles al DTO ExportarPerfilesDTO
            var perfilesExportables = perfiles.Select(p => UtilitiesDTO.MapToExportarPerfilesDTO(p));

            // Llamar al método para exportar los perfiles a Excel
            return ExportarPerfilesAPdfAsync(perfilesExportables);
        }

        private FileStreamResult ExportarPerfilesAPdfAsync(IEnumerable<ExportarPerfilesDTO> perfiles)
        {
            // Crear un nuevo libro de trabajo (Excel)
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Perfiles Psicológicos");

            // Agregar los encabezados de las columnas
            worksheet.Cell(1, 1).Value = "Nombre";
            worksheet.Cell(1, 2).Value = "Fecha Evaluacion";
            worksheet.Cell(1, 3).Value = "AnsiedadScore";
            worksheet.Cell(1, 4).Value = "EstresScore";
            worksheet.Cell(1, 5).Value = "MotivacionScore";
            worksheet.Cell(1, 6).Value = "AutoestimaScore";
            worksheet.Cell(1, 7).Value = "PropositoScore";
            worksheet.Cell(1, 8).Value = "Nivel de Riesgo";
            worksheet.Cell(1, 9).Value = "Estado Emocional";
            worksheet.Cell(1, 10).Value = "Observacion";

            // Agregar los datos de los perfiles
            int row = 2;
            foreach (var perfil in perfiles)
            {
                worksheet.Cell(row, 1).Value = perfil.NombreUsuario;
                worksheet.Cell(row, 2).Value = perfil.FechaEvaluacion;
                worksheet.Cell(row, 3).Value = perfil.AnsiedadScore;
                worksheet.Cell(row, 4).Value = perfil.EstresScore;
                worksheet.Cell(row, 5).Value = perfil.MotivacionScore;
                worksheet.Cell(row, 6).Value = perfil.AutoestimaScore;
                worksheet.Cell(row, 7).Value = perfil.PropositoScore;
                worksheet.Cell(row, 8).Value = perfil.NivelRiesgo;
                worksheet.Cell(row, 9).Value = perfil.EstadoEmocional;
                worksheet.Cell(row, 10).Value = perfil.Observacion;

                row++;
            }

            // Crear un MemoryStream para almacenar el archivo en memoria
            var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);

            // Es importante reiniciar el puntero del stream al principio antes de devolverlo
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Devolver el archivo Excel como una descarga
            return new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "PerfilesPsicoEstudiantes.xlsx"
            };
        }
    }
}
