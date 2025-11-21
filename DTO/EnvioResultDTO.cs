namespace EducaMente.DTO
{
    public class EnvioResultDTO
    {
        public string Estado { get; set; }
        public string Mensaje { get; set; }
        public string MensajeTexto { get; set; }
        public string JsonCompleto { get; set; }
        public bool Exitoso => Estado?.Equals("Enviado", StringComparison.OrdinalIgnoreCase) == true;
    }
}
