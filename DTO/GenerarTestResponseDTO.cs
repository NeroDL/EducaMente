namespace EducaMente.DTO
{
    public class GenerarTestResponseDTO
    {
        public List<GenerarTestPreguntaExistenteDTO> UsarExistentes { get; set; } = new List<GenerarTestPreguntaExistenteDTO>();

        public List<GenerarTestPreguntaNuevaDTO> CrearNuevas { get; set; } = new List<GenerarTestPreguntaNuevaDTO>();
    }
}
