namespace EducaMente.DTO
{
    public class SendSMSRequestDTO
    {
        public int EntidadId { get; set; }
        public string Password { get; set; }
        public int TipoEnvioId { get; set; }
        public UsuarioNotifyDTO Tercero { get; set; }
        public string Asunto { get; set; }
        public string Mensaje { get; set; }
        public string SmsUnitTipo { get; set; } = "Prioritario";
        public int Flash { get; set; } = 0;
        public int Certificado { get; set; } = 1;
    }
}
