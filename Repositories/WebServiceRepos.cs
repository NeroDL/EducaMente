using EducaMente.AccessData;
using EducaMente.Domain;
using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Models;
using EducaMente.Utilities;
using System.Data;
using System.Data.SqlClient;

namespace EducaMente.Repositories
{
    public class WebServiceRepos : I_WebService
    {
        private readonly AccesoData accessData;
        private readonly IConfiguration configuration;

        public WebServiceRepos(AccesoData accessData, IConfiguration configuration)
        {
            this.accessData = accessData;
            this.configuration = configuration;
        }

        public async Task<string> AddAsync(WebServiceAddDTO webServiceDTO)
        {
            string message = "";

            try
            {
                using (var connection = accessData.GetConnection())
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "dbo.CreateWebService";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@EntidadId", SqlDbType.Int).Value = (object?)webServiceDTO.EntidadId ?? DBNull.Value;
                        cmd.Parameters.Add("@ApiKey", SqlDbType.NVarChar, -1).Value = webServiceDTO.ApiKey;
                        cmd.Parameters.Add("@TipoEnvio", SqlDbType.Int).Value = (object?)webServiceDTO.TipoEnvio ?? DBNull.Value;
                        cmd.Parameters.Add("@Tipo", SqlDbType.Int).Value = (int)webServiceDTO.Tipo;
                        cmd.Parameters.Add("@Servicio", SqlDbType.NVarChar, 100).Value = (object?)webServiceDTO.Servicio ?? DBNull.Value;
                        cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value = (object?)webServiceDTO.Descripcion ?? DBNull.Value;
                        cmd.Parameters.Add("@FechaCreacion", SqlDbType.DateTime).Value = UtilidadesTiempo.ObtenerFechaColombia();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                message = "Servicio web creado correctamente";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }

            return message;
        }

        public async Task<WebServiceModel> GetItemAsync(int id)
        {
            try
            {
                using (var connection = accessData.GetConnection())
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "dbo.GetWebServiceById";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return MapFromReader(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return new WebServiceModel { ApiKey = string.Empty };
        }

        public async Task<WebServiceModel> GetByTipoAsync(TipoWebService tipo)
        {
            try
            {
                using (var connection = accessData.GetConnection())
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "dbo.GetWebServiceByTipo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Tipo", SqlDbType.Int).Value = (int)tipo;

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return MapFromReader(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return new WebServiceModel { ApiKey = string.Empty };
        }

        public async Task<string> UpdateAsync(WebServiceUpdateDTO webServiceDTO)
        {
            try
            {
                using (var connection = accessData.GetConnection())
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "dbo.UpdateWebService";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = webServiceDTO.Id;
                    cmd.Parameters.Add("@EntidadId", SqlDbType.Int).Value = (object?)webServiceDTO.EntidadId ?? DBNull.Value;
                    cmd.Parameters.Add("@ApiKey", SqlDbType.NVarChar, -1).Value = webServiceDTO.ApiKey;
                    cmd.Parameters.Add("@TipoEnvio", SqlDbType.Int).Value = (object?)webServiceDTO.TipoEnvio ?? DBNull.Value;
                    cmd.Parameters.Add("@Tipo", SqlDbType.Int).Value = (int)webServiceDTO.Tipo;
                    cmd.Parameters.Add("@Servicio", SqlDbType.NVarChar, 100).Value = (object?)webServiceDTO.Servicio ?? DBNull.Value;
                    cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value = (object?)webServiceDTO.Descripcion ?? DBNull.Value;
                    cmd.Parameters.Add("@ModeloPorDefecto", SqlDbType.NVarChar, 100).Value = (object?)webServiceDTO.ModeloPorDefecto ?? DBNull.Value;
                    cmd.Parameters.Add("@EndpointBase", SqlDbType.NVarChar, 500).Value = (object?)webServiceDTO.EndpointBase ?? DBNull.Value;
                    cmd.Parameters.Add("@MaxTokens", SqlDbType.Int).Value = (object?)webServiceDTO.MaxTokens ?? DBNull.Value;
                    var temperatureParam = cmd.Parameters.Add("@Temperature", SqlDbType.Decimal);
                    temperatureParam.Precision = 3;
                    temperatureParam.Scale = 2;
                    temperatureParam.Value = (object?)webServiceDTO.Temperature ?? DBNull.Value;
                    cmd.Parameters.Add("@FechaModificacion", SqlDbType.DateTime).Value = UtilidadesTiempo.ObtenerFechaColombia();

                    var rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0 ? "Servicio web actualizado correctamente" : "No se realizaron cambios.";
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        private WebServiceModel MapFromReader(SqlDataReader reader)
        {
            return new WebServiceModel
            {
                Id = reader["Id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Id"]),
                EntidadId = reader["EntidadId"] == DBNull.Value ? null : Convert.ToInt32(reader["EntidadId"]),
                ApiKey = reader["ApiKey"] == DBNull.Value ? string.Empty : reader["ApiKey"].ToString(),
                TipoEnvio = reader["TipoEnvio"] == DBNull.Value ? null : Convert.ToInt32(reader["TipoEnvio"]),
                Tipo = reader["Tipo"] == DBNull.Value ? null : (TipoWebService)Convert.ToInt32(reader["Tipo"]),
                Servicio = reader["Servicio"] == DBNull.Value ? null : reader["Servicio"].ToString(),
                Descripcion = reader["Descripcion"] == DBNull.Value ? null : reader["Descripcion"].ToString(),
                FechaCreacion = reader["FechaCreacion"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaCreacion"]),
                FechaModificacion = reader["FechaModificacion"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaModificacion"]),

                // Nuevas propiedades
                ModeloPorDefecto = reader["ModeloPorDefecto"] == DBNull.Value ? null : reader["ModeloPorDefecto"].ToString(),
                EndpointBase = reader["EndpointBase"] == DBNull.Value ? null : reader["EndpointBase"].ToString(),
                MaxTokens = reader["MaxTokens"] == DBNull.Value ? null : Convert.ToInt32(reader["MaxTokens"]),
                Temperature = reader["Temperature"] == DBNull.Value ? null : Convert.ToDecimal(reader["Temperature"])
            };
        }
    }
}
