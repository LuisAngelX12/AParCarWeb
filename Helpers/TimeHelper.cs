namespace AParCarWeb.Helpers
{
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo CostaRicaZone =
            TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");

        public static DateTime ToCostaRicaTime(DateTime utcDate)
        {
            if (utcDate.Kind == DateTimeKind.Unspecified)
                utcDate = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utcDate, CostaRicaZone);
        }
    }
}