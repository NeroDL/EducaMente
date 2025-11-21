using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EducaMente.Controllers
{
    [Route("Parametro")]
    [ApiController]
    public class ParametroController : ControllerBase
    {
        private readonly I_Parametro parametroRepos;

        public ParametroController(I_Parametro _parametroRepos)
        {
            parametroRepos = _parametroRepos;
        }

        [HttpPost("Create")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Crear parámetro",
            Description = "Permite registrar un nuevo parámetro en el sistema a partir del formulario."
        )]
        public async Task<IActionResult> AddAsync(ParametroAddFormDTO parametro)
        {
            if (parametro == null)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El cuerpo del parámetro no puede estar vacío."
                });
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Datos inválidos para crear el parámetro.",
                    Details = errores
                });
            }

            var message = await parametroRepos.AddFromFormAsync(parametro);
            return Ok(new { Message = message });
        }

        [HttpGet("Get")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Listar todos los parámetros",
            Description = "Despliega todos los parámetros existentes registrados en el sistema."
        )]
        public async Task<ActionResult<IEnumerable<ParametroAddDTO>>> GetAll()
        {
            var parametros = await parametroRepos.GetAllAsync();

            if (parametros == null || !parametros.Any())
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "No se encontró ningún parámetro registrado."
                });
            }

            return Ok(parametros);
        }

        [HttpGet("Get/{codigo}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Obtener parámetro por código",
            Description = "Permite consultar un parámetro específico usando su código único."
        )]
        public async Task<ActionResult<ParametroAddDTO>> GetItem(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El parámetro 'codigo' es obligatorio."
                });
            }

            var parametro = await parametroRepos.GetItemAsync(codigo);

            if (parametro == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = $"No se encontró el parámetro con el código: {codigo}."
                });
            }

            return Ok(parametro);
        }

        [HttpPut("Update")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Actualizar parámetro",
            Description = "Permite actualizar los datos de un parámetro existente en el sistema."
        )]
        public async Task<IActionResult> Update(ParametroAddFormDTO parametro)
        {
            if (parametro == null)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El cuerpo del parámetro no puede estar vacío."
                });
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Datos inválidos para actualizar el parámetro.",
                    Details = errores
                });
            }

            var message = await parametroRepos.UpdateAsync(parametro);
            return Ok(new { Message = message });
        }

        [HttpGet("GetDominiosAll")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Listar dominios de base de datos",
            Description = "Consulta todos los dominios disponibles en la base de datos asociados a restricciones CHECK."
        )]
        public async Task<ActionResult<List<CheckConstraintDTO>>> GetCheckConstraints()
        {
            var constraints = await parametroRepos.GetDominioAll();

            if (constraints == null || !constraints.Any())
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "No se encontraron dominios definidos en la base de datos."
                });
            }

            return Ok(constraints);
        }

        [HttpGet("GetDominioByNombre/{constraintName}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Consultar valores de un dominio",
            Description = "Retorna los valores permitidos por una restricción CHECK específica en la base de datos."
        )]
        public async Task<ActionResult<List<string>>> GetCheckConstraintValues(string constraintName)
        {
            if (string.IsNullOrWhiteSpace(constraintName))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El nombre de la restricción es obligatorio."
                });
            }

            var valores = await parametroRepos.GetDominioByNombreAsync(constraintName);

            if (!valores.Any())
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = $"No se encontraron valores para el CHECK CONSTRAINT: {constraintName}."
                });
            }

            return Ok(valores);
        }

        /// <summary>
        /// Buscar parámetros por texto (código, descripción, tipo o valor).
        /// </summary>
        [HttpGet("Search")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Consultar parametros por medio de diferentes filtros",
            Description = "Retorna los parametros filtrados por diferentes parametros de busqueda"
        )]
        public async Task<IActionResult> SearchByText([FromQuery] string? valorTxt, int pageNumber = 1, int pageSize = 10)
        {

            var result = await parametroRepos.SearchByTextAsync(valorTxt, pageNumber, pageSize);
            if (result == null || !result.Any())
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = $"No se encontraron parámetros con el texto '{valorTxt}'."
                });
            }
            return Ok(result);
        }
    }
}
