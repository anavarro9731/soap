namespace Palmtree.Sample.Api.Domain.Models.ViewModels
{
    using Newtonsoft.Json;
    using Palmtree.ApiPlatform.Utility;
    using Palmtree.ApiPlatform.Utility.PureFunctions;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;
    using Palmtree.Sample.Api.Domain.Models.ValueObjects;

    public class ClientSecurityContext
    {
        public string AuthToken { get; set; }

        public UserProfile UserProfile { get; set; }

        public static ClientSecurityContext Create(SecurityToken authToken, User user)
        {
            return new ClientSecurityContext
            {
                AuthToken = EncryptToken(authToken),
                UserProfile = UserProfile.Create(user)
            };
        }

        public static string EncryptToken(SecurityToken token)
        {
            const string AesPassword = "2xQXihMFXz7fE29oTKsrViBofE6GLr58";
            var encryptedMessage = AesEncryptamajig.Encrypt(JsonConvert.SerializeObject(token), AesPassword);
            var hashDetails = SecureHmacHash.CreateFrom(encryptedMessage);
            return hashDetails + encryptedMessage;
        }
    }
}
