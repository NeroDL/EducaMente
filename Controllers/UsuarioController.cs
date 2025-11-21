using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Models;
using EducaMente.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EducaMente.Controllers
{
    [Route("user")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsuarioController : ControllerBase
    {
        private readonly I_Usuario usuarioRepos;
        private readonly I_ExcelExporterService excelExporterService;
        public UsuarioController(I_Usuario _usuarioRepos, I_ExcelExporterService _excelExporterService)
        {
            usuarioRepos = _usuarioRepos;
            excelExporterService = _excelExporterService;
        }

        [HttpPost("Create")]
        [Authorize]
        [SwaggerOperation(Summary = "Crear un usuario", Description = "Permite a un Usuario Administrador crear nuevos usuarios")]
        public async Task<ActionResult<string>> Add(UsuarioAddDTO usuario)
        {
            if (usuario == null)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El cuerpo de la solicitud no puede ser nulo."
                });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Datos de entrada inválidos.",
                    Details = errors
                });
            }

            if (!string.IsNullOrEmpty(usuario.Password))
                usuario.Password = Encriptacion.Hashear(usuario.Password);

            var response = await usuarioRepos.AddAsync(usuario);
            return Ok(response);
        }

        [HttpGet("Get")]
        [Authorize]
        [SwaggerOperation(Summary = "Listar todos los usuarios", Description = "Este Endpoint genera una lista con todos los usuarios registrados.")]
        public async Task<ActionResult<IEnumerable<UsuarioModel>>> GetAll()
        {
            var usuarios = (await usuarioRepos.GetAllAsync());
            if (usuarios == null || !usuarios.Any())
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "No se encontró ningún usuario."
                });
            }
            return Ok(usuarios);
        }

        [HttpGet("Get/{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Visualizar un usuario", Description = "Consulta un único usuario por Id")]
        public async Task<ActionResult<UsuarioModel>> GetItem(string id)
        {
            var usuario = (await usuarioRepos.GetItemAsync(id));

            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del funcionario es obligatorio"
                });
            }

            if (usuario == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = $"No se encontró ningún usuario con el id: {id}."
                });
            }

            return Ok(usuario);
        }

        [HttpGet("ViewAlerts/{responsableId}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Listar alertas de riesgo por responsable",
            Description = "Devuelve todas las alertas de riesgo asignadas al responsable indicado."
        )]
        public async Task<ActionResult<IEnumerable<AlertaDTO>>> GetAlertasByResponsable(string responsableId)
        {
            if (string.IsNullOrWhiteSpace(responsableId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del responsable es obligatorio."
                });
            }

            IEnumerable<AlertaDTO> alertas;

            try
            {
                alertas = await usuarioRepos.GetAlertasByResponsableAsync(responsableId);
            }
            catch
            {
                // Dejas que el middleware global maneje la excepción
                throw;
            }

            if (alertas == null || !alertas.Any())
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "El responsable no tiene alertas de riesgo registradas."
                });
            }

            return Ok(alertas);
        }


        [HttpGet("PerfilPsico/Actual/{usuarioId}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Obtener perfil psicológico actual de un estudiante",
            Description = "Devuelve el último perfil psicológico generado para el estudiante, incluyendo puntajes por dimensión y nivel de riesgo."
        )]
        public async Task<ActionResult<PerfilPsicoActualDTO>> GetPerfilPsicoActual(string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del usuario es obligatorio."
                });
            }

            try
            {
                var perfil = await usuarioRepos.GetPerfilPsicoActualAsync(usuarioId);
                return Ok(perfil);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "El usuario no tiene un perfil psicológico actual registrado."
                });
            }
            catch (Exception)
            {
                // Dejas que el middleware global maneje esto
                throw;
            }
        }

        [HttpGet("GetAll-PerfilPsico")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Obtiene todos los perfiles psicológicos actuales de los estudiantes",
            Description = "Este endpoint recupera todos los perfiles psicológicos actuales de los estudiantes evaluados."
        )]
        public async Task<ActionResult<IEnumerable<PerfilPsicoActualDTO>>> GetAllPerfilPsicoActual()
        {
            try
            {
                var perfiles = await usuarioRepos.GetAllPerfilPsicoActualAsync();

                if (perfiles == null || !perfiles.Any())
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Code = "NOT_FOUND",
                        Message = "No se encontraron perfiles psicológicos actuales para los estudiantes."
                    });
                }

                return Ok(perfiles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Code = "INTERNAL_SERVER_ERROR",
                    Message = $"Hubo un error al intentar recuperar los perfiles psicológicos: {ex.Message}"
                });
            }
        }

        [HttpGet("ExportarPerfiles")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Exportar perfiles psicológicos de los estudiantes a Excel",
            Description = "Genera un archivo Excel con los perfiles psicológicos de los estudiantes y lo descarga."
        )]
        public async Task<ActionResult> ExportarPerfilesAsync()
        {
            try
            {
                // Llamar al servicio para exportar los perfiles a Excel
                return await excelExporterService.ExportarPerfilesAsync();
            }
            catch (Exception ex)
            {
                // En caso de error, retornar un mensaje de error adecuado
                return StatusCode(500, new ApiErrorResponse
                {
                    Code = "INTERNAL_SERVER_ERROR",
                    Message = $"Hubo un error al intentar exportar los perfiles: {ex.Message}"
                });
            }
        }


        [HttpGet("PerfilPsico/Hist/{usuarioId}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Obtener historial de perfiles psicológicos de un estudiante",
            Description = "Devuelve el historial de evaluaciones psicológicas del estudiante (PerfilPsicoHist), ordenado por fecha de generación descendente."
        )]
        public async Task<ActionResult<IEnumerable<PerfilPsicoHistDTO>>> GetPerfilPsicoHist(string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del usuario es obligatorio."
                });
            }

            IEnumerable<PerfilPsicoHistDTO> historial;

            try
            {
                historial = await usuarioRepos.GetPerfilPsicoHistAsync(usuarioId);
            }
            catch (Exception)
            {
                // Igual que arriba, dejas que el middleware maneje la excepción global
                throw;
            }

            if (historial == null || !historial.Any())
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "El usuario no tiene historial de evaluaciones psicológicas."
                });
            }

            return Ok(historial);
        }


        [HttpPut("Update")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Actualizar usuario",
            Description = "Permite actualizar la información de un usuario"
        )]
        public async Task<ActionResult<string>> Update(UsuarioUpdateDTO usuarioDTO)
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

            var message = await usuarioRepos.UpdateAsync(usuarioDTO);
            return Ok(new { Message = message });
        }

        [HttpPut("UpdatePassword")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Actualizar contraseña",
            Description = "Permite a un usuario cambiar su contraseña."
        )]
        public async Task<ActionResult<string>> UpdatePassword(UsuarioPasswordDTO updatePasswordDTO)
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

            var message = await usuarioRepos.UpdatePasswordAsync(updatePasswordDTO);
            return Ok(new { Message = message });
        }


        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Inicia el proceso de recuperación de contraseña",
            Description = @"Este endpoint se utiliza cuando un usuario hace clic en ""¿Olvidaste tu contraseña?"".
