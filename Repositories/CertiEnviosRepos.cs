using EducaMente.Domain;
using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Utilities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EducaMente.Repositories
{
    public class CertiEnviosRepos : I_CertiEnvios
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly I_WebService webserviceRepos;

        public CertiEnviosRepos(IHttpClientFactory httpClientFactory, I_WebService webserviceRepos)
        {
            _httpClientFactory = httpClientFactory;
            this.webserviceRepos = webserviceRepos;
        }
        public async Task<EnvioResultDTO> EnviarNotificacionAsync(UsuarioNotifyDTO usuario, string mensajeTexto, string asunto)
        {
            // Obtener configuración desde la base de datos
            var config = await webserviceRepos.GetByTipoAsync(TipoWebService.Hablame);

            if (config is null)
            {
                return new EnvioResultDTO
                {
                    Estado = "Error",
                    Mensaje = "No se encontró configuración válida para el servicio web.",
                    MensajeTexto = mensajeTexto,
                    JsonCompleto = "{}"
                };
            }

            // Desencriptar si fuese necesario en producción
            var entidadId = config.EntidadId ?? 0;
            var password = Encriptacion.DesencriptarSiEsBase64(config.ApiKey);
            var tipoEnvioId = config.TipoEnvio ?? 0;

            // Crear el payload
            var payload = new SendSMSRequestDTO
            {
                EntidadId = entidadId,
                Password = password,
                TipoEnvioId = tipoEnvioId,
                Tercero = usuario,
                Asunto = asunto,
                Mensaje = mensajeTexto,
                SmsUnitTipo = "Prioritario",
                Flash = 0,
                Certificado = 1
            };

            var client = _httpClientFactory.CreateClient("CertiEnviosClient");
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("/EnvioUnitario/Enviar", content);
            }
            catch (Exception ex)
            {
                return new EnvioResultDTO
                {
                    Estado = "Error",
                    Mensaje = $"Fallo de conexión con CertiEnvios: {ex.Message}",
                    MensajeTexto = mensajeTexto,
                    JsonCompleto = "{}"
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new EnvioResultDTO
                {
                    Estado = "Error",
                    Mensaje = $"HTTP {response.StatusCode}: {response.ReasonPhrase}",
                    MensajeTexto = mensajeTexto,
                    JsonCompleto = jsonResponse
                };
            }

            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var detalle = doc.RootElement.GetProperty("Detalle").EnumerateArray().FirstOrDefault();

                var estado = detalle.GetProperty("Estado").GetString();
                var observacion = detalle.TryGetProperty("Observacion", out var obs) ? obs.GetString() : null;

                return new EnvioResultDTO
                {
                    Estado = estado ?? "Desconocido",
                    Mensaje = observacion ?? "Respuesta recibida.",
                    MensajeTexto = mensajeTexto,
                    JsonCompleto = jsonResponse
                };
            }
            catch (Exception ex)
            {
                return new EnvioResultDTO
                {
                    Estado = "Error",
                    Mensaje = $"Error al interpretar la respuesta: {ex.Message}",
                    MensajeTexto = mensajeTexto,
                    JsonCompleto = jsonResponse
                };
            }
        }

        public async Task<EnvioResultDTO> EnviarEmailAsync(UsuarioNotifyDTO usuario, string asunto, string cuerpo)
        {
            // Obtener configuración desde la base de datos
            var config = await webserviceRepos.GetByTipoAsync(TipoWebService.Amazon);

            if (config is null)
            {
                return new EnvioResultDTO
                {
                    Estado = "Error",
                    Mensaje = "No se encontró configuración válida para el servicio web.",
                    MensajeTexto = cuerpo,
                    JsonCompleto = "{}"
                };
            }

            var entidadId = config.EntidadId ?? 0;
            var password = Encriptacion.DesencriptarSiEsBase64(config.ApiKey);
            var tipoEnvioId = config.TipoEnvio ?? 0;

            // Crear el payload para email
            var payload = new SendEmailRequestDTO
            {
                EntidadId = entidadId,
                Password = password,
                TipoEnvioId = tipoEnvioId,
                Tercero = usuario,
                Asunto = asunto,
                Cuerpo = cuerpo
            };

            var client = _httpClientFactory.CreateClient("CertiEnviosClient");
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("/EnvioEmail/Create", content);
            }
            catch (Exception ex)
            {
                return new EnvioResultDTO
                {
                    Estado = "Error",
                    Mensaje = $"Fallo de conexión con CertiEnvios: {ex.Message}",
                    MensajeTexto = cuerpo,
                    JsonCompleto = "{}"
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new EnvioResultDTO
                {
                    Estado = "Error",
                    Mensaje = $"HTTP {response.StatusCode}: {response.ReasonPhrase}",
                    MensajeTexto = cuerpo,
                    JsonCompleto = jsonResponse
                };
            }

            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var detalle = doc.RootElement.GetProperty("Detalle").EnumerateArray().FirstOrDefault();

                var estado = detalle.GetProperty("Estado").GetString();
                var observacion = detalle.TryGetProperty("Observacion", out var obs) ? obs.GetString() : null;

                return new EnvioResultDTO
                {
                    Estado = estado ?? "Desconocido",
                    Mensaje = observacion ?? "Respuesta recibida.",
                    MensajeTexto = cuerpo,
                    JsonCompleto = jsonResponse
                };
            }
            catch (Exception ex)
            {
                return new EnvioResultDTO
                {
                    Estado = "Error",
                    Mensaje = $"Error al interpretar la respuesta: {ex.Message}",
                    MensajeTexto = cuerpo,
                    JsonCompleto = jsonResponse
                };
            }
        }

        public async Task<EnvioResultDTO> EnviarEmailConAdjuntosAsync(UsuarioNotifyDTO usuario, string asunto, string cuerpo, List<IFormFile> adjuntos)
        {
            // Obtener configuración desde la base de datos
            var config = await webserviceRepos.GetByTipoAsync(TipoWebService.Amazon);

            if (config is null)
            {
                return new EnvioResultDTO
                {
                    Estado = "Error",
                    Mensaje = "No se encontró configuración válida para el servicio web.",
                    MensajeTexto = cuerpo,
                    JsonCompleto = "{}"
                };
            }

            var entidadId = config.EntidadId ?? 0;
            var password = Encriptacion.DesencriptarSiEsBase64(config.ApiKey);
            var tipoEnvioId = config.TipoEnvio ?? 0;

            // (seguimos creando 'payload' si lo usas en otros lados; no lo enviamos como JSON en multipart)
            var payload = new SendEmailRequestDTO
            {
                EntidadId = entidadId,
                Password = password,
                TipoEnvioId = tipoEnvioId,
                Tercero = usuario,
                Asunto = asunto,
                Cuerpo = cuerpo
            };

            var client = _httpClientFactory.CreateClient("CertiEnviosClient");
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var content = new MultipartFormDataContent();

            // === Campos aplanados que exige /EnvioEmail/CreateWithAttachments ===
            content.Add(new StringContent(entidadId.ToString()), "EntidadId");
            content.Add(new StringContent(tipoEnvioId.ToString()), "TipoEnvioId");
            content.Add(new StringContent(password ?? string.Empty), "Password");

            content.Add(new StringContent(usuario?.Documento ?? string.Empty), "Tercero.Documento");
            content.Add(new StringContent(usuario?.Nombre ?? string.Empty), "Tercero.Nombre");
            content.Add(new StringContent(usuario?.Celular ?? string.Empty), "Tercero.Celular");
            content.Add(new StringContent(usuario?.Email ?? string.Empty), "Tercero.Email");

            content.Add(new StringContent(asunto ?? string.Empty, Encoding.UTF8), "Asunto");
            content.Add(new StringContent(cuerpo ?? string.Empty, Encoding.UTF8), "Cuerpo");

            // === Adjuntos ===
            foreach (var file in adjuntos)
            {
                if (file == null || file.Length == 0) continue;

                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "Adjuntos", file.FileName);
            }

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("/EnvioEmail/CreateWithAttachments", content);
            }
            catch (Exception ex)
            {
                return new EnvioResultDTO
                {
                    Estado = "Error",
                    Mensaje = $"Fallo de conexión con CertiEnvios: {ex.Message}",
                    MensajeTexto = cuerpo,
                    JsonCompleto = "{}"
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new EnvioResultDTO
                {
                    Estado = "Error",
                    Mensaje = $"HTTP {response.StatusCode}: {response.ReasonPhrase}",
                    MensajeTexto = cuerpo,
                    JsonCompleto = jsonResponse
                };
            }

            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var detalle = doc.RootElement.GetProperty("Detalle").EnumerateArray().FirstOrDefault();

                var estado = detalle.GetProperty("Estado").GetString();
                var observacion = detalle.TryGetProperty("Observacion", out var obs) ? obs.GetString() : null;

                return new EnvioResultDTO
                {
                    Estado = estado ?? "Desconocido",
                    Mensaje = observacion ?? "Respuesta recibida.",
                    MensajeTexto = cuerpo,
                    JsonCompleto = jsonResponse
                };
            }
            catch (Exception ex)
            {
                return new EnvioResultDTO
                {
                    Estado = "Error",
                    Mensaje = $"Error al interpretar la respuesta: {ex.Message}",
                    MensajeTexto = cuerpo,
                    JsonCompleto = jsonResponse
                };
            }
        }
    }
}
