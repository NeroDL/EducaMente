using EducaMente.AccessData;
using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Utilities;
using System.Data;
using System.Globalization;

namespace EducaMente.Repositories
{
    public class CampaingRiskRepos : I_CampainRisk
    {
        private readonly AccesoData accessData;
        private readonly NotificationManager notificacionManager;
        private readonly IConfiguration configuration;
        private readonly I_Parametro _parametroRepos;

        public CampaingRiskRepos(AccesoData accessData, IConfiguration configuration, NotificationManager notificacionManager, I_Parametro parametroRepos)
        {
            this.accessData = accessData;
            this.configuration = configuration;
            this.notificacionManager = notificacionManager;
            _parametroRepos = parametroRepos;
        }

        public async Task<string> StartCampaingAsync(string responsableId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(responsableId))
                    throw new ArgumentException("ResponsableId es obligatorio.", nameof(responsableId));

                // 1. Traer estudiantes elegibles (SP: CampaingStudentGetElegibles)
                var estudiantesElegibles = (await GetEstudiantesElegiblesCampaniaAsync()).ToList();

                if (!estudiantesElegibles.Any())
                {
                    return "No hay estudiantes disponibles para realizar una campaña de correos";
                }

                // 2. Crear la campaña (SP: CampaingRiskCreate)
                var campaingId = await CrearCampaingRiskAsync(responsableId);

                // 3. Asociar cada estudiante a la campaña (SP: CampaingStudentAdd)
                foreach (var estudiante in estudiantesElegibles)
                {
                    await AgregarEstudianteACampaniaAsync(campaingId, estudiante.Id);

                    // 4. Enviar correo de invitación al test / campaña
                    await EnviarCorreoRecuperacionAsync(estudiante);
                }

                return "Se ha iniciado una campaña de bienestar";
            }
            catch (Exception ex)
            {
                // Sigues el patrón que ya usas en repositorios para que lo capture tu middleware
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<CampaingRiskResumenDTO>> GetCampaingsByResponsableAsync(string responsableId)
        {
            var campañas = new List<CampaingRiskResumenDTO>();

            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.CampaingRiskGetByResponsable";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@ResponsableId", SqlDbType.VarChar, 50).Value = responsableId;

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    campañas.Add(new CampaingRiskResumenDTO
                    {
                        Id = reader["Id"]?.ToString() ?? string.Empty,
                        ResponsableId = reader["ResponsableId"]?.ToString() ?? string.Empty,
                        NombreResponsable = reader["NombreResponsable"]?.ToString() ?? string.Empty,
                        FechaInicio = reader["FechaInicio"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaInicio"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO")),
                        FechaFin = reader["FechaFin"] == DBNull.Value ? string.Empty: Convert.ToDateTime(reader["FechaFin"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO")),
                        Estado = reader["Estado"]?.ToString() ?? string.Empty,
                        TotalEstudiantes = reader["TotalEstudiantes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalEstudiantes"]),
                        EstudiantesCompletados = reader["EstudiantesCompletados"] == DBNull.Value ? 0 : Convert.ToInt32(reader["EstudiantesCompletados"])
                    });
                }

