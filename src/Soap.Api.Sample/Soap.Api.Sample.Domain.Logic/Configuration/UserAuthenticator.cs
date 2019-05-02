namespace Soap.Api.Sample.Domain.Logic.Configuration
{
    using CircuitBoard.Permissions;
    using DataStore.Interfaces;
    using Soap.Api.Sample.Domain.Models.ValueObjects;
    using Soap.Api.Sample.Domain.Models.ViewModels;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;

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

            return new User
            {
                id = token.UserId
            };
        }
    }
}