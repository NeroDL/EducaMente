namespace EducaMente.DTO
{
    public class UsuarioTestOpcionRespuestaDetalleDTO
    {
        public string UsuarioTestItemOpcionId { get; set; } = string.Empty;
        public int OrdenOpcion { get; set; }
        public string TextoOpcion { get; set; } = string.Empty;
        public decimal ValorOpcion { get; set; }
        public string? UsuarioRespuestaId { get; set; }
        public decimal? ValorRespuesta { get; set; }
        public string FechaRespuesta { get; set; } = string.Empty;
        public bool EsSeleccionada { get; set; }
    }
}
