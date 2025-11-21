namespace EducaMente.DTO
{
    public class PsicoTestDetalleDTO
    {
        public string Id { get; set; }
        public string? Descripcion { get; set; }
        public string Alcance { get; set; }
        public string? TargetUsuarioId { get; set; }
        public string Estado { get; set; }
        public string FechaCreacion { get; set; } = string.Empty;
        public List<PsicoTestPreguntaDetalleDTO> Preguntas { get; set; } = new();
    }
}
