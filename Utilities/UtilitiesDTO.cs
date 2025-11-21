using EducaMente.DTO;

namespace EducaMente.Utilities
{
    public class UtilitiesDTO
    {
        public static ExportarPerfilesDTO MapToExportarPerfilesDTO(PerfilPsicoActualDTO perfil)
        {

            return new ExportarPerfilesDTO
            {
                NombreUsuario = perfil.NombreUsuario,
                FechaEvaluacion = perfil.FechaEvaluacion,
                AnsiedadScore = perfil.AnsiedadScore,
                EstresScore = perfil.EstresScore,
                MotivacionScore = perfil.MotivacionScore,
                AutoestimaScore = perfil.AutoestimaScore,
                PropositoScore = perfil.PropositoScore,
                NivelRiesgo = perfil.NivelRiesgo,
                EstadoEmocional = perfil.EstadoEmocional,
                Observacion = perfil.Observacion
            };
        }
    }
}
