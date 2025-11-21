using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Utilities;

namespace EducaMente.Services
{
    public class FinalizeTestOrquestadorService : I_FinalizeTestOrquestador
    {
        private readonly I_GeminiService _geminiService;
        private readonly I_PsicoTest _psicoTestRepos;
        private readonly ILogger<FinalizeTestOrquestadorService> _logger;

        public FinalizeTestOrquestadorService(
            I_GeminiService geminiService,
            I_PsicoTest psicoTestRepos,
            ILogger<FinalizeTestOrquestadorService> logger)
        {
            _geminiService = geminiService;
            _psicoTestRepos = psicoTestRepos;
            _logger = logger;
        }

        public async Task<UsuarioTestResultadoDTO> EndTestAsync(UsuarioTestFinalizadoDTO dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                if (string.IsNullOrWhiteSpace(dto.UsuarioTestId))
                    throw new ArgumentException("El UsuarioTestId es obligatorio.", nameof(dto.UsuarioTestId));

                _logger.LogInformation("Iniciando finalización de test. UsuarioTestId: {UsuarioTestId}",dto.UsuarioTestId);

                // 1) Finalizar el test (SP UsuarioTestFinalize) y obtener los scores inmediatamente
                var resultado = await _psicoTestRepos.FinalizarAsync(dto.UsuarioTestId);

                _logger.LogInformation("Test finalizado correctamente. UsuarioTestId: {UsuarioTestId}, NivelRiesgo: {NivelRiesgo}, EstadoEmocional: {EstadoEmocional}",dto.UsuarioTestId, resultado.NivelRiesgo, resultado.EstadoEmocional);

                // 2) Lanzar procesos de IA EN SEGUNDO PLANO (no bloquean la respuesta al usuario)
                _ = Task.Run(async () =>
                {
                    // 2.1) Generar observación del perfil con Gemini
                    try
                    {
                        // Iniciando la generación
                        _logger.LogInformation("[Gemini - Observación] Iniciando generación de observación para UsuarioTestId: {UsuarioTestId}", dto.UsuarioTestId);

                        // Generación de observación (llamada a Gemini)
                        var respuestaGemini = await _geminiService.GenerarObservacionPerfilDesdeTestAsync(dto.UsuarioTestId);

                        // Si Gemini responde exitosamente, logea el éxito
                        if (respuestaGemini.Exito)
                        {
                            _logger.LogInformation("[Gemini - Observación] Observación generada con éxito para UsuarioTestId: {UsuarioTestId}. Observación: {Observacion}", dto.UsuarioTestId, respuestaGemini.Observacion);
                        }
                        else
                        {
                            _logger.LogWarning("[Gemini - Observación] Gemini no generó observación válida para UsuarioTestId: {UsuarioTestId}. Mensaje: {Mensaje}", dto.UsuarioTestId, respuestaGemini.Mensaje);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Si algo falla en el proceso
                        _logger.LogError(ex, "[Gemini - Observación] Error al generar la observación para UsuarioTestId: {UsuarioTestId}", dto.UsuarioTestId);
                    }

                    // 2.2) Generar test personalizado para el estudiante
                    try
                    {
                        _logger.LogInformation("[Gemini - TestPersonalizado] Iniciando generación de test personalizado para UsuarioTestId: {UsuarioTestId}",dto.UsuarioTestId);

                        // a) Obtener detalle del intento para saber quién es el usuario
                        var detalleTest = await _psicoTestRepos.ObtenerConRespuestasAsync(dto.UsuarioTestId);
                        if (detalleTest == null)
                        {
                            _logger.LogWarning("[Gemini - TestPersonalizado] No se pudo obtener el detalle del test para UsuarioTestId: {UsuarioTestId}",dto.UsuarioTestId);
                            return;
                        }

                        var usuarioId = detalleTest.UsuarioId;

                        // b) Crear el Test personalizado para ese estudiante
                        var crearTestResp = await _psicoTestRepos.CrearTestPersonalizadoAsync(
                            new PsicoTestPersonalizadoAddDTO
                            {
                                TargetUsuarioId = usuarioId,
                                Descripcion = $"Test personalizado generado por IA el {UtilidadesTiempo.ObtenerFechaColombia():dd/MM/yy HH:mm} para el estudiante {detalleTest.NombreUsuario}"
                            });

                        var testIdPersonalizado = crearTestResp.Id;
                        if (string.IsNullOrWhiteSpace(testIdPersonalizado))
                        {
                            _logger.LogWarning("[Gemini - TestPersonalizado] El SP de creación de test no devolvió un Id válido. UsuarioTestId: {UsuarioTestId}", dto.UsuarioTestId);
                            return;
                        }

                        // c) Pedir a Gemini el contenido del test (arrays de preguntas)
                        var respGeminiTest = await _geminiService.GenerarTestPersonalizadoDesdeTestAsync(dto.UsuarioTestId);

                        if (respGeminiTest == null || !respGeminiTest.Exito || respGeminiTest.Contenido == null)
                        {
                            _logger.LogWarning("[Gemini - TestPersonalizado] Gemini no devolvió contenido válido para el test. UsuarioTestId: {UsuarioTestId}",dto.UsuarioTestId);
                            return;
                        }

                        var contenido = respGeminiTest.Contenido;

                        // d) Agregar preguntas existentes al test personalizado
                        foreach (var existente in contenido.UsarExistentes)
                        {
                            await _psicoTestRepos.AgregarPreguntaATestAsync(new PsicoTestPreguntaAddDTO
                            {
                                TestId = testIdPersonalizado,
                                PreguntaId = existente.PreguntaId
                            });
                        }

                        // e) Crear nuevas preguntas (Fuente = IA) y agregarlas al test personalizado
                        foreach (var nueva in contenido.CrearNuevas)
                        {
                            await _psicoTestRepos.CrearPreguntaYAgregarATestByIAAsync(
                                new PsicoPreguntaBankAddToTestDTO
                                {
                                    TestId = testIdPersonalizado,
                                    Texto = nueva.Texto,
                                    Dimension = nueva.Dimension,
                                    ScaleId = nueva.ScaleId
                                });
                        }

                        _logger.LogInformation("[Gemini - TestPersonalizado] Test personalizado {TestIdPersonalizado} generado para UsuarioId: {UsuarioId} a partir de UsuarioTestId: {UsuarioTestId}",testIdPersonalizado, usuarioId, dto.UsuarioTestId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,"[Gemini - TestPersonalizado] Error al generar test personalizado para UsuarioTestId: {UsuarioTestId}",dto.UsuarioTestId);
                    }
                });

                // 3) Retornar de inmediato el resultado numérico del test al front
                _logger.LogInformation("EndTestAsync completado. Devolviendo resultado para UsuarioTestId: {UsuarioTestId}",dto.UsuarioTestId);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error en EndTestAsync para UsuarioTestId: {UsuarioTestId}", dto?.UsuarioTestId);

                throw new Exception(ex.Message);
            }
        }
    }
}
