namespace Chubb_Entity.Utils
{
    public class TimeMethods
    {
        public static DateTime EC_Time()
        {
            TimeZoneInfo ecuadorTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Guayaquil");
            DateTime ecuadorTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ecuadorTimeZone);

            return ecuadorTime;
        }
    }
}
