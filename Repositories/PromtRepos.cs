using EducaMente.AccessData;
using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Models;
using System.Data;

namespace EducaMente.Repositories
{
    public class PromtRepos : I_Promt
    {
        private readonly AccesoData accessData;

        public PromtRepos(AccesoData accessData)
        {
            this.accessData = accessData;
        }

        public async Task<Response1StringDTO> AddAsync(PromtModel promt)
        {
            try
            {
                using var connection = accessData.GetConnection();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "dbo.CreatePromt";
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Mapeo de propiedades del DTO a parámetros del stored procedure
                    cmd.Parameters.AddWithValue("@Codigo", promt.Codigo);
                    cmd.Parameters.AddWithValue("@Descripcion", Convert.IsDBNull(promt.Descripcion) ? DBNull.Value : promt.Descripcion);
                    cmd.Parameters.Add("@Promt", SqlDbType.NVarChar, -1).Value = string.IsNullOrEmpty(promt.Promt) ? DBNull.Value : promt.Promt;

                    await cmd.ExecuteNonQueryAsync();

                    return new Response1StringDTO
                    {
                        Id = promt.Codigo,
                        Mensaje = "Promt registrado correctamente"
                    };
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task<IEnumerable<PromtModel>> SearchByTextAsync(string? valorTxt, int pageNumber, int pageSize)
        {
            var parametros = new List<PromtModel>();

            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.GetPromtByText";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ValorTxt", SqlDbType.VarChar, 50).Value = string.IsNullOrWhiteSpace(valorTxt) ? (object)DBNull.Value : valorTxt;
                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {

                    parametros.Add(new PromtModel
                    {
                        Codigo = reader["Codigo"].ToString(),
                        Descripcion = reader["Descripcion"]?.ToString(),
                        Promt = reader["Promt"]?.ToString(),
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return parametros;
        }

        /// <summary>
        /// Obtiene un prompt específico por su código desde ValorString.
        /// </summary>
        /// <param name="codigo">Código del prompt a buscar.</param>
        /// <returns>El texto del prompt almacenado en ValorString.</returns>
        public async Task<string> GetPromptByCodigoAsync(string codigo)
        {
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.GetPromt";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120; // 2 minutos de timeout para prompts largos
                cmd.Parameters.Add("@Codigo", SqlDbType.VarChar, 50).Value = codigo;

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var valorString = reader["Promt"]?.ToString();
                    if (string.IsNullOrEmpty(valorString))
                    {
                        throw new InvalidOperationException($"El prompt con código '{codigo}' no tiene contenido en ValorString.");
                    }
                    return valorString;
                }
                else
                {
                    throw new KeyNotFoundException($"No se encontró el prompt con código: {codigo}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> UpdateAsync(PromtModel promt)
        {
            string message = "";

            try
            {

                using var connection = accessData.GetConnection();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "dbo.UpdatePromt";
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Mapeo de propiedades del DTO a parámetros del stored procedure
                    cmd.Parameters.AddWithValue("@Codigo", promt.Codigo);
                    cmd.Parameters.AddWithValue("@Descripcion", Convert.IsDBNull(promt.Descripcion) ? DBNull.Value : promt.Descripcion);
                    cmd.Parameters.AddWithValue("@Promt", Convert.IsDBNull(promt.Promt) ? DBNull.Value : promt.Promt);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        message = "Parámetro actualizado correctamente";
                    }
                    else
                    {
                        message = "No se encontró ningún parámetro con el código especificado";
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }

            return message;
        }
    }
}
