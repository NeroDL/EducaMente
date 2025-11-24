using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class OTPVerifyDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string UsuarioId { get; set; } = string.Empty;
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int Token { get; set; }
    }
}
