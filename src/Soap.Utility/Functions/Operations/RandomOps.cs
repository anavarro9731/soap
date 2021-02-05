namespace Soap.Utility.Functions.Operations
{
    using System;
    using System.Linq;

    public static class RandomOps
    {
        private static readonly Random Random = new Random();
        
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                                        .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public static int RandomInt(int maxValue, int minValue = 1) => Random.Next(minValue, maxValue);
    }
}
