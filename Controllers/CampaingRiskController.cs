using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EducaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampaingRiskController : ControllerBase
    {
        private readonly I_CampainRisk campainRiskRepos;

        public CampaingRiskController(I_CampainRisk _campainRiskRepos)
        {
            campainRiskRepos = _campainRiskRepos;
        }

        [HttpPost("Start/{responsableId}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Iniciar campaña de bienestar",
            Description = "Crea una nueva campaña de bienestar y asocia automáticamente a todos los estudiantes elegibles."
        )]
        public async Task<ActionResult<string>> StartCampaingAsync(string responsableId)
        {
            if (string.IsNullOrWhiteSpace(responsableId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del responsable es obligatorio."
                });
            }

            string message;

            try
            {
                message = await campainRiskRepos.StartCampaingAsync(responsableId);
            }
            catch
            {
                throw; // tu middleware maneja errores SQL / THROW
            }

            return Ok(new { Message = message });
        }

        [HttpGet("List/{responsableId}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Listar campañas por responsable",
            Description = "Devuelve el listado de campañas creadas por un responsable, junto con su resumen."
        )]
        public async Task<ActionResult<IEnumerable<CampaingRiskResumenDTO>>> GetCampaingsByResponsableAsync(string responsableId)
        {
            if (string.IsNullOrWhiteSpace(responsableId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del responsable es obligatorio."
                });
            }

            IEnumerable<CampaingRiskResumenDTO> campañas;

            try
            {
                campañas = await campainRiskRepos.GetCampaingsByResponsableAsync(responsableId);
            }
            catch
            {
                throw;
            }

            if (campañas == null || !campañas.Any())
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "El responsable no tiene campañas registradas."
                });
            }

            return Ok(campañas);
        }

        [HttpGet("Detalle/{campaingId}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Obtener detalle de campaña",
            Description = "Devuelve la cabecera y el listado de estudiantes asociados a la campaña."
        )]
        public async Task<ActionResult<CampaingRiskDetalleDTO>> GetCampaingDetalleAsync(string campaingId)
        {
            if (string.IsNullOrWhiteSpace(campaingId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador de la campaña es obligatorio."
                });
            }

            CampaingRiskDetalleDTO? detalle;

            try
            {
                detalle = await campainRiskRepos.GetCampaingDetalleAsync(campaingId);
            }
            catch
            {
                throw;
            }

            if (detalle == null || detalle.Cabecera == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "No existe ninguna campaña con el identificador proporcionado."
                });
            }

            return Ok(detalle);
        }
    }
}
