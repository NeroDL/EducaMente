using EducaMente.Domain;
using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Models;
using EducaMente.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EducaMente.Controllers
{
    [Route("WebService")]
    [ApiController]
    public class WebServiceController : ControllerBase
    {
        private readonly I_WebService webserviceRepos;

        public WebServiceController(I_WebService _webserviceRepos)
        {
            webserviceRepos = _webserviceRepos;
        }

        [HttpPost("Crear")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Crear servicio web",
            Description = "Este es un endpoint de funcionamiento lógico interno para configurar la conexión con el microservicio"
        )]
        public async Task<IActionResult> AddAsync([FromBody] WebServiceAddDTO webserviceDTO)
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

            var encryptedKey = Encriptacion.Encriptar(webserviceDTO.ApiKey);
            var dtoToSave = new WebServiceAddDTO
            {
                EntidadId = webserviceDTO.EntidadId,
                ApiKey = encryptedKey,
                TipoEnvio = webserviceDTO.TipoEnvio,
                Tipo = webserviceDTO.Tipo,
                Servicio = webserviceDTO.Servicio,
                Descripcion = webserviceDTO.Descripcion
            };

            var response = await webserviceRepos.AddAsync(dtoToSave);
            return Ok(new { Message = response });
        }

        [HttpGet("ObtenerItemWebService/{id}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Obtener servicio web",
            Description = "Este es un endpoint de funcionamiento lógico interno para configurar la conexión con el microservicio"
        )]
        public async Task<ActionResult<WebServiceModel>> GetItem(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del servicio web invalido"
                });
            }

            var webservice = await webserviceRepos.GetItemAsync(id);

            if (webservice == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = $"No se encontró un Servicio web con el id: {id}"
                });
            }

            return Ok(webservice);
        }

        [HttpGet("ObtenerPorTipo/{tipo}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Obtener servicio web por tipo",
            Description = "Obtiene la configuración de un servicio web según su tipo (Amazon, Hablame, Gemini, etc.)"
        )]
        public async Task<ActionResult<WebServiceModel>> GetByTipo(TipoWebService tipo)
        {
            var webservice = await webserviceRepos.GetByTipoAsync(tipo);

            if (webservice == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = $"No se encontró configuración para el servicio web de tipo: {tipo}"
                });
            }

            return Ok(webservice);
        }

        [HttpPut("Actualizar")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Actualizar servicio web",
            Description = "Actualiza la configuración de un servicio web existente (ApiKey se almacena cifrada)."
        )]
        public async Task<IActionResult> UpdateAsync([FromBody] WebServiceUpdateDTO webserviceDTO)
        {
            // Validación básica del body y del modelo
            if (webserviceDTO == null)
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

            if (webserviceDTO.Id <= 0)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del servicio web es inválido."
                });
            }

            if (string.IsNullOrWhiteSpace(webserviceDTO.ApiKey))
            {
                // El SP requiere @ApiKey (no admite NULL).
                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "ApiKey es obligatorio para actualizar el servicio web."
                });
            }

            // Cifrar ApiKey antes de persistir
            var encryptedKey = Encriptacion.Encriptar(webserviceDTO.ApiKey);

            var dtoToSave = new WebServiceUpdateDTO
            {
                Id = webserviceDTO.Id,
                EntidadId = webserviceDTO.EntidadId,
                ApiKey = encryptedKey,
                TipoEnvio = webserviceDTO.TipoEnvio,
                Tipo = webserviceDTO.Tipo,
                Servicio = webserviceDTO.Servicio,
                Descripcion = webserviceDTO.Descripcion,
                ModeloPorDefecto = webserviceDTO.ModeloPorDefecto,
                EndpointBase = webserviceDTO.EndpointBase,
                MaxTokens = webserviceDTO.MaxTokens,
                Temperature = webserviceDTO.Temperature
            };

            var response = await webserviceRepos.UpdateAsync(dtoToSave);

            return Ok(new
            {
                Mensaje = response
            });
        }
    }
}
