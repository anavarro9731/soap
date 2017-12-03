﻿namespace Palmtree.Api.Sso.Domain.Logic
{
    using CircuitBoard.Permissions;
    using DataStore.Interfaces;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Palmtree.Api.Sso.Domain.Models.ValueObjects;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.Utility.PureFunctions;

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