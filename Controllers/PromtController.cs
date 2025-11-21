using EducaMente.Interface;
using EducaMente.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EducaMente.Controllers
{
    [Route("Promt")]
    [ApiController]
    public class PromtController : ControllerBase
    {
        private readonly I_Promt promtRepos;

        public PromtController(I_Promt _promtRepos)
        {
            promtRepos = _promtRepos;
        }

        [HttpPost("Create")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Crear promt",
            Description = "Permite registrar un nuevo promt en el sistema."
        )]
        public async Task<IActionResult> AddAsync(PromtModel promt)
        {
            if (promt == null)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El cuerpo del promt no puede estar vacío."
                });
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Datos inválidos para crear el promt.",
                    Details = errores
                });
            }

            var message = await promtRepos.AddAsync(promt);
            return Ok(new { Message = message });
        }

        [HttpGet("Search")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Buscar Promt",
            Description = "Permite visualizar los promts del sistema, puedes buscar por codigo, descripción y por promt"
        )]
        public async Task<IActionResult> SearchByText([FromQuery] string? valorTxt, int pageNumber = 1, int pageSize = 10)
        {

            var result = await promtRepos.SearchByTextAsync(valorTxt, pageNumber, pageSize);
            if (result == null || !result.Any())
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = $"No se encontraron promts con el texto '{valorTxt}'."
                });
            }
            return Ok(result);
        }

        [HttpPut("Update")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Actualizar promt",
            Description = "Permite actualizar los datos de un promt existente en el sistema."
        )]
        public async Task<IActionResult> Update(PromtModel promt)
        {
            if (promt == null)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El cuerpo del promt no puede estar vacío."
                });
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Datos inválidos para actualizar el promt.",
                    Details = errores
                });
            }

            var message = await promtRepos.UpdateAsync(promt);
            return Ok(new { Message = message });
        }
    }
}
