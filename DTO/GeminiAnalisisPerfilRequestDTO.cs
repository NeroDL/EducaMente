namespace EducaMente.DTO
{
    public class GeminiAnalisisPerfilRequestDTO
    {
        /* Prompt base que viene de la BD (por ejemplo, PROMT.ANALISIS.PERFIL.PSICO),
         con los placeholders {{PERFIL_PSICO_JSON}} y {{TEST_CON_RESPUESTAS_JSON}}.*/
        public string PromptBase { get; set; } = string.Empty;
        // JSON del PerfilPsicoActualDTO ya serializado.
        public string? PerfilJSON { get; set; }
        //JSON del UsuarioTestConRespuestasDTO ya serializado.
        public string? TestJSON { get; set; }
    }
}
