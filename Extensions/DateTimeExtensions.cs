using System.Globalization;

namespace AParCarWeb.Extensions
{
    public static class DateTimeExtensions
    {
        // Zona horaria de Costa Rica
        private static readonly TimeZoneInfo CostaRicaZone =
            TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");

        /// <summary>
        /// Convierte cualquier DateTime UTC a hora de Costa Rica
        /// y devuelve string en formato 12 horas con AM/PM.
        /// Ejemplo: "18/03/2026 02:45 p. m."
        /// </summary>
        public static string ToCR12H(this DateTime date)
        {
            // Asegurarse de que el DateTime sea UTC
            if (date.Kind == DateTimeKind.Unspecified)
                date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            // Convertir a hora de Costa Rica
            var crTime = TimeZoneInfo.ConvertTimeFromUtc(date, CostaRicaZone);

            // Retornar como string 12h con AM/PM en español
            return crTime.ToString("dd/MM/yyyy hh:mm tt", new CultureInfo("es-CR"));
        }
    }
}