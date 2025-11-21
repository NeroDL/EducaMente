using EducaMente.DTO;

namespace EducaMente.Interface
{
    public interface I_PsicoTest
    {
        // Crear Test Universal (usa Codigo recibido)
        Task<Response1StringDTO> CrearTestUniversalAsync(PsicoTestUniversalAddDTO dto);
        // Crear Test Personalizado (codigo dinámico en el SP)
        Task<Response1StringDTO> CrearTestPersonalizadoAsync(PsicoTestPersonalizadoAddDTO dto);
        //Crear y asociar inmediatamente una pregunta con un test FUente: Orientador
        Task<Response1StringDTO> CrearPreguntaYAgregarATestAsync(PsicoPreguntaBankAddToTestDTO dto);
        //Crear y asociar inmediatamente una pregunta con un test Fuente: IA
        Task<Response1StringDTO> CrearPreguntaYAgregarATestByIAAsync(PsicoPreguntaBankAddToTestDTO dto);

        // Asociar pregunta al test (orden lo maneja el SP)
        Task<Response1StringDTO> AgregarPreguntaATestAsync(PsicoTestPreguntaAddDTO dto);
        Task<IEnumerable<PsicoTestListDTO>> GetAllAsync();
        // Obtener cabecera + listado de preguntas del test
        Task<PsicoTestDetalleDTO?> ObtenerDetalleTestAsync(string testId);
        Task<UsuarioTestConRespuestasDTO?> ObtenerConRespuestasAsync(string usuarioTestId);
        Task<UsuarioTestDetalleDTO> UsuarioTestStartAsync(string usuarioId);
        Task<Response1StringDTO> GuardarRespuestaAsync(UsuarioRespuestaSaveDTO dto);
        Task<UsuarioTestResultadoDTO> FinalizarAsync(string usuarioTestId);
        Task<string> SetObservacionPerfilAsync(UpdateObservacionTestUsuarioDTO dto);
        Task<string> CambiarEstadoAsync(PsicoTestCambiarEstadoDTO dto);
    }
}
