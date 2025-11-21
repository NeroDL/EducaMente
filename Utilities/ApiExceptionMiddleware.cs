using EducaMente.Models;
using System.Net;
using System.Text.Json;

namespace EducaMente.Utilities
{
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiExceptionMiddleware> _logger;

        public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                // Si la respuesta ya empezó, no intentes escribir nada más.
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("La respuesta ya inició; re-lanzando excepción.");
                    throw;
                }

                var response = new ApiErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = "Ocurrió un error inesperado.",
                    Details = new[] { ex.Message }
                };

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Asegura tipo y charset explícitamente
                context.Response.ContentType = "application/json; charset=utf-8";

                // Escribe JSON con UTF-8 (WriteAsJsonAsync ya usa UTF-8 y pone el header adecuado)
                await context.Response.WriteAsJsonAsync(
                    response,
                    new JsonSerializerOptions
                    {
                        // Opcional: evita escapes innecesarios en mensajes con acentos
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    }
                );
            }
        }
    }
}