Debe enviar un objeto con un único campo llamado `email`.

El backend validará si existe un usuario registrado con ese correo y, de ser así, generará un token de recuperación válido por 5 minutos.

Se enviará un **correo electrónico** al usuario con un **enlace** como:

`https://localhost:7090/reset-password?token=GUID`

⚠️ El enlace expira automáticamente luego de 5 minutos o tras ser usado una vez.

El frontend solo debe enviar el email, no necesita manejar el token en este paso.

**Body esperado:**
```json
{
""email"": ""usuario@ejemplo.com""
}
```"
        )]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
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

            // La respuesta siempre es positiva, incluso si el correo no está registrado.
            await usuarioRepos.ForgotPasswordAsync(dto.Email);

            return Ok(new { Message = "Se ha enviado un enlace de recuperación." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Restablece la contraseña usando el enlace recibido por correo",
            Description = @"Este endpoint se utiliza cuando un usuario accede al formulario de recuperación tras hacer clic en el enlace enviado por correo.

El frontend debe capturar el `token` de la URL (`reset-password?token=GUID`) y construir el siguiente cuerpo para enviarlo al backend junto con la nueva contraseña:

**Body esperado:**
{
""token"": ""550e8400-e29b-41d4-a716-446655440000"",
""nuevaPassword"": ""12345678"",
""confirmacionPassword"": ""12345678""
}

Validaciones aplicadas:
- La contraseña debe tener mínimo 8 caracteres.
- La confirmación debe coincidir con la nueva contraseña.
- El token debe ser válido, no expirado y no haber sido usado anteriormente.

El backend actualizará la contraseña del usuario y marcará el token como usado. Si todo sale bien, el usuario podrá iniciar sesión con su nueva contraseña."
        )]

        public async Task<IActionResult> ResetPassword([FromBody] UsuarioResetPasswordDTO dto)
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

            var resultado = await usuarioRepos.ResetPasswordAsync(dto);
            return Ok(new { Message = resultado });
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Iniciar sesión",
            Description = "Campos para que un usuario pueda iniciar sesión."
        )]
        public async Task<ActionResult<UsuarioLogueadoDTO>> Login(UsuarioLoginDTO usuario)
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

            var usuarioLogueado = await usuarioRepos.Login(usuario);

            if (usuarioLogueado == null)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "INVALID_CREDENTIALS",
                    Message = "Usuario y/o contraseña incorrectos."
                });
            }

            return Ok(usuarioLogueado);
        }

        [HttpPut("ResetFirstTestDone/{usuarioId}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Reiniciar flag de primer login",
            Description = "Reinicia el campo FirstTestDone del usuario a 0, para que el sistema vuelva a tratarlo como si fuera la primera vez que inicia sesión y debiera presentar el test universal."
        )]
        public async Task<IActionResult> ResetFirstTestDone(string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Code = "MISSING_PARAMETER",
                    Message = "El identificador del usuario es obligatorio."
                });
            }

            try
            {
                var message = await usuarioRepos.ResetFirstTestDone(usuarioId);

                return Ok(new
                {
                    Message = message
                });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
