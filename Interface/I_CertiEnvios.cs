using EducaMente.DTO;

namespace EducaMente.Interface
{
    public interface I_CertiEnvios
    {
        Task<EnvioResultDTO> EnviarNotificacionAsync(UsuarioNotifyDTO usuario, string mensajeTexto, string asunto);
        Task<EnvioResultDTO> EnviarEmailAsync(UsuarioNotifyDTO usuario, string asunto, string cuerpo);
        Task<EnvioResultDTO> EnviarEmailConAdjuntosAsync(UsuarioNotifyDTO usuario, string asunto, string cuerpo, List<IFormFile> adjuntos);
    }
}
