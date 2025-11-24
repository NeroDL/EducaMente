using EducaMente.DTO;

namespace EducaMente.Interface
{
    public interface I_OTP
    {
        Task<bool> GenerarYEnviarOTPAsync(string usuarioId);
        Task<OTPVerificationResult> VerificarOTPAsync(string usuarioId, int codigoOTP);
    }
}
