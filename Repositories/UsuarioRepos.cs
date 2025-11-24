using EducaMente.AccessData;
using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Models;
using EducaMente.Utilities;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EducaMente.Repositories
{
    public class UsuarioRepos : I_Usuario
    {
        private readonly AccesoData accessData;
        private readonly NotificationManager notificacionManager;
        private readonly IConfiguration configuration;
        private readonly I_Parametro _parametroRepos;

        public UsuarioRepos(AccesoData accessData, IConfiguration configuration, NotificationManager notificacionManager, I_Parametro parametroRepos)
        {
            this.accessData = accessData;
            this.configuration = configuration;
            this.notificacionManager = notificacionManager;
            _parametroRepos = parametroRepos;
        }
        public async Task<Response1StringDTO> AddAsync(UsuarioAddDTO usuarioDTO)
        {
            try
            {
                using var connection = accessData.GetConnection();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.CreateUsuario";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Documento", SqlDbType.VarChar, 15).Value = usuarioDTO.Documento;
                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = usuarioDTO.Nombre;
                cmd.Parameters.Add("@TipoUsuId", SqlDbType.VarChar, 50).Value = usuarioDTO.TipoUsuId;
                cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = usuarioDTO.Correo;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar, 100).Value = usuarioDTO.Password;
                cmd.Parameters.Add("@Celular", SqlDbType.VarChar, 10).Value = usuarioDTO.Celular;

                var outputParam = cmd.Parameters.Add("@NewId", SqlDbType.VarChar, 50);
                outputParam.Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();

                return new Response1StringDTO
                {
                    Id = outputParam.Value.ToString(),
                    Mensaje = "Funcionario registrado correctamente"
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<UsuarioModel>> GetAllAsync()
        {
            var usuarios = new List<UsuarioModel>();
            try
            {
                using var connection = accessData.GetConnection();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.GetUsuario";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id", SqlDbType.VarChar, 50).Value = DBNull.Value;

                using var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    usuarios.Add(new UsuarioModel
                    {
                        Id = reader["Id"].ToString() ?? string.Empty,
                        Documento = reader["Documento"].ToString() ?? string.Empty,
                        Nombre = reader["Nombre"].ToString() ?? string.Empty,
                        Correo = reader["Correo"].ToString() ?? string.Empty,
                        Celular = reader["Celular"].ToString() ?? string.Empty,
                        TipoUsuId = reader["TipoUsuId"].ToString() ?? string.Empty,
                        Rol = reader["Rol"].ToString() ?? string.Empty,
                        Estado = reader["Estado"].ToString() ?? string.Empty
                    });
                }

                return usuarios;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<UsuarioModel> GetItemAsync(string id)
        {
            try
            {
                using var connection = accessData.GetConnection();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.GetUsuario";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id", SqlDbType.VarChar, 100).Value = id;

                using var reader = await cmd.ExecuteReaderAsync();
                if (reader.Read())
                {
                    return new UsuarioModel
                    {
                        Id = reader["Id"].ToString() ?? string.Empty,
                        Documento = reader["Documento"].ToString() ?? string.Empty,
                        Nombre = reader["Nombre"].ToString() ?? string.Empty,
                        Correo = reader["Correo"].ToString() ?? string.Empty,
                        Celular = reader["Celular"].ToString() ?? string.Empty,
                        TipoUsuId = reader["TipoUsuId"].ToString() ?? string.Empty,
                        Rol = reader["Rol"].ToString() ?? string.Empty,
                        Estado = reader["Estado"].ToString() ?? string.Empty
                    };
                }

                throw new KeyNotFoundException($"No se encontró un usuario con el ID: {id}");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<AlertaDTO>> GetAlertasByResponsableAsync(string responsableId)
        {
            var alertas = new List<AlertaDTO>();

            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.AlertaRiesgoGetByResponsable";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@ResponsableId", SqlDbType.VarChar, 50).Value = responsableId;

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    alertas.Add(new AlertaDTO
                    {
                        Id = reader["Id"]?.ToString() ?? string.Empty,
                        UsuarioId = reader["UsuarioId"]?.ToString() ?? string.Empty,
                        UsuarioTestId = reader["UsuarioTestId"]?.ToString() ?? string.Empty,
                        FechaCreacion = reader["FechaCreacion"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaCreacion"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO")),
                        Asunto = reader["Asunto"]?.ToString() ?? string.Empty,
                        MensajeAlerta = reader["MensajeAlerta"]?.ToString() ?? string.Empty
                    });
                }

                return alertas;
            }
            catch (Exception ex)
            {
                // Dejas que tu middleware capture el error
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<PerfilPsicoActualDTO?>> GetAllPerfilPsicoActualAsync()
        {
            var perfiles = new List<PerfilPsicoActualDTO>();

            try
            {
                using var connection = accessData.GetConnection();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.PerfilPsicoGetAllEstudiantes";
                cmd.CommandType = CommandType.StoredProcedure;

                using var reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    perfiles.Add(new PerfilPsicoActualDTO
                    {
                        Id = reader["Id"].ToString() ?? string.Empty,
                        UsuarioId = reader["UsuarioId"].ToString() ?? string.Empty,
                        NombreUsuario = reader["NombreUsuario"].ToString() ?? string.Empty,
                        FechaEvaluacion = reader["FechaEvaluacion"].ToString() ?? string.Empty,
                        AnsiedadScore = reader.IsDBNull(reader.GetOrdinal("AnsiedadScore")) ? 0 : reader.GetDecimal(reader.GetOrdinal("AnsiedadScore")),
                        EstresScore = reader.IsDBNull(reader.GetOrdinal("EstresScore")) ? 0 : reader.GetDecimal(reader.GetOrdinal("EstresScore")),
                        MotivacionScore = reader.IsDBNull(reader.GetOrdinal("MotivacionScore")) ? 0 : reader.GetDecimal(reader.GetOrdinal("MotivacionScore")),
                        AutoestimaScore = reader.IsDBNull(reader.GetOrdinal("AutoestimaScore")) ? 0 : reader.GetDecimal(reader.GetOrdinal("AutoestimaScore")),
                        PropositoScore = reader.IsDBNull(reader.GetOrdinal("PropositoScore")) ? 0 : reader.GetDecimal(reader.GetOrdinal("PropositoScore")),
                        NivelRiesgo = reader["NivelRiesgo"].ToString() ?? string.Empty,
                        EstadoEmocional = reader["EstadoEmocional"].ToString() ?? string.Empty,
                        UltimoTestId = reader["UltimoTestId"].ToString() ?? string.Empty,
                        UsuarioTestId = reader["UsuarioTestId"].ToString() ?? string.Empty,
                        Observacion = reader["Observacion"] == DBNull.Value ? string.Empty : reader["Observacion"]?.ToString()
                    });
                }

                return perfiles;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<PerfilPsicoActualDTO?> GetPerfilPsicoActualAsync(string usuarioId)
        {
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.PerfilPsicoGetActual";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@UsuarioId", SqlDbType.VarChar, 50).Value = usuarioId;

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new PerfilPsicoActualDTO
                    {
                        Id = reader["Id"]?.ToString() ?? string.Empty,
                        UsuarioId = reader["UsuarioId"]?.ToString() ?? string.Empty,
                        NombreUsuario = reader["NombreUsuario"]?.ToString() ?? string.Empty,
                        FechaEvaluacion = reader["FechaEvaluacion"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaEvaluacion"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO")),
                        AnsiedadScore = reader["AnsiedadScore"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["AnsiedadScore"]),
                        EstresScore = reader["EstresScore"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["EstresScore"]),
                        MotivacionScore = reader["MotivacionScore"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["MotivacionScore"]),
                        AutoestimaScore = reader["AutoestimaScore"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["AutoestimaScore"]),
                        PropositoScore = reader["PropositoScore"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["PropositoScore"]),
                        NivelRiesgo = reader["NivelRiesgo"]?.ToString() ?? string.Empty,
                        EstadoEmocional = reader["EstadoEmocional"]?.ToString() ?? string.Empty,
                        UltimoTestId = reader["UltimoTestId"]?.ToString() ?? string.Empty,
                        UsuarioTestId = reader["UsuarioTestId"]?.ToString() ?? string.Empty,
                        Observacion = reader["Observacion"] == DBNull.Value ? string.Empty : reader["Observacion"]?.ToString()
                    };
                }

                throw new KeyNotFoundException($"El usuario con Id {usuarioId} no tiene un perfil psicológico actual registrado.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<PerfilPsicoHistDTO>> GetPerfilPsicoHistAsync(string usuarioId)
        {
            var lista = new List<PerfilPsicoHistDTO>();

            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.PerfilPsicoGetHist";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@UsuarioId", SqlDbType.VarChar, 50).Value = usuarioId;

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var item = new PerfilPsicoHistDTO
                    {
                        Id = reader["Id"]?.ToString() ?? string.Empty,
                        UsuarioId = reader["UsuarioId"]?.ToString() ?? string.Empty,
                        NombreUsuario = reader["NombreUsuario"]?.ToString() ?? string.Empty,
                        TestId = reader["TestId"]?.ToString() ?? string.Empty,
                        UsuarioTestId = reader["UsuarioTestId"]?.ToString() ?? string.Empty,
                        DescripcionTest = reader["DescripcionTest"] == DBNull.Value ? string.Empty : reader["DescripcionTest"]?.ToString(),
                        FechaGeneracion = reader["FechaGeneracion"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaGeneracion"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO")),
                        AnsiedadScore = reader["AnsiedadScore"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["AnsiedadScore"]),
                        EstresScore = reader["EstresScore"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["EstresScore"]),
                        MotivacionScore = reader["MotivacionScore"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["MotivacionScore"]),
                        AutoestimaScore = reader["AutoestimaScore"] == DBNull.Value? 0m : Convert.ToDecimal(reader["AutoestimaScore"]),
                        PropositoScore = reader["PropositoScore"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["PropositoScore"]),
                        NivelRiesgo = reader["NivelRiesgo"]?.ToString() ?? string.Empty,
                        EstadoEmocional = reader["EstadoEmocional"]?.ToString() ?? string.Empty,
                        Observacion = reader["Observacion"] == DBNull.Value ? string.Empty : reader["Observacion"]?.ToString()
                    };

                    lista.Add(item);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> UpdateAsync(UsuarioUpdateDTO usuarioDTO)
        {
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.UpdateUsuario";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id", SqlDbType.VarChar, 50).Value = usuarioDTO.Id;
                cmd.Parameters.Add("@Documento", SqlDbType.VarChar, 15).Value = usuarioDTO.Documento;
                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = usuarioDTO.Nombre;
                cmd.Parameters.Add("@TipoUsuId", SqlDbType.VarChar, 50).Value = usuarioDTO.TipoUsuId;
                cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = usuarioDTO.Correo;
                cmd.Parameters.Add("@Celular", SqlDbType.VarChar, 10).Value = usuarioDTO.Celular;
                cmd.Parameters.Add("@Estado", SqlDbType.VarChar, 100).Value = usuarioDTO.Estado;

                await cmd.ExecuteNonQueryAsync();
                return "Usuario actualizado correctamente";
            }
            catch (SqlException sqlEx)
            {
                // Devuelve solo el mensaje del RAISERROR
                if (sqlEx.Errors.Count > 0)
                {
                    throw new Exception(sqlEx.Errors[0].Message);
                }

                throw;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> UpdatePasswordAsync(UsuarioPasswordDTO usuario)
        {
            try
            {
                string? hashActual = null;

                // 1. Obtener hash actual usando SP
                using (var connection = accessData.GetConnection())
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "dbo.GetPasswordById";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id", SqlDbType.VarChar, 50).Value = usuario.Id;

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (reader.Read())
                    {
                        hashActual = reader["Password"]?.ToString();
                    }
                }

                if (string.IsNullOrEmpty(hashActual))
                    throw new Exception("Usuario no encontrado.");

                // 2. Verificar contraseña antigua
                if (!Encriptacion.VerificarHash(usuario.PasswordAntigua, hashActual))
                    throw new Exception("La contraseña actual es incorrecta.");

                // 3. Hashear nueva contraseña
                string nuevoHash = Encriptacion.Hashear(usuario.PasswordNueva);

                // 4. Actualizar contraseña con SP
                using (var connection = accessData.GetConnection())
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "dbo.UpdatePassword";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id", SqlDbType.VarChar, 50).Value = usuario.Id;
                    cmd.Parameters.Add("@PasswordNueva", SqlDbType.VarChar, 200).Value = nuevoHash;

                    await cmd.ExecuteNonQueryAsync();
                }

                return "Contraseña actualizada correctamente.";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<UsuarioLogueadoDTO> Login(UsuarioLoginDTO usuario)
        {
            UsuarioLogueadoDTO? usuarioLogueado = null;

            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.GetAutentication";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = usuario.Email; 

                using var result = await cmd.ExecuteReaderAsync();

                if (result.Read())
                {
                    string storedHash = result["Password"]?.ToString() ?? "";

                    // Validación de contraseña usando Argon2
                    if (Encriptacion.VerificarHash(usuario.Password, storedHash))
                    {
                        // Reinicia los intentos fallidos
                        await ResetearIntentosLoginAsync(usuario.Email);

                        usuarioLogueado = new UsuarioLogueadoDTO
                        {
                            Id = result["Id"].ToString(),
                            Documento = result["Documento"].ToString(),
                            Nombre = result["Nombre"].ToString(),
                            Rol = result["Rol"].ToString(),
                            TipoUsuNombre = result["TipoUsuNombre"].ToString(),
                            Email = result["Correo"].ToString(),
                            Estado = result["Estado"].ToString(),
                            FirstTestDone = Convert.ToBoolean(result["FirstTestDone"])
                        };

                        usuarioLogueado = GenerarTokenJWT(usuarioLogueado);
                    }
                    else
                    {
                        // Aumenta los intentos fallidos
                        await AumentarIntentoLoginAsync(usuario.Email);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception(sqlEx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return usuarioLogueado;
        }

        public async Task<string> ResetFirstTestDone(string usuarioId)
        {
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.UsuarioResetFirstTestDone";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@UsuarioId", SqlDbType.VarChar, 50).Value = usuarioId;

                await cmd.ExecuteNonQueryAsync();

                return "LoginBefore restablecido correctamente";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> ForgotPasswordAsync(string email)
        {
            try
            {
                var usuarioId = await ObtenerUsuarioIdPorEmailAsync(email);
                if (string.IsNullOrEmpty(usuarioId))
                    throw new Exception("No se encontró un usuario con ese correo electrónico.");

                var token = Guid.NewGuid();
                await CrearTokenRecuperacionAsync(usuarioId, token);
                await EnviarCorreoRecuperacionAsync(email, token);

                return "Se ha enviado un enlace de recuperación a su correo.";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> ResetPasswordAsync(UsuarioResetPasswordDTO dto)
        {
            try
            {
                var usuarioId = await ObtenerUsuarioIdPorTokenAsync(dto.Token);
                if (string.IsNullOrEmpty(usuarioId))
                    throw new Exception("El enlace de recuperación no es válido o ha expirado.");

                var nuevoHash = Encriptacion.Hashear(dto.NuevaPassword);

                using (var connection = accessData.GetConnection())
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "dbo.UpdatePassword";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id", SqlDbType.VarChar, 50).Value = usuarioId;
                    cmd.Parameters.Add("@PasswordNueva", SqlDbType.VarChar, 200).Value = nuevoHash;

                    await cmd.ExecuteNonQueryAsync();
                }

                await MarcarTokenComoUsadoAsync(dto.Token);
                await CleanInvalidTokensAsync();
                return "La contraseña ha sido restablecida correctamente.";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task ResetearIntentosLoginAsync(string email)
        {
            using var connection = accessData.GetConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "dbo.ResetearIntentosLogin";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Correo", email);

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task AumentarIntentoLoginAsync(string email)
        {
            using var connection = accessData.GetConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "dbo.AumentarIntentoLogin";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Correo", email);

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<string?> ObtenerUsuarioIdPorEmailAsync(string email)
        {
            using var connection = accessData.GetConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "dbo.GetUsuarioIdByCorreo";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = email;

            using var reader = await cmd.ExecuteReaderAsync();
            return reader.Read() ? reader["Id"]?.ToString() : null;
        }

        private async Task<UsuarioNotifyDTO?> ObtenerUsuarioNotifyPorEmailAsync(string email)
        {
            using var connection = accessData.GetConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "dbo.GetUsuarioNotifyByCorreo";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = email;

            using var reader = await cmd.ExecuteReaderAsync();
            if (reader.Read())
            {
                return new UsuarioNotifyDTO
                {
                    Documento = reader["Documento"]?.ToString() ?? string.Empty,
                    Nombre = reader["Nombre"]?.ToString() ?? string.Empty,
                    Email = reader["Correo"]?.ToString() ?? string.Empty
                };
            }
            else
            {
                return null;
            }
        }

        private async Task<IEnumerable<UsuarioNotifyDTO>> ObtenerUsuariosNotificadosPorCorreoEstudiantesAsync()
        {
            var usuarios = new List<UsuarioNotifyDTO>();

            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.GetUsuarioNotifyByCorreoEstudiantes";
                cmd.CommandType = CommandType.StoredProcedure;

                using var reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    usuarios.Add(new UsuarioNotifyDTO
                    {
                        Documento = reader["Documento"]?.ToString() ?? string.Empty,
                        Nombre = reader["Nombre"]?.ToString() ?? string.Empty,
                        Email = reader["Correo"]?.ToString() ?? string.Empty
                    });
                }

                return usuarios;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener usuarios notificados por correo de estudiantes: " + ex.Message);
            }
        }


        private async Task<string?> ObtenerUsuarioIdPorTokenAsync(Guid token)
        {
            using var connection = accessData.GetConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "dbo.ValidateRecoveryToken";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@Token", SqlDbType.UniqueIdentifier).Value = token;

            using var reader = await cmd.ExecuteReaderAsync();
            return reader.Read() ? reader["UsuarioId"]?.ToString() : null;
        }

        private async Task MarcarTokenComoUsadoAsync(Guid token)
        {
            using var connection = accessData.GetConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "dbo.MarkTokenAsUsed";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@Token", SqlDbType.UniqueIdentifier).Value = token;

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task CrearTokenRecuperacionAsync(string usuarioId, Guid token)
        {
            using var connection = accessData.GetConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "dbo.CreatePasswordRecovery";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@UsuarioId", SqlDbType.VarChar, 50).Value = usuarioId;
            cmd.Parameters.Add("@Token", SqlDbType.UniqueIdentifier).Value = token;

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task EnviarCorreoRecuperacionAsync(string email, Guid token)
        {
            // Obtener URL de recuperación
            var urlFrontRecovery = await _parametroRepos.GetItemAsync("URL.RECOVERY.PASSWORD");

            if (urlFrontRecovery == null || string.IsNullOrEmpty(urlFrontRecovery.ValorString))
                throw new InvalidOperationException($"La URL del panel de recuperación de usuario URL.RECOVERY.PASSWORD no existe.");

            string urlCompleta = $"{urlFrontRecovery.ValorString.TrimEnd('/')}/{token.ToString().ToUpper()}";


            // Obtener plantilla
            var plantilla = await _parametroRepos.GetHtmlByCodigoAsync("EMAIL.RECOVERY.PASSWORD");

            if (plantilla == null || string.IsNullOrEmpty(plantilla))
                throw new InvalidOperationException($"La plantilla HTML para recuperar password con código EMAIL.RECOVERY.PASSWORD no existe o no tiene contenido.");

            var HTML = plantilla!;

            // Reemplazar variables dinámicas
            var contenido = HTML
                .Replace("#Email#", email)
                .Replace("#URLSolicitud#", urlCompleta ?? "");

            // Obtener datos completos del usuario
            var usuarioNotify = await ObtenerUsuarioNotifyPorEmailAsync(email);
            if (usuarioNotify == null)
                throw new Exception($"No se encontró información de usuario para el email {email}.");

            // Aplicar fallback en caso de que algún campo sea null/vacío (por seguridad)
            usuarioNotify.Nombre = string.IsNullOrWhiteSpace(usuarioNotify.Nombre) ? "Usuario EducaMente" : usuarioNotify.Nombre;
            usuarioNotify.Documento = string.IsNullOrWhiteSpace(usuarioNotify.Documento) ? "N/D" : usuarioNotify.Documento;

            var Htmlbase64 = Base64Converter.EncodeBase64(contenido);

            var dto = new NotificacionContextDTO
            {
                Usuario = usuarioNotify,
                Asunto = "Recuperación de contraseña EducaMente",
                MensajeHtml = Htmlbase64
            };

            await notificacionManager.NotificarAlertaEmailAsync(dto);
        }

        private async Task CleanInvalidTokensAsync()
        {
            using var connection = accessData.GetConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "dbo.DeleteInvalidTokens";
            cmd.CommandType = CommandType.StoredProcedure;

            await cmd.ExecuteNonQueryAsync();
        }

        private UsuarioLogueadoDTO GenerarTokenJWT(UsuarioLogueadoDTO usuario)
        {
            //Cabecera
            var _symmetricSecurityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JWT:ClaveSecreta"])
                );
            var _signingCredentials = new SigningCredentials(
                _symmetricSecurityKey, SecurityAlgorithms.HmacSha256
                );
            var _header = new JwtHeader(_signingCredentials);

            //Claims
            var _Claims = new[]
            {
                new Claim("nombreUsuario", usuario.Nombre),
                new Claim("nombreCorreo", usuario.Email),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email)
            };

            //PayLoad
            var _Payload = new JwtPayload(
                    issuer: configuration["JWT:Issuer"],
                    audience: configuration["JWT:Audience"],
                    claims: _Claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddHours(12)
                );

            //Token
            var _Token = new JwtSecurityToken(
                    _header,
                    _Payload
                );

            usuario.Token = new JwtSecurityTokenHandler().WriteToken(_Token);

            return usuario;
        }
    }
}
