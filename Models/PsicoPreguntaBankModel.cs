namespace EducaMente.Models
{
    public class PsicoPreguntaBankModel
    {
        public string Id { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public string Dimension { get; set; } = string.Empty;
        public string ScaleId { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;   // 'Activo' | 'Inactivo'
        public string Fuente { get; set; } = string.Empty;   // 'Base' | 'IA' | 'Orientador'
        public string FechaCreacion { get; set; } = string.Empty;
    }
}
