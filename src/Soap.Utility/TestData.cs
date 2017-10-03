namespace Soap.Utility
{
    using System;

    public static class Make
    {
        private static readonly Random Random = new Random();

        public static string AppendTimestamp(string value)
        {
            return $"{value}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }

        public static string NewString()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static string NewString(int maxLength)
        {
            return NewString().Substring(0, maxLength);
        }

        public static int RandomInt(int maxValue, int minValue = 1)
        {
            return Random.Next(minValue, maxValue);
        }
    }
}
