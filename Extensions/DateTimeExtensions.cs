namespace AParCarWeb.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly TimeZoneInfo CostaRicaZone =
            TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");

        public static DateTime ToCR(this DateTime date)
        {
            if (date.Kind == DateTimeKind.Unspecified)
                date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(date, CostaRicaZone);
        }
    }
}