namespace Expence.Domain.Constants
{
    public class DateTimeConstants
    {
        public static DateTime CurrentWestAfricanTime
        {
            get
            {
                string windowsTimeZoneId = "W. Central Africa Standard Time";
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);
                DateTime currentTimeInWestAfrica = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
                return currentTimeInWestAfrica;
            }
        }

        public static string CurrentUnixTimeStamp
        {
            get
            {
                return DateTime.UtcNow.ToString("yyMMddHHmmss");
            }
        }
    }
}
