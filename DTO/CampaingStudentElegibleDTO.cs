namespace EducaMente.DTO
{
    public class CampaingStudentElegibleDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public bool FirstTestDone { get; set; }
    }
}
