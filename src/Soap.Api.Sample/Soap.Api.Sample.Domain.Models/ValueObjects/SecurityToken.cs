namespace Soap.Api.Sample.Domain.Models.ValueObjects
{
    using System;
    using Destructurama.Attributed;
    using Newtonsoft.Json;
    using Soap.If.Utility.PureFunctions;

    /// <summary>
    ///     a data structure representing a users authenticated status
    /// </summary>
    public class SecurityToken
    {
        public DateTime Expires { get; set; }

        public DateTime Issued { get; set; }

        public bool Renewable { get; set; }

        [NotLogged]
        public string SecureHmacHash { get; set; }

        public Guid UserId { get; set; }

        public static SecurityToken Create(Guid userId, string passwordHash, DateTime issued, TimeSpan expiresIn, bool renewable)
        {
            return new SecurityToken
            {
                Issued = issued,
                Expires = issued.Add(expiresIn),
                SecureHmacHash = passwordHash,
                UserId = userId,
                Renewable = renewable
            };
        }

        public static string EncryptToken(SecurityToken token)
        {
            const string AesPassword = "2xQXihMFXz7fE29oTKsrViBofE6GLr58";
            var encryptedMessage = AesEncryptamajig.Encrypt(JsonConvert.SerializeObject(token), AesPassword);
            var hashDetails = Soap.If.Utility.SecureHmacHash.CreateFrom(encryptedMessage);
            return hashDetails + encryptedMessage;
        }

        public static SecurityToken DecryptToken(string tokenString)
        {
            const string AesPassword = "2xQXihMFXz7fE29oTKsrViBofE6GLr58";
            SecurityToken token;
            try
            {
                var endOfHash = tokenString.LastIndexOf(':');
                var hashDetails = tokenString.Substring(0, endOfHash);
                var hashParts = hashDetails.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var encryptedMessage = tokenString.Substring(endOfHash + 1);
                if (Soap.If.Utility.SecureHmacHash.CreateFrom(encryptedMessage, int.Parse(hashParts[2]), hashParts[0]).HexHash != hashParts[1])
                {
                    throw new Exception("Hashes don't match. Token may have been tampered with.");
                }

                token = JsonConvert.DeserializeObject<SecurityToken>(AesEncryptamajig.Decrypt(encryptedMessage, AesPassword));
            }
            catch
            {
                throw new ApplicationException("User not authenticated", new Exception("Token cannot be decrypted"));
            }

            return token;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SecurityToken;
            if (other == null) return false;
            return Equals(other);
        }

        public bool Equals(SecurityToken other)
        {
            if (base.Equals(other)) return true;

            if (UserId == other.UserId && SecureHmacHash == other.SecureHmacHash && Renewable == other.Renewable && Expires == other.Expires
                && Issued == other.Issued)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ UserId.GetHashCode();
        }
    }
}