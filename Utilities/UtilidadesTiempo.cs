namespace EducaMente.Utilities
{
    public class UtilidadesTiempo
    {
        public static DateTime ObtenerFechaColombia()
        {
            var colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, colombiaTimeZone);
        }
    }
}
