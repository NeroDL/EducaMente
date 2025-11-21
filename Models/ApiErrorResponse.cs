namespace EducaMente.Models
{
    public class ApiErrorResponse
    {
        public bool Success { get; set; } = false;
        public string Code { get; set; }  // Ej: "VALIDATION_ERROR", "NOT_FOUND"
        public string Message { get; set; }
        public IEnumerable<string> Details { get; set; }
    }
}
