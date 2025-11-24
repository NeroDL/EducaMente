using EducaMente.AccessData;
using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Utilities;
using System.Data;
using System.Data.SqlClient;

namespace EducaMente.Repositories
{
    public class OTPRepos : I_OTP
    {
        private readonly AccesoData _accessData;
        private readonly NotificationManager _notificationManager;
        private readonly I_Parametro _parametroRepos;

        public OTPRepos(AccesoData accessData, NotificationManager notificationManager, I_Parametro parametroRepos)
        {
            _accessData = accessData;
            _notificationManager = notificationManager;
            _parametroRepos = parametroRepos;
        }

        public async Task<bool> GenerarYEnviarOTPAsync(string usuarioId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuarioId))
                    throw new ArgumentException("UsuarioId es obligatorio.", nameof(usuarioId));

                // 1. Obtener datos del usuario (para saber a dónde enviar el OTP)
                var usuarioNotify = await GetUsuarioNotifyByIdAsync(usuarioId);
                if (usuarioNotify == null)
                {
                    // No existe el usuario, no se puede continuar
                    return false;
                }

                // 2. Generar token OTP de 4 dígitos
                var token = GenerarOtp();

                // 3. Crear/registrar OTP en BD (SP: CreateOTP)
                await CrearOtpAsync(usuarioId, token);

                // 4. Enviar el código por SMS/Email según datos disponibles
                await EnviarCodigoOTPAsync(usuarioNotify, token);

                return true;
            }
            catch (Exception ex)
            {
                // Sigues el patrón que manejas en tus repositorios
                throw new Exception(ex.Message);
            }
        }

        public async Task<OTPVerificationResult> VerificarOTPAsync(string usuarioId, int codigoOTP)
        {
            try
            {
                using var connection = _accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.ValidateOTP";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@UsuarioId", SqlDbType.VarChar, 50).Value = usuarioId;
                cmd.Parameters.Add("@Token", SqlDbType.Int).Value = codigoOTP;

                using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return new OTPVerificationResult
                    {
                        Success = false,
                        ErrorCode = "NO_RESPONSE",
                        ErrorMessage = "No se pudo validar el código OTP."
                    };
                }

                // Datos devueltos por el SP
                var idValue = reader["Id"];
                var mensaje = reader["Mensaje"]?.ToString() ?? "Sin mensaje del servidor.";

                bool success = idValue != DBNull.Value;

                if (success)
                {
                    await CleanExpiredOtpsAsync();

                    return new OTPVerificationResult
                    {
                        Success = true,
                        ErrorCode = string.Empty,
                        ErrorMessage = mensaje
                    };
                }
                else
                {
                    // OTP inválido — mensaje viene del SP
                    return new OTPVerificationResult
                    {
                        Success = false,
                        ErrorCode = "OTP_INVALID",
                        ErrorMessage = mensaje
                    };
                }
            }
            catch (Exception ex)
            {
                return new OTPVerificationResult
                {
                    Success = false,
                    ErrorCode = "EXCEPTION",
                    ErrorMessage = ex.Message
                };
            }
        }

        private async Task CrearOtpAsync(string usuarioId, int token)
        {
            using var connection = _accessData.GetConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "dbo.CreateOTP";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@UsuarioId", SqlDbType.VarChar, 50).Value = usuarioId;
            cmd.Parameters.Add("@Token", SqlDbType.Int).Value = token;

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task MarkOtpAsUsedAsync(Guid id)
        {
            using var connection = _accessData.GetConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "dbo.MarkOTPAsUsed";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task CleanExpiredOtpsAsync()
        {
            using var connection = _accessData.GetConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "dbo.DeleteExpiredOTPs";
            cmd.CommandType = CommandType.StoredProcedure;

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task EnviarCodigoOTPAsync(UsuarioNotifyDTO usuario, int token)
        {
            try
            {
                Console.WriteLine($"[OTP DEBUG] EnviarCodigoOTPAsync - Iniciando envío de notificaciones");

                var nombre = usuario.Nombre ?? "Usuario";
                var mensajeSMS = $"Hola {nombre}, tu código de verificación es {token}. Es válido por 5 minutos.";

                Console.WriteLine($"[OTP DEBUG] Construyendo mensaje de email...");
                var mensajeEmail = await ConstruirMensajeOTPEmail(nombre, token);
                Console.WriteLine($"[OTP DEBUG] Mensaje de email construido: {!string.IsNullOrEmpty(mensajeEmail)}");

                if (!string.IsNullOrWhiteSpace(usuario.Celular))
                {
                    Console.WriteLine($"[OTP DEBUG] Enviando SMS a celular: {usuario.Celular}");
                    var contextoSMS = new NotificacionContextDTO { Usuario = usuario, Asunto = "Código de Verificación", Mensaje = mensajeSMS };
                    await _notificationManager.NotificarAlertaSMSAsync(contextoSMS);
                    Console.WriteLine($"[OTP DEBUG] SMS enviado exitosamente");
                }
                else
                {
                    Console.WriteLine($"[OTP DEBUG] ADVERTENCIA: Usuario no tiene celular configurado");
                }

                if (!string.IsNullOrWhiteSpace(usuario.Email) && !string.IsNullOrWhiteSpace(mensajeEmail))
                {
                    Base64Converter.EncodeBase64(mensajeEmail);
                    Console.WriteLine($"[OTP DEBUG] Enviando Email a: {usuario.Email}");
                    var contextoEmail = new NotificacionContextDTO { Usuario = usuario, Asunto = "Código de Verificación", MensajeHtml = mensajeEmail };
                    await _notificationManager.NotificarAlertaEmailAsync(contextoEmail);
                    Console.WriteLine($"[OTP DEBUG] Email enviado exitosamente");
                }
                else
                {
                    Console.WriteLine($"[OTP DEBUG] ADVERTENCIA: No se puede enviar email. Email válido: {!string.IsNullOrWhiteSpace(usuario.Email)}, Mensaje válido: {!string.IsNullOrWhiteSpace(mensajeEmail)}");
                }

                Console.WriteLine($"[OTP DEBUG] EnviarCodigoOTPAsync completado exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OTP DEBUG] ERROR en EnviarCodigoOTPAsync: {ex.Message}");
                Console.WriteLine($"[OTP DEBUG] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        private int GenerarOtp()
        {
            var random = new Random();
            return random.Next(1000, 9999);
        }


        private async Task<string> ConstruirMensajeOTPEmail(string nombre, int token)
        {
            var cuerpoPlano = $"Hola {nombre}, tu código de verificación es {token}. Es válido por 5 minutos. No lo compartas con nadie.";
            var parametro = await _parametroRepos.GetHtmlByCodigoAsync("EMAIL.SEND.OTP");
            return parametro == null || string.IsNullOrWhiteSpace(parametro) ? cuerpoPlano : parametro.Replace("#Cuerpo#", cuerpoPlano);
        }

        private async Task<UsuarioNotifyDTO?> GetUsuarioNotifyByIdAsync(string usuarioId)
        {
            using var connection = _accessData.GetConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "dbo.GetUsuarioNotifyById";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@UsuarioId", SqlDbType.VarChar, 50).Value = usuarioId;

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new UsuarioNotifyDTO
            {
                Documento = reader["Documento"] == DBNull.Value? null : reader["Documento"]?.ToString(),
                Nombre = reader["Nombre"]?.ToString() ?? "Usuario EducaMente",
                Celular = reader["Celular"] == DBNull.Value ? null : reader["Celular"]?.ToString(),
                Email = reader["Correo"] == DBNull.Value ? null : reader["Correo"]?.ToString()
            };
        }
    }
}
