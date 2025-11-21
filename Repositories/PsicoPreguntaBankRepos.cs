using EducaMente.AccessData;
using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Models;
using EducaMente.Utilities;
using System.Data;
using System.Globalization;

namespace EducaMente.Repositories
{
    public class PsicoPreguntaBankRepos : I_PsicoPreguntaBank
    {
        private readonly AccesoData accessData;

        public PsicoPreguntaBankRepos(AccesoData accessData)
        {
            this.accessData = accessData;
        }

        public async Task<Response1StringDTO> AddAsync(PsicoPreguntaBankAddDTO dto)
        {
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.PsicoPreguntaBankCreate";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Texto", SqlDbType.VarChar, 600).Value = dto.Texto;
                cmd.Parameters.Add("@Dimension", SqlDbType.VarChar, 30).Value = dto.Dimension;
                cmd.Parameters.Add("@ScaleId", SqlDbType.VarChar, 50).Value = dto.ScaleId;
                cmd.Parameters.Add("@Fuente", SqlDbType.VarChar, 20).Value = dto.Fuente;
                cmd.Parameters.Add("@FechaCreacion", SqlDbType.DateTime).Value = UtilidadesTiempo.ObtenerFechaColombia();

                var outputParam = cmd.Parameters.Add("@NuevaPreguntaId", SqlDbType.VarChar, 50);
                outputParam.Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();

                return new Response1StringDTO
                {
                    Id = outputParam.Value?.ToString() ?? string.Empty,
                    Mensaje = "Pregunta creada correctamente"
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<PsicoPreguntaBankModel?> GetByIdAsync(string preguntaId)
        {
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.PsicoPreguntaBankGetById";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@PreguntaId", SqlDbType.VarChar, 50).Value = preguntaId;

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new PsicoPreguntaBankModel
                    {
                        Id = reader["Id"].ToString()!,
                        Texto = reader["Texto"]?.ToString() ?? string.Empty,
                        Dimension = reader["Dimension"]?.ToString() ?? string.Empty,
                        ScaleId = reader["ScaleId"]?.ToString() ?? string.Empty,
                        Estado = reader["Estado"]?.ToString() ?? string.Empty,
                        Fuente = reader["Fuente"]?.ToString() ?? string.Empty,
                        FechaCreacion = reader["FechaCreacion"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaCreacion"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO"))
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<LikertScaleDTO>> GetAllAsync()
        {
            var lista = new List<LikertScaleDTO>();

            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.LikertScaleGetAll";
                cmd.CommandType = CommandType.StoredProcedure;

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    lista.Add(new LikertScaleDTO
                    {
                        Id = reader["Id"]?.ToString() ?? string.Empty,
                        Nombre = reader["Nombre"]?.ToString() ?? string.Empty,
                        Descripcion = reader["Descripcion"] == DBNull.Value ? string.Empty : reader["Descripcion"]?.ToString()
                    });
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<PsicoPreguntaBankModel>> SearchByTextAsync(string valortxt, int pageNumber, int pageSize)
        {
            var resultados = new List<PsicoPreguntaBankModel>();

            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.PsicoPreguntaBankByText";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@ValorTxt", SqlDbType.VarChar, 100).Value = string.IsNullOrWhiteSpace(valortxt) ? (object)DBNull.Value : valortxt;
                cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber <= 0 ? 1 : pageNumber;
                cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize <= 0 ? 10 : pageSize;

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var item = new PsicoPreguntaBankModel
                    {
                        Id = reader["Id"].ToString()!,
                        Texto = reader["Texto"]?.ToString() ?? string.Empty,
                        Dimension = reader["Dimension"]?.ToString() ?? string.Empty,
                        ScaleId = reader["ScaleId"]?.ToString() ?? string.Empty,
                        Estado = reader["Estado"]?.ToString() ?? string.Empty,
                        Fuente = reader["Fuente"]?.ToString() ?? string.Empty,
                        FechaCreacion = reader["FechaCreacion"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaCreacion"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO"))
                    };

                    resultados.Add(item);
                }

                return resultados;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> UpdateAsync(PsicoPreguntaBankUpdateDTO dto)
        {
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.PsicoPreguntaBankUpdate";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Id", SqlDbType.VarChar, 50).Value = dto.Id;
                cmd.Parameters.Add("@Texto", SqlDbType.VarChar, 600).Value = (object?)dto.Texto ?? DBNull.Value;
                cmd.Parameters.Add("@Dimension", SqlDbType.VarChar, 30).Value = (object?)dto.Dimension ?? DBNull.Value;
                cmd.Parameters.Add("@ScaleId", SqlDbType.VarChar, 50).Value = (object?)dto.ScaleId ?? DBNull.Value;
                cmd.Parameters.Add("@Estado", SqlDbType.VarChar, 20).Value = (object?)dto.Estado ?? DBNull.Value;
                cmd.Parameters.Add("@Fuente", SqlDbType.VarChar, 20).Value =(object?)dto.Fuente ?? DBNull.Value;

                var rows = await cmd.ExecuteNonQueryAsync();

                return rows > 0
                    ? "Pregunta actualizada correctamente"
                    : "No se aplicaron cambios a la pregunta.";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
