using ClosedXML.Excel;
using EducaMente.DTO;
using Microsoft.AspNetCore.Mvc;

namespace EducaMente.Utilities
{
    public class ExcelExporter
    {
        // Método para exportar los datos de los perfiles psicológicos a Excel en memoria
        public async Task<IActionResult> ExportarPerfilesAPdfAsync(IEnumerable<PerfilPsicoActualDTO> perfiles)
        {
            // Crear un nuevo libro de trabajo (Excel)
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Perfiles Psicológicos");

            // Agregar los encabezados de las columnas
            worksheet.Cell(1, 1).Value = "Id";
            worksheet.Cell(1, 2).Value = "UsuarioId";
            worksheet.Cell(1, 3).Value = "NombreUsuario";
            worksheet.Cell(1, 4).Value = "FechaEvaluacion";
            worksheet.Cell(1, 5).Value = "AnsiedadScore";
            worksheet.Cell(1, 6).Value = "EstresScore";
            worksheet.Cell(1, 7).Value = "MotivacionScore";
            worksheet.Cell(1, 8).Value = "AutoestimaScore";
            worksheet.Cell(1, 9).Value = "PropositoScore";
            worksheet.Cell(1, 10).Value = "NivelRiesgo";
            worksheet.Cell(1, 11).Value = "EstadoEmocional";
            worksheet.Cell(1, 12).Value = "UltimoTestId";
            worksheet.Cell(1, 13).Value = "UsuarioTestId";
            worksheet.Cell(1, 14).Value = "Observacion";

            // Agregar los datos de los perfiles
            int row = 2;
            foreach (var perfil in perfiles)
            {
                worksheet.Cell(row, 1).Value = perfil.Id;
                worksheet.Cell(row, 2).Value = perfil.UsuarioId;
                worksheet.Cell(row, 3).Value = perfil.NombreUsuario;
                worksheet.Cell(row, 4).Value = perfil.FechaEvaluacion;
                worksheet.Cell(row, 5).Value = perfil.AnsiedadScore;
                worksheet.Cell(row, 6).Value = perfil.EstresScore;
                worksheet.Cell(row, 7).Value = perfil.MotivacionScore;
                worksheet.Cell(row, 8).Value = perfil.AutoestimaScore;
                worksheet.Cell(row, 9).Value = perfil.PropositoScore;
                worksheet.Cell(row, 10).Value = perfil.NivelRiesgo;
                worksheet.Cell(row, 11).Value = perfil.EstadoEmocional;
                worksheet.Cell(row, 12).Value = perfil.UltimoTestId;
                worksheet.Cell(row, 13).Value = perfil.UsuarioTestId;
                worksheet.Cell(row, 14).Value = perfil.Observacion;

                row++;
            }

            // Usar un MemoryStream para almacenar el archivo en memoria
            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);

            // Devolver el archivo Excel como una descarga
            memoryStream.Seek(0, SeekOrigin.Begin); // Resetear el puntero del stream al inicio

            return new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "PerfilesPsicoEstudiantes.xlsx"
            };
        }
    }
}
