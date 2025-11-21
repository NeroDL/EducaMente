using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EducaMente.Controllers
{
    [Route("PsicoPreguntaBank")]
    [ApiController]
    public class PsicoPreguntaBankController : ControllerBase
    {
        private readonly I_PsicoPreguntaBank psicoPreguntaBankRepos;

        public PsicoPreguntaBankController(I_PsicoPreguntaBank _psicoPreguntaBankRepos)
        {
            psicoPreguntaBankRepos = _psicoPreguntaBankRepos;
        }

        [HttpPost("Create")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Crear pregunta en el banco",
            Description = "Crea una nueva pregunta en PsicoPreguntaBank con su dimensión, escala Likert y fuente."
        )]
        public async Task<ActionResult<Response1StringDTO>> Create([FromBody] PsicoPreguntaBankAddDTO dto)
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

            var response = await psicoPreguntaBankRepos.AddAsync(dto);
            return Ok(response);
        }

        [HttpGet("Get/{id}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Obtener pregunta por Id",
            Description = "Devuelve una pregunta del banco PsicoPreguntaBank a partir de su identificador."
        )]
        public async Task<ActionResult<PsicoPreguntaBankModel>> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador de la pregunta es obligatorio."
                });
            }

            var pregunta = await psicoPreguntaBankRepos.GetByIdAsync(id);

            if (pregunta == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = $"No se encontró ninguna pregunta con el id: {id}."
                });
            }

            return Ok(pregunta);
        }

        [HttpGet("Get-All-LikerScale")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Listar escalas Likert",
            Description = "Devuelve todas las escalas Likert registradas en el sistema."
        )]
        public async Task<ActionResult<IEnumerable<LikertScaleDTO>>> GetAll()
        {
            var escalas = await psicoPreguntaBankRepos.GetAllAsync();

            if (escalas == null || !escalas.Any())
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "No existen escalas Likert registradas."
                });
            }

            return Ok(escalas);
        }

        [HttpGet("Search")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Buscar preguntas por texto",
            Description = "Busca preguntas del banco usando texto libre en el enunciado, dimensión, estado, fuente y ScaleId, con paginación."
        )]
        public async Task<ActionResult<IEnumerable<PsicoPreguntaBankModel>>> Search(
            [FromQuery] string? valorTxt,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            // Normalización básica por si te llega 0 o negativos
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var resultados = await psicoPreguntaBankRepos.SearchByTextAsync(
                valorTxt ?? string.Empty,
                pageNumber,
                pageSize
            );

            if (resultados == null || !resultados.Any())
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "No se encontraron preguntas que coincidan con los criterios de búsqueda."
                });
            }

            return Ok(resultados);
        }

        [HttpPut("Update")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Actualizar pregunta del banco",
            Description = "Actualiza parcialmente una pregunta del banco PsicoPreguntaBank. Solo se modifican los campos enviados."
        )]
        public async Task<ActionResult<object>> Update([FromBody] PsicoPreguntaBankUpdateDTO dto)
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

            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador de la pregunta es obligatorio."
                });
            }

            var message = await psicoPreguntaBankRepos.UpdateAsync(dto);

            return Ok(new { Message = message });
        }
    }
}
