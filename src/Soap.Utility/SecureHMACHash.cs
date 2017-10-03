namespace Soap.Utility
{
    using PWDTK_DOTNET451;

    /// <summary>
    ///     This is a HMACSHA512 implementation of PBKDF2 With a 512-bit(64 bytes) random default salt and variable iterations
    /// </summary>
    public class SecureHmacHash
    {
        public const int CurrentIterations = 12000;

        public string HexHash { get; private set; }

        public string HexSalt { get; private set; }

        public int IterationsUsed { get; private set; }

        public static SecureHmacHash CreateFrom(string textString, int iterations = CurrentIterations, string saltHex = null)
        {
            saltHex = saltHex ?? PWDTK.GetRandomSaltHexString();
            return Create(textString, iterations, saltHex);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;

            if (ReferenceEquals(this, obj)) return true;

            if (obj.GetType() != GetType()) return false;

            return Equals((SecureHmacHash)obj);
        }

        public bool EqualsPassword(string textString)
        {
            return PWDTK.ComparePasswordToHash(PWDTK.HashHexStringToBytes(HexSalt), textString, PWDTK.HashHexStringToBytes(HexHash), CurrentIterations);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = HexSalt != null ? HexSalt.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ IterationsUsed;
                hashCode = (hashCode * 397) ^ (HexHash != null ? HexHash.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{HexSalt}:{HexHash}:{IterationsUsed}:";
        }

        protected bool Equals(SecureHmacHash other)
        {
            return string.Equals(HexSalt, other.HexSalt) && IterationsUsed == other.IterationsUsed && string.Equals(HexHash, other.HexHash);
        }

        private static SecureHmacHash Create(string password, int iterations, string saltHex)
        {
            var passwordHashHex = PWDTK.PasswordToHashHexString(PWDTK.HashHexStringToBytes(saltHex), password, iterations);

            return new SecureHmacHash
            {
                HexHash = passwordHashHex,
                HexSalt = saltHex,
                IterationsUsed = iterations
            };
        }
    }
}
