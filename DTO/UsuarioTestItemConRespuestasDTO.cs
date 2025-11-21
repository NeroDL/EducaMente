namespace EducaMente.DTO
{
    public class UsuarioTestItemConRespuestasDTO
    {
        public string UsuarioTestItemId { get; set; } = string.Empty;
        public string UsuarioTestId { get; set; } = string.Empty;
        public string PreguntaId { get; set; } = string.Empty;
        public string TextoSnapshot { get; set; } = string.Empty;
        public string DimensionSnapshot { get; set; } = string.Empty;
        public string ScaleIdSnapshot { get; set; } = string.Empty;
        public int OrdenPregunta { get; set; }
        public List<UsuarioTestOpcionRespuestaDetalleDTO> Opciones { get; set; } = new();
    }
}
