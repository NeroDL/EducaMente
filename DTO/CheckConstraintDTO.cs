namespace EducaMente.DTO
{
    public class CheckConstraintDTO
    {
        public string ConstraintName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public List<string> AllowedValues { get; set; } = new();
    }
}
