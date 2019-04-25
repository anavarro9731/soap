namespace Soap.Api.Sso.Domain.Models.ViewModels
{
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.Api.Sso.Domain.Models.ValueObjects;

    public class ClientSecurityContext
    {
        public string AuthToken { get; set; }

        public UserProfile UserProfile { get; set; }

        public static ClientSecurityContext Create(SecurityToken authToken, User user)
        {
            return new ClientSecurityContext
            {
                AuthToken = SecurityToken.EncryptToken(authToken), UserProfile = UserProfile.Create(user)
            };
        }
    }
}