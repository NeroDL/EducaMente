namespace EducaMente.DTO
{
    public class UsuarioLogueadoDTO
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Documento { get; set; }
        public string? Nombre { get; set; }
        public string? Rol { get; set; }
        public string? TipoUsuNombre { get; set; }
        public string? Estado { get; set; }
        public bool? FirstTestDone { get; set; }
        public string? Token { get; set; }
    }
}
