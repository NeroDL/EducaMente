namespace EducaMente.DTO
{
    public class NotificacionContextDTO
    {
        public UsuarioNotifyDTO Usuario { get; set; }
        public string Asunto { get; set; }
        public string Mensaje { get; set; }
        public string MensajeHtml { get; set; }
    }
}
