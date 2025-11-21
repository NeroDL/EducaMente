using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class ParametroAddFormDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Codigo { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string? Descripcion { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [RegularExpression("^(String|Int|Decimal|Date|Bool|ImgUrl|Html)$",
            ErrorMessage = "El campo {0} no cumple con los valores permitidos")]
        public string TipoParametro { get; set; }
        public string? ValorString { get; set; }
        public int? ValorInt { get; set; }
        public decimal? ValorDecimal { get; set; }
        public DateTime? ValorDate { get; set; }
        public bool? ValorBool { get; set; }
        public string? ValorImgUrl { get; set; }
        public IFormFile? HtmlFile { get; set; } // Solo si es tipo HTML
    }
}