                return campañas;
            }
            catch (Exception ex)
            {
                // Dejas que tu middleware lo maneje
                throw new Exception(ex.Message);
            }
        }

        public async Task<CampaingRiskDetalleDTO?> GetCampaingDetalleAsync(string campaingId)
        {
            var detalle = new CampaingRiskDetalleDTO();

            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.CampaingRiskGetDetalle";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@CampaingId", SqlDbType.VarChar, 50).Value = campaingId;

                using var reader = await cmd.ExecuteReaderAsync();

                // RS1: Cabecera
                if (await reader.ReadAsync())
                {
                    detalle.Cabecera = new CampaingRiskCabeceraDTO
                    {
                        Id = reader["Id"]?.ToString() ?? string.Empty,
                        ResponsableId = reader["ResponsableId"]?.ToString() ?? string.Empty,
                        NombreResponsable = reader["NombreResponsable"]?.ToString() ?? string.Empty,
                        FechaInicio = reader["FechaInicio"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaInicio"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO")),
                        FechaFin = reader["FechaFin"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaFin"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO")),
                        Estado = reader["Estado"]?.ToString() ?? string.Empty,
                        TotalEstudiantes = reader["TotalEstudiantes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalEstudiantes"]),
                        EstudiantesCompletados = reader["EstudiantesCompletados"] == DBNull.Value ? 0 : Convert.ToInt32(reader["EstudiantesCompletados"])
                    };
                }

                if (detalle == null)
                    return null;

                // Pasar al segundo resultset (RS2: estudiantes)
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        detalle.Estudiantes.Add(new CampaingRiskEstudianteDTO
                        {
                            CampaingStudentId = reader["CampaingStudentId"]?.ToString() ?? string.Empty,
                            CampaingId = reader["CampaingId"]?.ToString() ?? string.Empty,
                            EstudianteId = reader["EstudianteId"]?.ToString() ?? string.Empty,
                            NombreEstudiante = reader["NombreEstudiante"]?.ToString() ?? string.Empty,
                            CorreoEstudiante = reader["CorreoEstudiante"]?.ToString() ?? string.Empty,
                            Estado = reader["Estado"]?.ToString() ?? string.Empty,
                            UltimoUsuarioTestId = reader["UltimoUsuarioTestId"] == DBNull.Value ? null : reader["UltimoUsuarioTestId"]?.ToString(),
                            FechaUltimoTest = reader["FechaUltimoTest"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaUltimoTest"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO")),
                        });
                    }
                }

                return detalle;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<string> CrearCampaingRiskAsync(string responsableId)
        {
            using var connection = accessData.GetConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "dbo.CampaingRiskCreate";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@ResponsableId", SqlDbType.VarChar, 50).Value = responsableId;
            cmd.Parameters.Add("@FechaInicio", SqlDbType.DateTime).Value = UtilidadesTiempo.ObtenerFechaColombia();

            var campaingIdParam = cmd.Parameters.Add("@NuevoCampaingId", SqlDbType.VarChar, 50);
            campaingIdParam.Direction = ParameterDirection.Output;

            await cmd.ExecuteNonQueryAsync();

            return campaingIdParam.Value?.ToString() ?? string.Empty;
        }

        private async Task<IEnumerable<CampaingStudentElegibleDTO>> GetEstudiantesElegiblesCampaniaAsync()
        {
            var estudiantes = new List<CampaingStudentElegibleDTO>();

            using var connection = accessData.GetConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "dbo.CampaingStudentGetElegibles";
            cmd.CommandType = CommandType.StoredProcedure;

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                estudiantes.Add(new CampaingStudentElegibleDTO
                {
                    Id = reader["Id"]?.ToString() ?? string.Empty,
                    Documento = reader["Documento"]?.ToString() ?? string.Empty,
                    Nombre = reader["Nombre"]?.ToString() ?? string.Empty,
                    Correo = reader["Correo"]?.ToString() ?? string.Empty,
                    FirstTestDone = reader["FirstTestDone"] != DBNull.Value && Convert.ToBoolean(reader["FirstTestDone"])
                });
            }

            return estudiantes;
        }

        private async Task<string> AgregarEstudianteACampaniaAsync(string campaingId, string estudianteId)
        {
            using var connection = accessData.GetConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "dbo.CampaingStudentAdd";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@CampaingId", SqlDbType.VarChar, 50).Value = campaingId;
            cmd.Parameters.Add("@EstudianteId", SqlDbType.VarChar, 50).Value = estudianteId;

            var nuevoIdParam = cmd.Parameters.Add("@NuevoCampaingStudentId", SqlDbType.VarChar, 50);
            nuevoIdParam.Direction = ParameterDirection.Output;

            await cmd.ExecuteNonQueryAsync();

            return nuevoIdParam.Value?.ToString() ?? string.Empty;
        }

        private async Task EnviarCorreoRecuperacionAsync(CampaingStudentElegibleDTO estudiante)
        {
            // Obtener URL de recuperación
            var url = await _parametroRepos.GetItemAsync("URL.START.TEST");

            if (url == null || string.IsNullOrEmpty(url.ValorString))
                throw new InvalidOperationException($"La URL del panel de recuperación de usuario URL.START.TEST no existe.");

            // Obtener plantilla
            var plantilla = await _parametroRepos.GetHtmlByCodigoAsync("EMAIL.CAMPAING.BIENESTAR");

            if (plantilla == null || string.IsNullOrEmpty(plantilla))
                throw new InvalidOperationException($"La plantilla HTML para recuperar password con código EMAIL.CAMPAING.BIENESTAR no existe o no tiene contenido.");

            var HTML = plantilla!;

            // Reemplazar variables dinámicas
            var contenido = HTML
                .Replace("#NombreEstudiante#", estudiante.Nombre)
                .Replace("#URLSolicitud#", url.ValorString);

            var usuarioNotify = new UsuarioNotifyDTO
            {
                Documento = estudiante.Documento,
                Nombre = estudiante.Nombre,
                Email = estudiante.Correo,
                Celular = null
            };

            var Htmlbase64 = Base64Converter.EncodeBase64(contenido);

            var dto = new NotificacionContextDTO
            {
                Usuario = usuarioNotify,
                Asunto = "Campaña de bienestar institucional",
                MensajeHtml = Htmlbase64
            };

            await notificacionManager.NotificarAlertaEmailAsync(dto);
        }
    }
}
