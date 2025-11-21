using EducaMente.DataAnotations;
using System.ComponentModel.DataAnnotations;

namespace EducaMente.DTO
{
    public class ForgotPasswordDTO
    {
        [EmailValidation]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Email { get; set; }
    }
}
