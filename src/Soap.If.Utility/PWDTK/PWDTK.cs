﻿namespace Soap.Utility.PWDTK
{
    using System;
    using System.Linq;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     Password Toolkit created by Thashiznets
    ///     This library facilitates crypto random salt generating and password hashing using HMACSHA512 based PBKDF2
    ///     Also it provides hash comparisson and password policy enforcement by regex (optional)
    ///     Created by thashiznets@yahoo.com.au
    ///     v2.0.0.2
    /// </summary>
    public static class PWDTK
    {
        /// <summary>
        ///     The default iteration count for key stretching
        /// </summary>
        public const int CDefaultIterationCount = 5000;

        /// <summary>
        ///     The default character length to create salt strings
        /// </summary>
        public const int CDefaultSaltLength = 64;

        /// <summary>
        ///     The key length used in the PBKDF2 derive bytes and matches the output of the underlying HMACSHA512 psuedo random
        ///     function
        /// </summary>
        public const int CKeyLength = 64;

        /// <summary>
        ///     The maximum size a password can be to avoid massive passwords that force the initial hash to take so long it
        ///     creates a DOS effect. Be careful increasing this, also make sure you catch exception for password too long!
        /// </summary>
        public const int CMaxPasswordLength = 1024;

        /// <summary>
        ///     The minimum size in characters the password hashing function will allow for a salt string, salt must be always
        ///     greater than 8 for PBKDF2 key derivitation to function
        /// </summary>
        public const int CMinSaltLength = 8;

        private const string CNonAlphaNumericRegex = "[^0-9a-zA-Z]";

        /// <summary>
        ///     Below are regular expressions used for password to policy comparrisons
        /// </summary>
        private const string CNumbersRegex = "[\\d]";

        private const string CUppercaseRegex = "[A-Z]";

        /// <summary>
        ///     A default password policy provided for use if you are unsure what to make your own PasswordPolicy
        /// </summary>
        public static PasswordPolicy CDefaultPasswordPolicy = new PasswordPolicy(1, 1, 2, 6, int.MaxValue);

        /// <summary>
        ///     Input a password and a hash value in bytes and it uses PBKDF2 HMACSHA512 to hash the password and compare it to the
        ///     supplied hash
        /// </summary>
        /// <param name="password">The text password to be hashed for comparrison</param>
        /// <param name="salt">The salt to be added to the  password pre hash</param>
        /// <param name="hash">The existing hash byte array you have stored for comparison</param>
        /// <param name="iterationCount">
        ///     The number of times you have specified to hash the password for key stretching. Default is
        ///     5000 iterations
        /// </param>
        /// <returns>True if Password matches Hash else returns  false</returns>
        public static bool ComparePasswordToHash(
            byte[] salt,
            string password,
            byte[] hash,
            int iterationCount = CDefaultIterationCount) =>
            PPasswordToHash(salt, StringToUtf8Bytes(password), iterationCount).SequenceEqual(hash);

        /// <summary>
        ///     Crypto Randomly generates a byte array that can be used safely as salt
        /// </summary>
        /// <param name="saltLength">Length of salt Byte Array to be generated. Defaults to 64</param>
        /// <returns>A Byte Array to be used as Salt</returns>
        public static byte[] GetRandomSalt(int saltLength = CDefaultSaltLength) => PGenerateRandomSalt(saltLength);

        /// <summary>
        ///     Crypto Randomly generates a byte array that can be used safely as salt and returns it as a HEX string.
        /// </summary>
        /// <param name="saltLength">Length of salt HEX string to be generated. Defaults to 64</param>
        /// <returns></returns>
        public static string GetRandomSaltHexString(int saltLength = CDefaultSaltLength) =>
            HashBytesToHexString(GetRandomSalt(saltLength));

        /// <summary>
        ///     Converts the Byte array Hash into a Human Friendly HEX String
        /// </summary>
        /// <param name="hash">The Hash value to convert</param>
        /// <returns>A HEX String representation of the Hash value</returns>
        public static string HashBytesToHexString(byte[] hash)
        {
            return ByteToHexBitFiddle(hash);

            static string ByteToHexBitFiddle(byte[] bytes)
            {
                var c = new char[bytes.Length * 2];
                int b;
                for (var i = 0; i < bytes.Length; i++)
                {
                    b = bytes[i] >> 4;
                    c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                    b = bytes[i] & 0xF;
                    c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
                }

                return new string(c);
            }
        }

        /// <summary>
        ///     Converts the Hash Hex String into a Byte[] for computational processing
        /// </summary>
        /// <param name="hashHexString">The Hash Hex String to convert back to bytes</param>
        /// <returns>Esentially reverses the HashToHexString function, turns the String back into Bytes</returns>
        public static byte[] HashHexStringToBytes(string hashHexString)
        {
            return HexToByteUsingByteManipulation(hashHexString);

            static byte[] HexToByteUsingByteManipulation(string s)
            {
                var bytes = new byte[s.Length / 2];
                for (var i = 0; i < bytes.Length; i++)
                {
                    var hi = s[i * 2] - 65;
                    hi = hi + 10 + ((hi >> 31) & 7);

                    var lo = s[i * 2 + 1] - 65;
                    lo = (lo + 10 + ((lo >> 31) & 7)) & 0x0f;

                    bytes[i] = (byte)(lo | (hi << 4));
                }

                return bytes;
            }
        }

        /// <summary>
        ///     Converts Salt + Password into a Hash
        /// </summary>
        /// <param name="salt">The salt to add infront of the password before processing the hash (Anti-Rainbow Table tactic)</param>
        /// <param name="password">The password used to compute the hash</param>
        /// <param name="iterationCount">
        ///     Repeat the PBKDF2 dunction this many times (Anti-Rainbow Table tactic), higher value =
        ///     more CPU usage which is better defence against cracking. Default is 5000 iterations
        /// </param>
        /// <returns>The Hash value of the salt + password as a Byte Array</returns>
        public static byte[] PasswordToHash(byte[] salt, string password, int iterationCount = CDefaultIterationCount)
        {
            PCheckSaltCompliance(salt);

            var convertedPassword = StringToUtf8Bytes(password);

            PCheckPasswordSizeCompliance(convertedPassword);

            return PPasswordToHash(salt, convertedPassword, iterationCount);
        }

        /// <summary>
        ///     Converts Salt + Password into a Hash represented as a HEX String
        /// </summary>
        /// <param name="salt">The salt to add infront of the password before processing the hash (Anti-Rainbow Table tactic)</param>
        /// <param name="password">The password used to compute the hash</param>
        /// <param name="iterationCount">
        ///     Repeat the PBKDF2 dunction this many times (Anti-Rainbow Table tactic), higher value =
        ///     more CPU usage which is better defence against cracking. Default is 5000 iterations
        /// </param>
        /// <returns>The Hash value of the salt + password as a HEX String</returns>
        public static string PasswordToHashHexString(byte[] salt, string password, int iterationCount = CDefaultIterationCount)
        {
            PCheckSaltCompliance(salt);

            var convertedPassword = StringToUtf8Bytes(password);

            PCheckPasswordSizeCompliance(convertedPassword);

            return HashBytesToHexString(PPasswordToHash(salt, convertedPassword, iterationCount));
        }

        /// <summary>
        ///     Convert a String into a SecureString and attempt to remove the original String from RAM
        /// </summary>
        /// <param name="toConvert">
        ///     This is the String to convert into a SecureString, please note that String ToConvert will be
        ///     Overwritten with the same length of *'s to try remove it from RAM so after conversion it can be used no more and
        ///     the SecureString must be used instead
        /// </param>
        /// <returns>A SecureString which resides in memory in an encrypted state</returns>
        public static SecureString StringToSecureString(ref string toConvert)
        {
            var outputSecureString = new SecureString();
            var overwriteString = string.Empty;

            foreach (var c in toConvert) outputSecureString.AppendChar(c);

            //Overwrite original String to try clear it from RAM (Note may still reside in Paged and Hiberfil)
            for (var i = 0; i < toConvert.Length; i++) overwriteString += "*";

            toConvert = overwriteString;

            return outputSecureString;
        }

        /// <summary>
        ///     Converts String to UTF8 friendly Byte Array
        /// </summary>
        /// <param name="stringToConvert">String to convert to Byte Array</param>
        /// <returns>A UTF8 decoded string as Byte Array</returns>
        public static byte[] StringToUtf8Bytes(string stringToConvert) => new UTF8Encoding(false).GetBytes(stringToConvert);

        /// <summary>
        ///     Tests the password for compliance against the supplied password policy
        /// </summary>
        /// <param name="password">The password to test for compliance</param>
        /// <param name="pwdPolicy">The PasswordPolicy that we are testing that the Password complies with</param>
        /// <returns>True for Password Compliance with the Policy</returns>
        public static bool TryPasswordPolicyCompliance(string password, PasswordPolicy pwdPolicy)
        {
            try
            {
                PCheckPasswordPolicyCompliance(password, pwdPolicy);
                return true;
            }
            catch
            {
            }

            return false;
        }

        /// <summary>
        ///     Tests the password for compliance against the supplied password policy
        /// </summary>
        /// <param name="password">The password to test for compliance</param>
        /// <param name="pwdPolicy">The PasswordPolicy that we are testing that the Password complies with</param>
        /// <param name="pwdPolicyException">The exception that will contain why the Password does not meet the PasswordPolicy</param>
        /// <returns>True for Password Compliance with the Policy</returns>
        public static bool TryPasswordPolicyCompliance(
            string password,
            PasswordPolicy pwdPolicy,
            ref PasswordPolicyException pwdPolicyException)
        {
            try
            {
                PCheckPasswordPolicyCompliance(password, pwdPolicy);
                return true;
            }
            catch (PasswordPolicyException ex)
            {
                pwdPolicyException = ex;
            }

            return false;
        }

        /// <summary>
        ///     Converts UTF8 friendly Byte Array to String
        /// </summary>
        /// <param name="bytesToConvert">Byte Array to convert to String</param>
        /// <returns>A UTF8 encoded Byte Array as String</returns>
        public static string Utf8BytesToString(byte[] bytesToConvert) => new UTF8Encoding(false).GetString(bytesToConvert);

        private static void PCheckPasswordPolicyCompliance(string password, PasswordPolicy pwdPolicy)
        {
            if (new Regex(CNumbersRegex).Matches(password).Count < pwdPolicy.ForceXNumeric)
            {
                throw new PasswordPolicyException(
                    "The password must contain " + pwdPolicy.ForceXNumeric + " numeric [0-9] characters");
            }

            if (new Regex(CNonAlphaNumericRegex).Matches(password).Count < pwdPolicy.ForceXNonAlphaNumeric)
            {
                throw new PasswordPolicyException(
                    "The password must contain " + pwdPolicy.ForceXNonAlphaNumeric + " special characters");
            }

            if (new Regex(CUppercaseRegex).Matches(password).Count < pwdPolicy.ForceXUpperCase)
            {
                throw new PasswordPolicyException(
                    "The password must contain " + pwdPolicy.ForceXUpperCase + " uppercase characters");
            }

            if (password.Length < pwdPolicy.PasswordMinLength)
            {
                throw new PasswordPolicyException(
                    "The password does not have a length of at least " + pwdPolicy.PasswordMinLength + " characters");
            }

            if (password.Length > pwdPolicy.PasswordMaxLength)
            {
                throw new PasswordPolicyException("The password is longer than " + pwdPolicy.PasswordMaxLength + " characters");
            }
        }

        private static void PCheckPasswordSizeCompliance(byte[] password)
        {
            if (password.Length > CMaxPasswordLength)
            {
                throw new PasswordTooLongException(
                    "The supplied password is longer than allowed, it must be smaller than " + CMaxPasswordLength
                                                                                             + " bytes long as defined by CMaxPasswordLength");
            }
        }

        private static void PCheckSaltCompliance(byte[] salt)
        {
            if (salt.Length < CMinSaltLength)
            {
                throw new SaltTooShortException(
                    "The supplied salt is too short, it must be at least " + CMinSaltLength
                                                                           + " bytes long as defined by CMinSaltLength");
            }
        }

        private static byte[] PGenerateRandomSalt(int saltLength)
        {
            var salt = new byte[saltLength];
            RandomNumberGenerator.Create().GetBytes(salt);
            return salt;
        }

        private static byte[] PPasswordToHash(byte[] salt, byte[] password, int iterationCount) =>
            new Rfc2898(password, salt, iterationCount).GetDerivedKeyBytes_PBKDF2_HMACSHA512(CKeyLength);

        /// <summary>
        ///     A PasswordPolicy defines min and max password length and also minimum amount of Uppercase, Non-Alpanum and Numerics
        ///     to be present in the password string
        /// </summary>
        public struct PasswordPolicy
        {
            /// <summary>
            ///     Creates a new PasswordPolicy Struct
            /// </summary>
            /// <param name="xUpper">Forces at least this number of Uppercase characters</param>
            /// <param name="xNonAlphaNumeric">Forces at least this number of Special characters</param>
            /// <param name="xNumeric">Forces at least this number of Numeric characters</param>
            /// <param name="minLength">Forces at least this number of characters</param>
            /// <param name="maxLength">Forces at most this number of characters</param>
            public PasswordPolicy(int xUpper, int xNonAlphaNumeric, int xNumeric, int minLength, int maxLength)
            {
                ForceXUpperCase = xUpper;
                ForceXNonAlphaNumeric = xNonAlphaNumeric;
                ForceXNumeric = xNumeric;
                PasswordMinLength = minLength;
                PasswordMaxLength = maxLength;
            }

            public int ForceXUpperCase { get; }

            public int ForceXNonAlphaNumeric { get; }

            public int ForceXNumeric { get; }

            public int PasswordMinLength { get; }

            public int PasswordMaxLength { get; }
        }
    }

    #region PWDTK Custom Exceptions

    public class SaltTooShortException : Exception
    {
        public SaltTooShortException(string message)
            : base(message)
        {
        }
    }

    public class PasswordPolicyException : Exception
    {
        public PasswordPolicyException(string message)
            : base(message)
        {
        }
    }

    public class PasswordTooLongException : Exception
    {
        public PasswordTooLongException(string message)
            : base(message)
        {
        }
    }

    #endregion
}