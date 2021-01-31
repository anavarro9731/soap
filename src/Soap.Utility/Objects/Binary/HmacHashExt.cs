namespace Soap.Utility.Objects.Binary
{
    using Soap.Utility.PWDTK;

    /// <summary>
    ///     This is a HMACSHA512 implementation of PBKDF2 With a 512-bit(64 bytes) random default salt and variable iterations
    /// </summary>
    public static class HmacHashExt
    {
        public static HmacHash CreateMatchingIterations(this HmacHash currentHmacHash, string textString, string saltHex = null)
        {
            saltHex ??= PWDTK.GetRandomSaltHexString();
            return new HmacHash(textString, currentHmacHash.IterationsUsed, saltHex);
        }

        public static bool EqualsString(this HmacHash hmacHash, string @string) =>
            PWDTK.ComparePasswordToHash(
                PWDTK.HashHexStringToBytes(hmacHash.HexSalt),
                @string,
                PWDTK.HashHexStringToBytes(hmacHash.HexHash),
                hmacHash.IterationsUsed);
    }

    public class HmacHash
    {
        public HmacHash(string @string, int iterations, string saltHex)
        {
            var passwordHashHex = PWDTK.PasswordToHashHexString(PWDTK.HashHexStringToBytes(saltHex), @string, iterations);

            HexHash = passwordHashHex;
            HexSalt = saltHex;
            IterationsUsed = iterations;
        }

        public string HexHash { get; internal set; }

        public string HexSalt { get; internal set; }

        public int IterationsUsed { get; internal set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;

            if (ReferenceEquals(this, obj)) return true;

            if (obj.GetType() != GetType()) return false;

            return Equals((HmacHash)obj);
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

        public override string ToString() => $"{HexSalt}:{HexHash}:{IterationsUsed}:";

        protected bool Equals(HmacHash other) =>
            string.Equals(HexSalt, other.HexSalt) && IterationsUsed == other.IterationsUsed
                                                  && string.Equals(HexHash, other.HexHash);
    }
}
