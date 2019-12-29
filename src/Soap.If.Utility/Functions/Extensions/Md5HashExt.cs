namespace Soap.If.Utility
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class Md5HashExt
    {
        public static string ToMd5Hash(this Guid input)
        {
            return input.ToString().ToMd5Hash();
        }

        public static string ToMd5SaltedHash(this Guid input, string salt)
        {
            return input.ToString().ToMd5SaltedHash(salt);
        }

        // Hash an input string and return the hash as
        // a 32 character hexadecimal string.
        public static string ToMd5Hash(this string input)
        {
            var md5Hasher = new MD5CryptoServiceProvider();

            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            return ConvertHashDataToHexString(data);
        }

        public static string ToMd5SaltedHash(this string input, string salt)
        {
            var hmacMd5 = new HMACMD5(Encoding.Default.GetBytes(salt));

            var saltedHash = hmacMd5.ComputeHash(Encoding.Default.GetBytes(input));

            return ConvertHashDataToHexString(saltedHash);
        }

        public static bool VerifWithSalt(this string input, string hash, string salt)
        {
            // Hash the input.
            var hashOfInput = ToMd5SaltedHash(input, salt);

            // Create a StringComparer an compare the hashes.
            var comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash)) return true;
            return false;
        }

        // Verify a hash against a string.
        public static bool Verify(this string input, string hash)
        {
            // Hash the input.
            var hashOfInput = ToMd5Hash(input);

            // Create a StringComparer an compare the hashes.
            var comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash)) return true;
            return false;
        }

        private static string ConvertHashDataToHexString(byte[] data)
        {
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (var i = 0; i < data.Length; i++) sBuilder.Append(data[i].ToString("x2"));

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}