using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EducaMente.Controllers
{
    [Route("TipoUsuario")]
    [ApiController]
    public class TipoUsuarioController : ControllerBase
    {
        private readonly I_TipoUsuario tipoUsuarioRepos;
        public TipoUsuarioController(I_TipoUsuario _tipoUsuarioRepos)
        {

            tipoUsuarioRepos = _tipoUsuarioRepos;

        }

        [HttpPost("Crear")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Crear tipos de usuario",
            Description = "Este es un endpoint de funcionamiento lógico interno para configurar los tipos de usuario que puede tener la aplicación"
        )]
        public async Task<IActionResult> AddAsync(TipoUsuarioAddDTO tipoUsuario)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Datos de entrada inválidos.",
                    Details = errores
                });
            }

            var mensaje = await tipoUsuarioRepos.AddAsync(tipoUsuario);
            return Ok(new { Message = mensaje });
        }

        [HttpGet("ObtenerAllTipoUsuario")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Visualizar los tipos de usuario",
            Description = "Despliega la lista de tipos de usuario existentes en el sistema"
        )]
        public async Task<ActionResult<IEnumerable<TipoUsuarioDTO>>> GetAll()
        {
            var tipos_usuario = await tipoUsuarioRepos.GetAllAsync();
            if (tipos_usuario == null || !tipos_usuario.Any())
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "No se encontró ningún tipo de usuario."
                });
            }

            return Ok(tipos_usuario);
        }

        [HttpGet("ObtenerItemTipoUsuario/{id}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Visualizar tipo de usuario",
            Description = "Permite consultar un Tipo de usuario por medio de su Id"
        )]
        public async Task<ActionResult<TipoUsuarioDTO>> GetItem(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del tipo de usuario es obligatorio."
                });
            }

            var tipoUsuario = await tipoUsuarioRepos.GetItemAsync(id);
            if (tipoUsuario == null || string.IsNullOrEmpty(tipoUsuario.Id))
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = $"No se encontró ningún tipo de usuario con el id: {id}."
                });
            }

            return Ok(tipoUsuario);
        }

        [HttpPut("ActualizarItemTipoUsuario")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Actualizar tipo de usuario",
            Description = "Permite actualizar la información de un tipo de usuario"
        )]
        public async Task<ActionResult<string>> Update(TipoUsuarioDTO tipoUsuarioDTO)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Datos de entrada inválidos.",
                    Details = errores
                });
            }

            var mensaje = await tipoUsuarioRepos.UpdateAsync(tipoUsuarioDTO);
            return Ok(new { Message = mensaje });
        }
    }
}
