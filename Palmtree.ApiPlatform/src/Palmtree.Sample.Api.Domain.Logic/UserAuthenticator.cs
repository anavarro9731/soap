namespace Palmtree.Sample.Api.Domain.Logic
{
    using DataStore.Interfaces;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using ServiceApi.Interfaces.LowLevel.Permissions;
    using Palmtree.ApiPlatform.Interfaces;
    using Palmtree.ApiPlatform.Utility.PureFunctions;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;
    using Palmtree.Sample.Api.Domain.Models.ValueObjects;

    public class UserAuthenticator : IAuthenticateUsers
    {
        private readonly IDataStore dataStore;

        public UserAuthenticator(IDataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        public IUserWithPermissions Authenticate(IApiMessage message)
        {
            var encryptedToken = message.IdentityToken;

            var token = SecurityToken.DecryptToken(encryptedToken);

            var user = this.dataStore.ReadActiveById<User>(token.UserId).Result;

            Guard.Against(() => user == null, "Authentication Failed.", "User no longer exists");

            Guard.Against(() => user.AccountIsDisabled(), "Authentication Failed.", "User account disabled");

            Guard.Against(() => !user.HasSecurityToken(token), "Authentication Failed.", "Token not found");

            Guard.Against(() => user.PasswordDetails.PasswordHash != token.SecureHmacHash, "Authentication Failed. Password changed since this ticket was issued.");

            return user;
        }
    }
}
