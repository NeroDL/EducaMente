using System.Text.Json.Serialization;

namespace EducaMente.DTO
{
    public class PsicoTestListDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Alcance { get; set; } = string.Empty;          // 'Universal' | 'Personalizado'
        public string Estado { get; set; } = string.Empty;           // 'Activo', 'Inactivo', etc.
        public string FechaCreacion { get; set; } = string.Empty;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TargetUsuarioId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TargetUsuarioNombre { get; set; }

    }
}
