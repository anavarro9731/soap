namespace Soap.Utility.Functions.Operations
{
    using System;

    public static class RandomOps
    {
        private static readonly Random Random = new Random();

        public static string NewString() => Guid.NewGuid().ToString("N");

        public static string NewString(int maxLength) => NewString().Substring(0, maxLength);

        public static int RandomInt(int maxValue, int minValue = 1) => Random.Next(minValue, maxValue);
    }
}