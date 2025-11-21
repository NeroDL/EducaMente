using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class ParametroAddDTO
    {
        [StringLength(50)]
        public string Codigo { get; set; }
        [StringLength(500)]
        public string Descripcion { get; set; }
        [RegularExpression("^(String|Int|Decimal|Date|Bool|ImgUrl|Email)$",
            ErrorMessage = "El campo {0} no cumple con los valores permitidos")]
        public string TipoParametro { get; set; }
        public string? ValorString { get; set; }
        public int? ValorInt { get; set; }
        public decimal? ValorDecimal { get; set; }
        public DateTime? ValorDate { get; set; }
        public bool? ValorBool { get; set; }
        public string? ValorImgUrl { get; set; }
        public string? ValorHtml { get; set; }
    }
}
