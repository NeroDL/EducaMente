using EducaMente.AccessData;
using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Utilities;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace EducaMente.Repositories
{
    public class ParametroRepos : I_Parametro
    {
        private readonly AccesoData accessData;

        public ParametroRepos(AccesoData accessData)
        {
            this.accessData = accessData;
        }

        public async Task<Response1StringDTO> AddFromFormAsync(ParametroAddFormDTO parametro)
        {
            try
            {
                string? valorEmailBase64 = null;

                if (parametro.TipoParametro == "Html")
                {
                    if (parametro.HtmlFile == null || parametro.HtmlFile.Length == 0)
                        throw new ArgumentException("Debe subir un archivo .html válido para este tipo de parámetro.");

                    using var stream = new MemoryStream();
                    await parametro.HtmlFile.CopyToAsync(stream);
                    valorEmailBase64 = Convert.ToBase64String(stream.ToArray());
                }

                var dto = new ParametroAddDTO
                {
                    Codigo = parametro.Codigo,
                    Descripcion = parametro.Descripcion,
                    TipoParametro = parametro.TipoParametro,
                    ValorString = parametro.ValorString,
                    ValorInt = parametro.ValorInt,
                    ValorDecimal = parametro.ValorDecimal,
                    ValorDate = parametro.ValorDate,
                    ValorBool = parametro.ValorBool,
                    ValorImgUrl = parametro.ValorImgUrl,
                    ValorHtml = valorEmailBase64
                };

                return await AddAsync(dto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }


        /// <summary>
        /// Agrega un nuevo parámetro a la base de datos.
        /// </summary>
        /// <param name="parametro">Objeto DTO que contiene los datos del parámetro.</param>
        /// <returns>Un mensaje indicando el resultado de la operación.</returns>
        public async Task<Response1StringDTO> AddAsync(ParametroAddDTO parametro)
        {
            try
            {
                using var connection = accessData.GetConnection();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "dbo.CreateParametro";
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Mapeo de propiedades del DTO a parámetros del stored procedure
                    cmd.Parameters.AddWithValue("@Codigo", parametro.Codigo);
                    cmd.Parameters.AddWithValue("@Descripcion", Convert.IsDBNull(parametro.Descripcion) ? DBNull.Value : parametro.Descripcion);
                    cmd.Parameters.AddWithValue("@TipoParametro", parametro.TipoParametro);
                    cmd.Parameters.Add("@ValorString", SqlDbType.NVarChar, -1).Value = string.IsNullOrEmpty(parametro.ValorString) ? DBNull.Value : parametro.ValorString;
                    cmd.Parameters.AddWithValue("@ValorInt", Convert.IsDBNull(parametro.ValorInt) ? DBNull.Value : parametro.ValorInt);
                    cmd.Parameters.AddWithValue("@ValorDecimal", Convert.IsDBNull(parametro.ValorDecimal) ? DBNull.Value : parametro.ValorDecimal);
                    cmd.Parameters.AddWithValue("@ValorDate", Convert.IsDBNull(parametro.ValorDate) ? DBNull.Value : parametro.ValorDate);
                    cmd.Parameters.AddWithValue("@ValorBool", parametro.ValorBool.HasValue ? (short?)((bool)parametro.ValorBool ? 1 : 0) : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ValorImgUrl", Convert.IsDBNull(parametro.ValorImgUrl) ? DBNull.Value : parametro.ValorImgUrl);
                    cmd.Parameters.Add("@ValorHtml", SqlDbType.NVarChar, -1).Value = string.IsNullOrEmpty(parametro.ValorHtml) ? DBNull.Value : parametro.ValorHtml;


                    await cmd.ExecuteNonQueryAsync();

                    return new Response1StringDTO
                    {
                        Id = parametro.Codigo,
                        Mensaje = "Parametro registrado correctamente"
                    };
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un parámetro existente en la base de datos.
        /// </summary>
        /// <param name="parametro">Objeto DTO que contiene los datos actualizados del parámetro.</param>
        /// <returns>Un mensaje indicando el resultado de la operación.</returns>
        public async Task<string> UpdateAsync(ParametroAddFormDTO parametro)
        {
            string message = "";

            try
            {

                string? valorEmailBase64 = null;

                if (parametro.TipoParametro == "Html")
                {
                    if (parametro.HtmlFile == null || parametro.HtmlFile.Length == 0)
                        throw new ArgumentException("Debe subir un archivo .html válido para este tipo de parámetro.");

                    using var stream = new MemoryStream();
                    await parametro.HtmlFile.CopyToAsync(stream);
                    valorEmailBase64 = Convert.ToBase64String(stream.ToArray());
                }

                var dto = new ParametroAddDTO
                {
                    Codigo = parametro.Codigo,
                    Descripcion = parametro.Descripcion,
                    TipoParametro = parametro.TipoParametro,
                    ValorString = parametro.ValorString,
                    ValorInt = parametro.ValorInt,
                    ValorDecimal = parametro.ValorDecimal,
                    ValorDate = parametro.ValorDate,
                    ValorBool = parametro.ValorBool,
                    ValorImgUrl = parametro.ValorImgUrl,
                    ValorHtml = valorEmailBase64
                };

                using var connection = accessData.GetConnection();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "dbo.UpdateParametro";
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Mapeo de propiedades del DTO a parámetros del stored procedure
                    cmd.Parameters.AddWithValue("@Codigo", dto.Codigo);
                    cmd.Parameters.AddWithValue("@Descripcion", Convert.IsDBNull(dto.Descripcion) ? DBNull.Value : dto.Descripcion);
                    cmd.Parameters.AddWithValue("@TipoParametro", dto.TipoParametro);
                    cmd.Parameters.AddWithValue("@ValorString", Convert.IsDBNull(dto.ValorString) ? DBNull.Value : dto.ValorString);
                    cmd.Parameters.AddWithValue("@ValorInt", Convert.IsDBNull(dto.ValorInt) ? DBNull.Value : dto.ValorInt);
                    cmd.Parameters.AddWithValue("@ValorDecimal", Convert.IsDBNull(dto.ValorDecimal) ? DBNull.Value : dto.ValorDecimal);
                    cmd.Parameters.AddWithValue("@ValorDate", Convert.IsDBNull(dto.ValorDate) ? DBNull.Value : dto.ValorDate);
                    cmd.Parameters.AddWithValue("@ValorBool", dto.ValorBool.HasValue ? (short?)((bool)dto.ValorBool ? 1 : 0) : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ValorImgUrl", Convert.IsDBNull(dto.ValorImgUrl) ? DBNull.Value : dto.ValorImgUrl);
                    cmd.Parameters.AddWithValue("@ValorHtml", Convert.IsDBNull(dto.ValorHtml) ? DBNull.Value : dto.ValorHtml);

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

        /// <summary>
        /// Obtiene todos los parámetros de la base de datos.
        /// </summary>
        /// <returns>Una lista de objetos DTO que representan los parámetros.</returns>
        public async Task<IEnumerable<ParametroAddDTO>> GetAllAsync()
        {
            var parametros = new List<ParametroAddDTO>();
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.GetParametro";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Codigo", SqlDbType.VarChar, 50).Value = DBNull.Value;

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string? base64Html = reader["ValorHtml"]?.ToString();
                    //string? html = null;
                    //if (!string.IsNullOrEmpty(base64Html))
                    //{
                    //    var bytes = Convert.FromBase64String(base64Html);
                    //    html = Encoding.UTF8.GetString(bytes);
                    //}

                    parametros.Add(new ParametroAddDTO
                    {
                        Codigo = reader["Codigo"].ToString(),
                        Descripcion = reader["Descripcion"]?.ToString(),
                        TipoParametro = reader["TipoParametro"].ToString(),
                        ValorString = reader["ValorString"]?.ToString(),
                        ValorInt = reader["ValorInt"] != DBNull.Value ? Convert.ToInt32(reader["ValorInt"]) : (int?)null,
                        ValorDecimal = reader["ValorDecimal"] != DBNull.Value ? Convert.ToDecimal(reader["ValorDecimal"]) : (decimal?)null,
                        ValorDate = reader["ValorDate"] != DBNull.Value ? Convert.ToDateTime(reader["ValorDate"]) : (DateTime?)null,
                        ValorBool = reader["ValorBool"] != DBNull.Value ? Convert.ToBoolean(Convert.ToInt32(reader["ValorBool"])) : (bool?)null,
                        ValorImgUrl = reader["ValorImgUrl"]?.ToString(),
                        ValorHtml = base64Html
                    });
                }

                return parametros;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene un parámetro específico por su código.
        /// </summary>
        /// <param name="codigo">Código del parámetro a buscar.</param>
        /// <returns>Un objeto DTO que representa el parámetro encontrado.</returns>
        public async Task<ParametroAddDTO> GetItemAsync(string codigo)
        {
            ParametroAddDTO parametro = null;
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.GetParametro";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120; // 2 minutos de timeout para prompts largos
                cmd.Parameters.Add("@Codigo", SqlDbType.VarChar, 50).Value = codigo;

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    string? base64Html = reader["ValorHtml"]?.ToString();
                    //string? html = null;
                    //if (!string.IsNullOrEmpty(base64Html))
                    //{
                    //    var bytes = Convert.FromBase64String(base64Html);
                    //    html = Encoding.UTF8.GetString(bytes);
                    //}

                    parametro = new ParametroAddDTO
                    {
                        Codigo = reader["Codigo"].ToString(),
                        Descripcion = reader["Descripcion"]?.ToString(),
                        TipoParametro = reader["TipoParametro"].ToString(),
                        ValorString = reader["ValorString"]?.ToString(),
                        ValorInt = reader["ValorInt"] != DBNull.Value ? Convert.ToInt32(reader["ValorInt"]) : (int?)null,
                        ValorDecimal = reader["ValorDecimal"] != DBNull.Value ? Convert.ToDecimal(reader["ValorDecimal"]) : (decimal?)null,
                        ValorDate = reader["ValorDate"] != DBNull.Value ? Convert.ToDateTime(reader["ValorDate"]) : (DateTime?)null,
                        ValorBool = reader["ValorBool"] != DBNull.Value ? Convert.ToBoolean(Convert.ToInt32(reader["ValorBool"])) : (bool?)null,
                        ValorImgUrl = reader["ValorImgUrl"]?.ToString(),
                        ValorHtml = base64Html
                    };
                }
                else
                {
                    throw new KeyNotFoundException("No se encontró parámetro con el código especificado.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return parametro;
        }

        public async Task<List<CheckConstraintDTO>> GetDominioAll()
        {
            var constraints = new List<CheckConstraintDTO>();

            try
            {
                using var connection = accessData.GetConnection();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "dbo.GetAllCheckConstraints"; // Llamamos al procedimiento almacenado
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var constraint = new CheckConstraintDTO
                            {
                                ConstraintName = reader["ConstraintName"].ToString()!,
                                TableName = reader["TableName"].ToString()!,
                                ColumnName = reader["ColumnName"].ToString()!,
                                AllowedValues = ExtractAllowedValues(reader["CheckDefinition"].ToString()!)
                            };

                            constraints.Add(constraint);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return constraints;
        }

        public async Task<List<string>> GetDominioByNombreAsync(string constraintName)
        {
            var allowedValues = new List<string>();

            try
            {
                using var connection = accessData.GetConnection();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "dbo.GetCheckConstraintValues"; // Procedimiento almacenado
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ConstraintName", SqlDbType.VarChar, 255).Value = constraintName;

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            allowedValues = ExtractAllowedValues(reader["CheckDefinition"].ToString()!);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"'{constraintName}': {ex.Message}");
            }

            return allowedValues;
        }

        private List<string> ExtractAllowedValues(string checkDefinition)
        {
            var values = new List<string>();

            var matches = Regex.Matches(checkDefinition, @"'([^']*)'");
            foreach (Match match in matches)
            {
                values.Add(match.Groups[1].Value);
            }

            return values;
        }

        public async Task<IEnumerable<ParametroAddDTO>> SearchByTextAsync(string? valorTxt, int pageNumber, int pageSize)
        {
            var parametros = new List<ParametroAddDTO>();

            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.GetParametroByText";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ValorTxt", SqlDbType.VarChar, 50).Value = string.IsNullOrWhiteSpace(valorTxt) ? (object)DBNull.Value : valorTxt;
                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string? base64Html = reader["ValorHtml"]?.ToString();
                    string? html = null;
                    if (!string.IsNullOrEmpty(base64Html))
                    {
                        try
                        {
                            var bytes = Convert.FromBase64String(base64Html);
                            html = Encoding.UTF8.GetString(bytes);
                        }
                        catch
                        {
                            html = base64Html; // Si no es base64, retorna el valor tal cual
                        }
                    }

                    parametros.Add(new ParametroAddDTO
                    {
                        Codigo = reader["Codigo"].ToString(),
                        Descripcion = reader["Descripcion"]?.ToString(),
                        TipoParametro = reader["TipoParametro"].ToString(),
                        ValorString = reader["ValorString"]?.ToString(),
                        ValorInt = reader["ValorInt"] != DBNull.Value ? Convert.ToInt32(reader["ValorInt"]) : (int?)null,
                        ValorDecimal = reader["ValorDecimal"] != DBNull.Value ? Convert.ToDecimal(reader["ValorDecimal"]) : (decimal?)null,
                        ValorDate = reader["ValorDate"] != DBNull.Value ? Convert.ToDateTime(reader["ValorDate"]) : (DateTime?)null,
                        ValorBool = reader["ValorBool"] != DBNull.Value ? Convert.ToBoolean(Convert.ToInt32(reader["ValorBool"])) : (bool?)null,
                        ValorImgUrl = reader["ValorImgUrl"]?.ToString(),
                        ValorHtml = html
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return parametros;
        }

        public async Task<string> GetHtmlByCodigoAsync(string codigo)
        {
            string htmlContent = null;
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.GetParametro";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Codigo", SqlDbType.VarChar, 50).Value = codigo;

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var tipoParametro = reader["TipoParametro"]?.ToString();
                    if (!string.Equals(tipoParametro, "Html", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException($"El parámetro con código '{codigo}' no es de tipo HTML.");

                    htmlContent = reader["ValorHtml"]?.ToString();
                    if (string.IsNullOrWhiteSpace(htmlContent))
                        throw new KeyNotFoundException($"El parámetro con código '{codigo}' no contiene contenido HTML.");

                    // Intentar decodificar siempre
                    try
                    {
                        htmlContent = Base64Converter.DecodeBase64(htmlContent);
                    }
                    catch
                    {
                        // Si falla la conversión, dejamos el valor tal cual
                    }
                }
                else
                {
                    throw new KeyNotFoundException($"No se encontró parámetro con el código '{codigo}'.");
                }
            }
            catch (Exception ex) when (!(ex is InvalidOperationException || ex is KeyNotFoundException))
            {
                throw new Exception(ex.Message);
            }

            return htmlContent;
        }

    }
}
