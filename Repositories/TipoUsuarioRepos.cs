using EducaMente.AccessData;
using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Models;
using System.Data;

namespace EducaMente.Repositories
{
    public class TipoUsuarioRepos : I_TipoUsuario
    {
        private readonly AccesoData accessData;
        private readonly IConfiguration configuration;

        public TipoUsuarioRepos(AccesoData accessData, IConfiguration configuration)
        {
            this.accessData = accessData;
            this.configuration = configuration;
        }

        public async Task<string> AddAsync(TipoUsuarioAddDTO tipoUsuarioDTO)
        {
            try
            {
                using var connection = accessData.GetConnection();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.CreateTipoUsuario";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = tipoUsuarioDTO.Nombre;
                cmd.Parameters.Add("@Rol", SqlDbType.VarChar, 50).Value = tipoUsuarioDTO.Rol;
                await cmd.ExecuteNonQueryAsync();

                return "Tipo de usuario creado correctamente";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task<IEnumerable<TipoUsuarioModel>> GetAllAsync()
        {
            var tipousuarios = new List<TipoUsuarioModel>();

            try
            {
                using var connection = accessData.GetConnection();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.GetTipoUsuario";
                cmd.CommandType = CommandType.StoredProcedure;

                using var result = await cmd.ExecuteReaderAsync();
                while (result.Read())
                {
                    tipousuarios.Add(new TipoUsuarioModel
                    {
                        Id = result["Id"].ToString(),
                        Nombre = result["Nombre"].ToString(),
                        Rol = result["Rol"].ToString(),
                        Estado = Convert.ToInt16(result["Estado"])
                    });
                }

                return tipousuarios;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<TipoUsuarioModel> GetItemAsync(string id)
        {
            try
            {
                using var connection = accessData.GetConnection();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.GetTipoUsuarioById";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id", SqlDbType.VarChar, 100).Value = id;

                using var result = await cmd.ExecuteReaderAsync();
                if (result.Read())
                {
                    return new TipoUsuarioModel
                    {
                        Id = result["Id"].ToString(),
                        Nombre = result["Nombre"].ToString(),
                        Rol = result["Rol"].ToString(),
                        Estado = Convert.ToInt16(result["Estado"])
                    };
                }

                throw new KeyNotFoundException($"No se encontró el tipo de usuario con el ID: {id}");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> UpdateAsync(TipoUsuarioDTO tipoUsuarioDTO)
        {
            try
            {
                using var connection = accessData.GetConnection();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.UpdateTipoUsuario";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id", SqlDbType.VarChar, 50).Value = tipoUsuarioDTO.Id;
                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = tipoUsuarioDTO.Nombre;
                cmd.Parameters.Add("@Rol", SqlDbType.VarChar, 50).Value = tipoUsuarioDTO.Rol;
                cmd.Parameters.Add("@Estado", SqlDbType.SmallInt).Value = tipoUsuarioDTO.Estado;

                await cmd.ExecuteNonQueryAsync();
                return "Tipo de funcionario actualizado correctamente";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
