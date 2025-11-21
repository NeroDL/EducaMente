using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EducaMente.Controllers
{
    [Route("PsicoTest")]
    [ApiController]
    public class PsicoTestController : ControllerBase
    {
        private readonly I_PsicoTest psicoTestRepos;
        private readonly I_FinalizeTestOrquestador finalizeTestOrquestadorService;

        public PsicoTestController(I_PsicoTest _psicoTestReposs, I_FinalizeTestOrquestador _finalizeTestOrquestadorService)
        {
            psicoTestRepos = _psicoTestReposs;
            finalizeTestOrquestadorService = _finalizeTestOrquestadorService;
        }

        [HttpPost("CreateUniversal")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Crear test universal",
            Description = "Crea un nuevo test psicológico de alcance universal."
        )]
        public async Task<ActionResult<Response1StringDTO>> CrearUniversal([FromBody] PsicoTestUniversalAddDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_BODY",
                    Message = "Se requiere el cuerpo de la solicitud."
                });
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Datos de entrada inválidos.",
                    Details = errores
                });
            }

            var response = await psicoTestRepos.CrearTestUniversalAsync(dto);
            return Ok(response);
        }

        [HttpPost("CreatePersonalizado")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Crear test personalizado",
            Description = "Crea un nuevo test psicológico personalizado para un usuario específico."
        )]
        public async Task<ActionResult<Response1StringDTO>> CrearPersonalizado([FromBody] PsicoTestPersonalizadoAddDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_BODY",
                    Message = "Se requiere el cuerpo de la solicitud."
                });
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Datos de entrada inválidos.",
                    Details = errores
                });
            }

            if (string.IsNullOrWhiteSpace(dto.TargetUsuarioId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del usuario objetivo (TargetUsuarioId) es obligatorio para crear un test personalizado."
                });
            }

            var response = await psicoTestRepos.CrearTestPersonalizadoAsync(dto);
            return Ok(response);
        }

        [HttpPut("CambiarEstado")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Cambiar estado de un test",
            Description = "Actualiza el estado de un test psicológico existente (por ejemplo: Borrador, Activo, Inactivo)."
        )]
        public async Task<ActionResult<string>> CambiarEstado([FromBody] PsicoTestCambiarEstadoDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_BODY",
                    Message = "Se requiere el cuerpo de la solicitud."
                });
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Datos de entrada inválidos.",
                    Details = errores
                });
            }

            if (string.IsNullOrWhiteSpace(dto.TestId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del test (TestId) es obligatorio."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.NuevoEstado))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El nuevo estado del test es obligatorio."
                });
            }

            var response = await psicoTestRepos.CambiarEstadoAsync(dto);
            return Ok(response);
        }

        [HttpPost("CrearPreguntaYAgregarATest")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Crear una nueva pregunta y agregarla a un Test",
            Description = "Crea una nueva pregunta en el banco de preguntas y la asocia inmediatamente a un Test."
        )]
        public async Task<ActionResult<Response1StringDTO>> CrearPreguntaYAgregarATest([FromBody] PsicoPreguntaBankAddToTestDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_BODY",
                    Message = "Se requiere el cuerpo de la solicitud."
                });
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Datos de entrada inválidos.",
                    Details = errores
                });
            }

            if (string.IsNullOrWhiteSpace(dto.TestId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del Test (TestId) es obligatorio."
                });
            }

            var response = await psicoTestRepos.CrearPreguntaYAgregarATestAsync(dto);
            return Ok(response);
        }

        [HttpPost("AddPregunta")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Agregar pregunta a un test",
            Description = "Asocia una pregunta del banco al test indicado."
        )]
        public async Task<ActionResult<Response1StringDTO>> AgregarPreguntaATest([FromBody] PsicoTestPreguntaAddDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_BODY",
                    Message = "Se requiere el cuerpo de la solicitud."
                });
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Datos de entrada inválidos.",
                    Details = errores
                });
            }

            if (string.IsNullOrWhiteSpace(dto.TestId) || string.IsNullOrWhiteSpace(dto.PreguntaId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "TestId y PreguntaId son obligatorios para asociar una pregunta al test."
                });
            }

            var response = await psicoTestRepos.AgregarPreguntaATestAsync(dto);
            return Ok(response);
        }

        [HttpGet("Get-All")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Listar tests psicológicos",
            Description = "Obtiene todos los tests registrados, tanto universales como personalizados."
        )]
        public async Task<ActionResult<IEnumerable<PsicoTestListDTO>>> GetAllTests()
        {
            IEnumerable<PsicoTestListDTO> tests;

            try
            {
                tests = await psicoTestRepos.GetAllAsync();
            }
            catch (Exception)
            {
                throw;
            }

            if (tests == null || !tests.Any())
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "No existen tests psicológicos registrados."
                });
            }

            return Ok(tests);
        }

        [HttpGet("GetDetalleTest/{testId}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Obtener detalle de un test",
            Description = "Devuelve la cabecera del test y el listado de preguntas asociadas."
        )]
        public async Task<ActionResult<PsicoTestDetalleDTO>> ObtenerDetalle(string testId)
        {
            if (string.IsNullOrWhiteSpace(testId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del test (testId) es obligatorio."
                });
            }

            var detalle = await psicoTestRepos.ObtenerDetalleTestAsync(testId);

            if (detalle == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = $"No se encontró ningún test con el id: {testId}."
                });
            }

            return Ok(detalle);
        }

        [HttpGet("GetDetalleConRespuestas/{usuarioTestId}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Obtener intento de test con respuestas",
            Description = "Devuelve la cabecera del intento de test, datos del usuario, datos del test, preguntas, opciones y cuáles fueron seleccionadas."
        )]
        public async Task<ActionResult<UsuarioTestConRespuestasDTO>> GetDetalleConRespuestas(string usuarioTestId)
        {
            if (string.IsNullOrWhiteSpace(usuarioTestId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del intento de test (UsuarioTestId) es obligatorio."
                });
            }

            var detalle = await psicoTestRepos.ObtenerConRespuestasAsync(usuarioTestId);

            if (detalle == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = $"No se encontró ningún intento de test con el id: {usuarioTestId}."
                });
            }

            return Ok(detalle);
        }

        [HttpPost("Start-Test/{usuarioId}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Iniciar intento de test",
            Description = "Inicia un intento de test para un usuario y devuelve el detalle completo del test materializado (preguntas y opciones)."
        )]
        public async Task<ActionResult<UsuarioTestDetalleDTO>> Start(string usuarioId)
        {
            if (usuarioId == null)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_BODY",
                    Message = "Se requiere el cuerpo de la solicitud."
                });
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Datos de entrada inválidos.",
                    Details = errores
                });
            }

            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "UsuarioId obligatorio para iniciar un test."
                });
            }

            var detalle = await psicoTestRepos.UsuarioTestStartAsync(usuarioId);

            return Ok(detalle);
        }

        [HttpPost("SaveAnswer")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Guardar respuesta de una pregunta",
            Description = "Registra o actualiza la respuesta de un ítem del test para el intento en curso."
        )]
        public async Task<ActionResult<Response1StringDTO>> GuardarRespuesta([FromBody] UsuarioRespuestaSaveDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_BODY",
                    Message = "Se requiere el cuerpo de la solicitud."
                });
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Datos de entrada inválidos.",
                    Details = errores
                });
            }

            if (string.IsNullOrWhiteSpace(dto.UsuarioTestItemId) || string.IsNullOrWhiteSpace(dto.OpcionId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "UsuarioTestItemId y OpcionId son obligatorios para registrar la respuesta."
                });
            }

            var response = await psicoTestRepos.GuardarRespuestaAsync(dto);
            return Ok(response);
        }

        [HttpPost("FinalizeTest")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Finalizar intento de test",
            Description = "Finaliza el intento de test, calcula los puntajes por dimensión, determina el nivel de riesgo y actualiza el perfil psicológico del usuario."
        )]
        public async Task<ActionResult<UsuarioTestResultadoDTO>> Finalizar([FromBody] UsuarioTestFinalizadoDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_BODY",
                    Message = "Se requiere el cuerpo de la solicitud."
                });
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Datos de entrada inválidos.",
                    Details = errores
                });
            }

            if (string.IsNullOrWhiteSpace(dto.UsuarioTestId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del intento de test (UsuarioTestId) es obligatorio."
                });
            }

            var resultado = await finalizeTestOrquestadorService.EndTestAsync(dto);
            return Ok(resultado);
        }
    }
}
