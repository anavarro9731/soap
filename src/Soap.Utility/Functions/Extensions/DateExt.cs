namespace Soap.Utility.Functions.Extensions
{
    using System;

    public static class DateExt
    {
        public static DateTime ConvertFromMillisecondsEpochTime(this double src) =>
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(src);

        public static DateTime ConvertFromSecondsEpochTime(this double src) =>
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(src);

        public static double ConvertToMillisecondsEpochTime(this DateTime src) =>
            (src - new DateTime(1970, 1, 1)).TotalMilliseconds;

        public static double ConvertToSecondsEpochTime(this DateTime src) => (src - new DateTime(1970, 1, 1)).TotalSeconds;
    }
}