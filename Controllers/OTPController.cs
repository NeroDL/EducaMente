using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EducaMente.Controllers
{
    [Route("OTP")]
    [ApiController]
    public class OTPController : ControllerBase
    {
        private readonly I_OTP otpRepos;
        public OTPController(I_OTP _otpRepos)
        {

            otpRepos = _otpRepos;

        }

        [HttpPost("Generar-OTP")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Generar y enviar un código OTP",
            Description = "Genera un código de verificación de 4 dígitos y lo envía al celular y/o correo del usuario."
        )]
        public async Task<ActionResult<string>> GenerarOTP([FromBody] OTPRequestDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.UsuarioId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "Debe proporcionar un identificador de usuario válido."
                });
            }

            var resultado = await otpRepos.GenerarYEnviarOTPAsync(dto.UsuarioId);
            return Ok(resultado);
        }

        [HttpPost("Verificar-OTP")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Verificar código OTP",
            Description = "Verifica si un código OTP ingresado por el usuario es válido, que aún no ha sido usado y no ha expirado."
        )]
        public async Task<IActionResult> VerificarOTP([FromBody] OTPVerifyDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.UsuarioId) || dto.Token == 0)
            {
                return BadRequest(new
                {
                    Exito = false,
                    Message = "Debe proporcionar un usuario y un código válido."
                });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new
                {
                    Exito = false,
                    Message = "Datos de entrada inválidos.",
                    Errores = errors
                });
            }

            var resultado = await otpRepos.VerificarOTPAsync(dto.UsuarioId, dto.Token);

            if (!resultado.Success)
            {
                return BadRequest(new
                {
                    Exito = false,
                    Message = resultado.ErrorMessage
                });
            }

            return Ok(new
            {
                Exito = true,
                Message = resultado.ErrorMessage
            });
        }

    }
}
