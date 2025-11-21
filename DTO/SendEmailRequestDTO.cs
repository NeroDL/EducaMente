namespace EducaMente.DTO
{
    public class SendEmailRequestDTO
    {
        public int EntidadId { get; set; }
        public string Password { get; set; } = null!;
        public int TipoEnvioId { get; set; }
        public UsuarioNotifyDTO Tercero { get; set; } = null!;
        public string Asunto { get; set; } = null!;
        public string Cuerpo { get; set; } = null!;
    }
}
