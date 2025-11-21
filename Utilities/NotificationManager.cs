using EducaMente.DTO;
using EducaMente.Interface;
using System.Text.Json;

namespace EducaMente.Utilities
{
    public class NotificationManager
    {
        private readonly I_CertiEnvios certiEnvios;

        public NotificationManager(I_CertiEnvios _certiEnvios)
        {
            certiEnvios = _certiEnvios;
        }

        public async Task NotificarAlertaSMSAsync(NotificacionContextDTO dto)
        {
            var resultado = await certiEnvios.EnviarNotificacionAsync(
                dto.Usuario,
                dto.Mensaje,
                dto.Asunto
            );

            if (!resultado.Exitoso)
                throw new Exception($"No se pudo enviar el SMS: {resultado.Mensaje}. Response: {resultado.JsonCompleto}");
        }

        public async Task NotificarAlertaEmailAsync(NotificacionContextDTO dto)
        {
            var resultado = await certiEnvios.EnviarEmailAsync(
                dto.Usuario,
                dto.Asunto,
                dto.MensajeHtml
            );

            if (!resultado.Exitoso)
                throw new Exception($"No se pudo enviar el Email: {resultado.Mensaje}. Response: {resultado.JsonCompleto}");
        }
    }
}
