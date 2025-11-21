using EducaMente.Domain;
using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Repositories;
using EducaMente.Utilities;
using System.Text;
using System.Text.Json;

namespace EducaMente.Services
{
    public class GeminiService : I_GeminiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly I_WebService _webServiceRepos;
        private readonly I_Promt _promtRepos;
        private readonly I_Usuario _usuarioRepos;
        private readonly I_PsicoPreguntaBank _psicoPreguntaBankRepos;
        private readonly I_PsicoTest _psicoTestRepos;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(
            IHttpClientFactory httpClientFactory,
            I_WebService webServiceRepos,
            I_Promt promtRepos,
            I_Usuario usuarioRepos,
            I_PsicoPreguntaBank psicoPreguntaBankRepos,
            I_PsicoTest psicoTestRepos,
            ILogger<GeminiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _webServiceRepos = webServiceRepos;
            _promtRepos = promtRepos;
            _usuarioRepos = usuarioRepos;
            _psicoPreguntaBankRepos = psicoPreguntaBankRepos;
            _psicoTestRepos = psicoTestRepos;
            _logger = logger;
        }

        public async Task<GeminiAnalisisPerfilResponseDTO> GenerarObservacionPerfilDesdeTestAsync(string usuarioTestId)
        {
            _logger.LogInformation(
                $"Iniciando orquestación de análisis de perfil para UsuarioTestId: {usuarioTestId}"
            );

            if (string.IsNullOrWhiteSpace(usuarioTestId))
            {
                _logger.LogWarning("UsuarioTestId nulo o vacío en GenerarObservacionPerfilDesdeTestAsync.");
                return new GeminiAnalisisPerfilResponseDTO
                {
                    Exito = false,
                    Observacion = string.Empty,
                    Mensaje = "El UsuarioTestId es obligatorio.",
                    JsonCompleto = "{}"
                };
            }

            try
            {
                // -------------------------------------------------------
                // 1) Obtener detalle del intento de test con respuestas
                // -------------------------------------------------------
                var testDetalle = await _psicoTestRepos.ObtenerConRespuestasAsync(usuarioTestId);

                if (testDetalle is null)
                {
                    _logger.LogWarning(
                        $"No se encontró detalle de test para UsuarioTest: {usuarioTestId}"
                    );

                    return new GeminiAnalisisPerfilResponseDTO
                    {
                        Exito = false,
                        Observacion = string.Empty,
                        Mensaje = $"No se encontró información del test para el UsuarioTest {usuarioTestId}.",
                        JsonCompleto = "{}"
                    };
                }

                var usuarioId = testDetalle.UsuarioId;

                // -------------------------------------------------------
                // 2) Obtener perfil psicológico actual del estudiante
                // -------------------------------------------------------
                PerfilPsicoActualDTO? perfilActual = null;

                try
                {
                    perfilActual = await _usuarioRepos.GetPerfilPsicoActualAsync(usuarioId);
                }
                catch (KeyNotFoundException knfEx)
                {
                    _logger.LogWarning(
                        knfEx,
                        $"El usuario {testDetalle.NombreUsuario} no tiene perfil psicológico actual.",
                        usuarioId
                    );
                }

                if (perfilActual is null)
                {
                    return new GeminiAnalisisPerfilResponseDTO
                    {
                        Exito = false,
                        Observacion = string.Empty,
                        Mensaje = $"El usuario {testDetalle.NombreUsuario} no tiene un perfil psicológico actual registrado.",
                        JsonCompleto = "{}"
                    };
                }

                // -------------------------------------------------------
                // 3) Obtener el Prompt base desde la BD
                // -------------------------------------------------------
                var promptBase = await _promtRepos.GetPromptByCodigoAsync("PROMT.ANALISIS.PERFIL.PSICO");

                if (string.IsNullOrWhiteSpace(promptBase))
                {
                    _logger.LogWarning(
                        "El prompt con código PROMT.ANALISIS.PERFIL.PSICO está vacío o no se obtuvo correctamente."
                    );

                    return new GeminiAnalisisPerfilResponseDTO
                    {
                        Exito = false,
                        Observacion = string.Empty,
                        Mensaje = "No se pudo cargar el prompt base para el análisis del perfil.",
                        JsonCompleto = "{}"
                    };
                }

                // -------------------------------------------------------
                // 4) Serializar Perfil + Test a JSON
                // -------------------------------------------------------
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = false
                };

                var perfilJson = JsonSerializer.Serialize(perfilActual, jsonOptions);
                var testJson = JsonSerializer.Serialize(testDetalle, jsonOptions);

                // -------------------------------------------------------
                // 5) Construir DTO para Gemini
                // -------------------------------------------------------
                var requestGemini = new GeminiAnalisisPerfilRequestDTO
                {
                    PromptBase = promptBase,
                    PerfilJSON = perfilJson,
                    TestJSON = testJson
                };

                // -------------------------------------------------------
                // 6) Invocar a Gemini para generar la observación (con reintentos)
                // -------------------------------------------------------
                const int maxReintentos = 3;
                int intentoActual = 0;
                GeminiAnalisisPerfilResponseDTO? respuestaGemini = null;

                while (intentoActual < maxReintentos)
                {
                    intentoActual++;

                    try
                    {
                        _logger.LogInformation(
                            $"Intento {intentoActual}/{maxReintentos} para generar observación con Gemini. UsuarioTestId: {usuarioTestId}"
                        );

                        respuestaGemini = await GenerarObservacionPerfilAsync(requestGemini);

                        // Si la respuesta es válida -> salimos del ciclo
                        if (respuestaGemini.Exito && !string.IsNullOrWhiteSpace(respuestaGemini.Observacion))
                            break;

                        // Si hubo error lógico, NO reintentar (por ejemplo prompt malo)
                        if (!respuestaGemini.Exito &&
                            respuestaGemini.Mensaje.Contains("prompt", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning("Error no recuperable. No se harán más reintentos.");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            $"Error en intento {intentoActual} al llamar a Gemini. UsuarioTestId: {usuarioTestId}"
                        );
                    }

                    // Si aún hay intentos disponibles -> esperar antes de reintentar
                    if (intentoActual < maxReintentos)
                    {
                        int delayMs = 1500 * intentoActual; // backoff exponencial
                        _logger.LogInformation($"Reintentando en {delayMs} ms...");
                        await Task.Delay(delayMs);
                    }
                }

                // Si sigue siendo inválida después de los reintentos -> devolver error
                if (respuestaGemini == null || !respuestaGemini.Exito || string.IsNullOrWhiteSpace(respuestaGemini.Observacion))
                {
                    _logger.LogWarning(
                        $"Gemini no pudo generar una observación válida después de {maxReintentos} intentos."
                    );

                    return respuestaGemini ?? new GeminiAnalisisPerfilResponseDTO
                    {
                        Exito = false,
                        Observacion = string.Empty,
                        Mensaje = "No se obtuvo una observación válida después de varios intentos.",
                        JsonCompleto = "{}"
                    };
                }

                // -------------------------------------------------------
                // 7) Guardar la observación en PerfilPsico y PerfilPsicoHist
                // -------------------------------------------------------
                var updateDto = new UpdateObservacionTestUsuarioDTO
                {
                    UsuarioTestId = usuarioTestId,
                    Observacion = respuestaGemini.Observacion.Trim()
                };

                await _psicoTestRepos.SetObservacionPerfilAsync(updateDto);

                _logger.LogInformation(
                    $"Observación generada y almacenada correctamente para UsuarioTestId: {usuarioTestId}"
                );

                return respuestaGemini;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Error inesperado en GenerarObservacionPerfilDesdeTestAsync para UsuarioTestId: {usuarioTestId}"
                );

                return new GeminiAnalisisPerfilResponseDTO
                {
                    Exito = false,
                    Observacion = string.Empty,
                    Mensaje = $"Error inesperado al generar la observación: {ex.Message}",
                    JsonCompleto = "{}"
                };
            }
        }

        public async Task<GeminiGenerarTestServiceResponseDTO> GenerarTestPersonalizadoDesdeTestAsync(string usuarioTestId)
        {
            _logger.LogInformation("Iniciando generación de Test Personalizado desde UsuarioTestId: {UsuarioTestId}", usuarioTestId);

            if (string.IsNullOrWhiteSpace(usuarioTestId))
            {
                return new GeminiGenerarTestServiceResponseDTO
                {
                    Exito = false,
                    Mensaje = "El UsuarioTestId es obligatorio.",
                    Contenido = null
                };
            }

            try
            {
                // -------------------------------------------------------
                // 1) Obtener el intento de test con preguntas y respuestas
                // -------------------------------------------------------
                var detalleTest = await _psicoTestRepos.ObtenerConRespuestasAsync(usuarioTestId);

                if (detalleTest == null)
                {
                    _logger.LogWarning("No se encontró detalle de test para UsuarioTestId {UsuarioTestId}", usuarioTestId);

                    return new GeminiGenerarTestServiceResponseDTO
                    {
                        Exito = false,
                        Mensaje = $"No se encontró el intento de test con Id {usuarioTestId}.",
                        Contenido = null
                    };
                }

                var usuarioId = detalleTest.UsuarioId;
                _logger.LogInformation("Usuario asociado al test: {UsuarioId}", usuarioId);

                // -------------------------------------------------------
                // 2) Obtener el perfil psicológico actual del estudiante
                // -------------------------------------------------------
                var perfil = await _usuarioRepos.GetPerfilPsicoActualAsync(usuarioId);

                if (perfil == null)
                {
                    _logger.LogWarning("El usuario {UsuarioId} no tiene perfil psicológico actual.", usuarioId);

                    return new GeminiGenerarTestServiceResponseDTO
                    {
                        Exito = false,
                        Mensaje = $"El usuario {usuarioId} no tiene un perfil psicológico actual registrado.",
                        Contenido = null
                    };
                }

                // -------------------------------------------------------
                // 3) Obtener el banco de preguntas (PsicoPreguntaBank)
                // -------------------------------------------------------
                var bancoPreguntas = await _psicoPreguntaBankRepos.SearchByTextAsync(
                    string.Empty, // sin filtro → SP recibe NULL/'' y retorna todo
                    pageNumber: 1,
                    pageSize: 500
                );

                var bancoFiltrado = bancoPreguntas
                    .Where(p => string.Equals(p.Estado, "Activo", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!bancoFiltrado.Any())
                {
                    _logger.LogWarning("No se encontraron preguntas activas en PsicoPreguntaBank.");
                    // Igual continuamos, Gemini puede proponer nuevas.
                }

                // -------------------------------------------------------
                // 4) Serializar objetos a JSON (perfil, test, banco)
                // -------------------------------------------------------
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };

                string perfilJson = JsonSerializer.Serialize(perfil, jsonOptions);
                string testJson = JsonSerializer.Serialize(detalleTest, jsonOptions);
                string bancoJson = JsonSerializer.Serialize(bancoFiltrado, jsonOptions);

                // -------------------------------------------------------
                // 5) Obtener el PROMT base desde la tabla de promts
                // -------------------------------------------------------
                const string codigoPromt = "PROMT.GENERACION.TEST.PERSONALIZADO";

                string promptBase;
                try
                {
                    promptBase = await _promtRepos.GetPromptByCodigoAsync(codigoPromt);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener el promt con código {CodigoPromt}", codigoPromt);

                    return new GeminiGenerarTestServiceResponseDTO
                    {
                        Exito = false,
                        Mensaje = $"No se pudo obtener el promt '{codigoPromt}' desde base de datos.",
                        Contenido = null
                    };
                }

                if (string.IsNullOrWhiteSpace(promptBase))
                {
                    _logger.LogWarning("El promt con código {CodigoPromt} está vacío.", codigoPromt);

                    return new GeminiGenerarTestServiceResponseDTO
                    {
                        Exito = false,
                        Mensaje = $"El promt '{codigoPromt}' está vacío o no es válido.",
                        Contenido = null
                    };
                }

                // -------------------------------------------------------
                // 6) Construir el prompt final (reemplazar placeholders)
                // -------------------------------------------------------
                string promptFinal = promptBase
                    .Replace("{{PERFIL_PSICO_JSON}}", perfilJson)
                    .Replace("{{TEST_CON_RESPUESTAS_JSON}}", testJson)
                    .Replace("{{PREGUNTAS_BANCO_JSON}}", bancoJson);

                _logger.LogDebug("PromptFinal para test personalizado generado. Longitud: {Length} caracteres.",
                    promptFinal.Length);

                // -------------------------------------------------------
                // 7) Configuración de Gemini (WebService -> ApiKey, Endpoint, Modelo)
                // -------------------------------------------------------
                var config = await _webServiceRepos.GetByTipoAsync(TipoWebService.Gemini);

                if (config == null || string.IsNullOrWhiteSpace(config.ApiKey))
                {
                    _logger.LogWarning("No se encontró configuración válida para Gemini (o ApiKey vacía).");

                    return new GeminiGenerarTestServiceResponseDTO
                    {
                        Exito = false,
                        Mensaje = "No se encontró configuración válida para el servicio Gemini.",
                        Contenido = null
                    };
                }

                var apiKey = Encriptacion.DesencriptarSiEsBase64(config.ApiKey);
                var endpointBase = config.EndpointBase ?? "https://generativelanguage.googleapis.com/v1beta/models/";
                var modelName = config.ModeloPorDefecto ?? "gemini-2.5-pro";

                // -------------------------------------------------------
                // 8) Llamar a Gemini para generar el test personalizado (CON REINTENTOS)
                // -------------------------------------------------------
                const int maxReintentos = 3;
                int intentoActual = 0;
                GenerarTestResponseDTO? contenido = null;

                while (intentoActual < maxReintentos)
                {
                    intentoActual++;

                    try
                    {
                        _logger.LogInformation(
                            "Intento {IntentoActual}/{MaxReintentos} al llamar a Gemini para generar test personalizado. UsuarioTestId: {UsuarioTestId}",
                            intentoActual, maxReintentos, usuarioTestId
                        );

                        contenido = await _EnviarPromptGeminiTestAsync(
                            endpointBase,
                            modelName,
                            apiKey,
                            promptFinal
                        );

                        if (contenido != null)
                        {
                            // Respuesta válida -> salimos del bucle
                            break;
                        }

                        _logger.LogWarning(
                            "Gemini devolvió un contenido nulo en el intento {IntentoActual}.",
                            intentoActual
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Error en el intento {IntentoActual} al llamar a Gemini para generar test personalizado. UsuarioTestId: {UsuarioTestId}",
                            intentoActual,
                            usuarioTestId
                        );
                    }

                    if (intentoActual < maxReintentos)
                    {
                        int delayMs = 1500 * intentoActual; // backoff lineal/exponencial suave
                        _logger.LogInformation(
                            "Reintentando llamada a Gemini en {DelayMs} ms. (Intento {IntentoActual}/{MaxReintentos})",
                            delayMs, intentoActual, maxReintentos
                        );
                        await Task.Delay(delayMs);
                    }
                }

                // Si después de los reintentos no hay contenido → devolvemos error
                if (contenido == null)
                {
                    _logger.LogError(
                        "No fue posible generar el test personalizado con Gemini después de {MaxReintentos} intentos. UsuarioTestId: {UsuarioTestId}",
                        maxReintentos,
                        usuarioTestId
                    );

                    return new GeminiGenerarTestServiceResponseDTO
                    {
                        Exito = false,
                        Mensaje = "No fue posible generar el test personalizado después de varios intentos.",
                        Contenido = null,
                        ModeloUsado = modelName,
                        RawResponse = null
                    };
                }

                // Si llegamos aquí, tenemos un contenido estructurado desde Gemini
                _logger.LogInformation("Gemini generó sugerencias de test personalizado correctamente.");

                return new GeminiGenerarTestServiceResponseDTO
                {
                    Exito = true,
                    Mensaje = "Sugerencias de test personalizado generadas correctamente.",
                    Contenido = contenido,
                    ModeloUsado = modelName,
                    RawResponse = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en GenerarTestPersonalizadoDesdeTestAsync.");

                return new GeminiGenerarTestServiceResponseDTO
                {
                    Exito = false,
                    Mensaje = $"Error inesperado: {ex.Message}",
                    Contenido = null
                };
            }
        }

        private async Task<GeminiAnalisisPerfilResponseDTO> GenerarObservacionPerfilAsync(GeminiAnalisisPerfilRequestDTO dto)
        {
            _logger.LogInformation("Iniciando generación de observación de perfil con Gemini.");

            // -------------------------------------------------------
            // 1) Obtener configuración del servicio desde BD
            // -------------------------------------------------------
            var config = await _webServiceRepos.GetByTipoAsync(TipoWebService.Gemini);

            if (config == null || string.IsNullOrWhiteSpace(config.ApiKey))
            {
                _logger.LogWarning("No se encontró configuración válida para Gemini o ApiKey está vacía.");

                return new GeminiAnalisisPerfilResponseDTO
                {
                    Exito = false,
                    Observacion = string.Empty,
                    Mensaje = "No se encontró configuración válida para el servicio Gemini.",
                    JsonCompleto = "{}"
                };
            }

            // -------------------------------------------------------
            // 2) Desencriptar API Key
            // -------------------------------------------------------
            var apiKey = Encriptacion.DesencriptarSiEsBase64(config.ApiKey);

            // -------------------------------------------------------
            // 3) Parámetros esenciales
            // -------------------------------------------------------
            var endpointBase = config.EndpointBase ?? string.Empty;
            var modelName = config.ModeloPorDefecto ?? "gemini-2.5-pro";

            // -------------------------------------------------------
            // 4) Construir prompt final (template + JSONs)
            // -------------------------------------------------------
            if (string.IsNullOrWhiteSpace(dto.PromptBase))
            {
                _logger.LogWarning("El PromptBase recibido en el DTO es nulo o vacío.");

                return new GeminiAnalisisPerfilResponseDTO
                {
                    Exito = false,
                    Observacion = string.Empty,
                    Mensaje = "El PromptBase no puede ser nulo o vacío.",
                    JsonCompleto = "{}"
                };
            }

            string perfilJson = dto.PerfilJSON ?? "null";
            string testJson = dto.TestJSON ?? "null";

            string promptFinal = dto.PromptBase
                .Replace("{{PERFIL_PSICO_JSON}}", perfilJson)
                .Replace("{{TEST_CON_RESPUESTAS_JSON}}", testJson);

            _logger.LogDebug(
                "PromptFinal generado para Gemini. Longitud: {Length} caracteres.",
                promptFinal.Length
            );

            // -------------------------------------------------------
            // 5) Llamar al método privado que hace la petición HTTP
            // -------------------------------------------------------
            string observacion;

            try
            {
                observacion = await _EnviarPromptGeminiAsync(
                    endpointBase,
                    modelName,
                    apiKey,
                    promptFinal
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llamar a la API de Gemini.");

                return new GeminiAnalisisPerfilResponseDTO
                {
                    Exito = false,
                    Observacion = string.Empty,
                    Mensaje = $"Error al llamar a Gemini: {ex.Message}",
                    JsonCompleto = "{}",
                    ModeloUsado = modelName
                };
            }

            _logger.LogInformation("Observación de perfil generada correctamente con Gemini.");

            // -------------------------------------------------------
            // 6) Respuesta final exitosa
            // -------------------------------------------------------
            return new GeminiAnalisisPerfilResponseDTO
            {
                Exito = true,
                Observacion = observacion.Trim(),
                Mensaje = "Observación generada con éxito.",
                JsonCompleto = "{}",   // cuando adaptemos _EnviarPromptGeminiAsync, aquí puedes guardar el JSON crudo
                ModeloUsado = modelName
            };
        }

        private async Task<GenerarTestResponseDTO> _EnviarPromptGeminiTestAsync(string endpointBase, string modelName, string apiKey, string promptFinal)
        {
            if (string.IsNullOrWhiteSpace(endpointBase))
                throw new ArgumentException("EndpointBase no puede ser nulo o vacío.");

            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("ModelName no puede ser nulo o vacío.");

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("ApiKey no puede ser nulo o vacío.");

            if (string.IsNullOrWhiteSpace(promptFinal))
                throw new ArgumentException("El promptFinal no puede ser nulo o vacío.");

            // Construir URL final
            string url = $"{endpointBase}{modelName}:generateContent?key={apiKey}";

            // Construir el cuerpo para Gemini
            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = promptFinal }
                        }
                    }
                }
            };

            using var client = new HttpClient();

            client.Timeout = TimeSpan.FromSeconds(60);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };

            _logger.LogInformation("Enviando prompt a Gemini para generar test personalizado.");

            var response = await client.SendAsync(request);

            var rawJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini devolvió error HTTP {Status}: {Content}", response.StatusCode, rawJson);
                throw new Exception($"Gemini error HTTP {response.StatusCode}: {rawJson}");
            }

            _logger.LogDebug("Respuesta cruda de Gemini recibida para test personalizado: {Raw}", rawJson);

            // ------------------------
            // EXTRAER SOLO EL JSON INTERNO
            // ------------------------

            string? contenidoJson = _ExtraerTextoDeGemini(rawJson);

            if (string.IsNullOrWhiteSpace(contenidoJson))
            {
                throw new Exception("Gemini no devolvió ningún contenido válido para test personalizado.");
            }

            // ------------------------
            // PARSEAR EL CONTENIDO JSON
            // ------------------------
            return _ParseGenerarTestResponse(contenidoJson);
        }

        private async Task<string> _EnviarPromptGeminiAsync(string endpointBase, string modelName, string apiKey, string promptFinal)
        {
            if (string.IsNullOrWhiteSpace(endpointBase))
                throw new ArgumentException("EndpointBase no puede ser nulo o vacío.");

            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("ModelName no puede ser nulo o vacío.");

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("ApiKey no puede ser nulo o vacío.");

            if (string.IsNullOrWhiteSpace(promptFinal))
                throw new ArgumentException("El promptFinal no puede ser nulo o vacío.");

            // Construcción del URL final de la API
            string url = $"{endpointBase}{modelName}:generateContent?key={apiKey}";

            // Construcción del cuerpo del request según el API de Gemini
            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = promptFinal }
                        }
                    }
                }
            };

            using var client = new HttpClient();

            client.Timeout = TimeSpan.FromSeconds(60);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al llamar a Gemini: {response.StatusCode} → {errorText}");
            }

            var rawJson = await response.Content.ReadAsStringAsync();

            // Extraer el texto generado (Gemini responde con: candidates[0].content.parts[].text)
            string? text = _ExtraerTextoDeGemini(rawJson);

            if (string.IsNullOrWhiteSpace(text))
                throw new Exception("La respuesta de Gemini no contiene texto generado.");

            return text;
        }

        private GenerarTestResponseDTO _ParseGenerarTestResponse(string jsonContenido)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonContenido))
                {
                    throw new ArgumentException("El contenido JSON es nulo o vacío.");
                }

                // Limpiar la respuesta de Gemini de caracteres especiales y bloques de código
                jsonContenido = LimpiarRespuestaGemini(jsonContenido);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Para manejar caracteres especiales.
                };

                // Intentamos deserializar el JSON
                GenerarTestResponseDTO result;
                try
                {
                    result = JsonSerializer.Deserialize<GenerarTestResponseDTO>(jsonContenido, options);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Error al intentar deserializar el JSON.");
                    throw new Exception("El formato JSON recibido no es válido.", jsonEx);
                }

                // Validamos que el resultado no sea nulo
                if (result == null)
                {
                    throw new Exception("Deserialización devolvió un objeto nulo.");
                }

                // Aseguramos listas no nulas
                result.UsarExistentes ??= new List<GenerarTestPreguntaExistenteDTO>();
                result.CrearNuevas ??= new List<GenerarTestPreguntaNuevaDTO>();

                _logger.LogInformation(
                    "GenerarTestResponseDTO parseado correctamente. UsarExistentes: {CountExistentes}, CrearNuevas: {CountNuevas}",
                    result.UsarExistentes.Count,
                    result.CrearNuevas.Count
                );

                return result;
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "Argumento inválido al parsear JSON.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al deserializar el JSON de test generado por Gemini.");
                throw new Exception("Error al deserializar el JSON de test generado por Gemini.", ex);
            }
        }

        private string LimpiarRespuestaGemini(string respuesta)
        {
            if (string.IsNullOrWhiteSpace(respuesta))
                return string.Empty;

            // Elimina bloques de código tipo markdown
            respuesta = respuesta.Trim();
            if (respuesta.StartsWith("```json"))
                respuesta = respuesta.Substring(7); // quita ```json (7 caracteres)
            if (respuesta.StartsWith("```"))
                respuesta = respuesta.Substring(3); // quita ``` (3 caracteres)
            if (respuesta.EndsWith("```"))
                respuesta = respuesta.Substring(0, respuesta.Length - 3);

            // Asegúrate de quitar también saltos de línea extra
            return respuesta.Trim();
        }

        private string? _ExtraerTextoDeGemini(string rawJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(rawJson);

                var root = doc.RootElement;

                // candidates -> content -> parts -> text
                var candidates = root.GetProperty("candidates");

                var content = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return content;
            }
            catch
            {
                return null;
            }
        }
    }
}
