namespace Palmtree.Api.Sso.Domain.Models.Aggregates
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard.Permissions;
    using DataStore.Interfaces.LowLevel;
    using Destructurama.Attributed;
    using Palmtree.Api.Sso.Domain.Models.Entities;
    using Palmtree.Api.Sso.Domain.Models.ValueObjects;
    using Soap.If.Utility;

    public class User : Aggregate, IUserWithPermissions
    {
        public enum UserStates
        {
            EmailConfirmed = 1,

            AccountDisabled = 2
        }

        public List<SecurityToken> ActiveSecurityTokens { get; set; } = new List<SecurityToken>();

        public string Email { get; set; }

        public string FullName { get; set; }

        public AccountHistory History { get; set; }

        [NotLogged]
        public PasswordDetails PasswordDetails { get; set; }

        public List<IApplicationPermission> Permissions { get; set; }

        public FlaggedState Status { get; set; }

        public List<Guid> ThingIds { get; set; }

        public string UserName { get; set; }

        public static User Create(
            string email,
            string fullName,
            PasswordDetails passwordDetails,
            List<IApplicationPermission> permissions,
            string userName,
            Guid? id = null)
        {
            return new User
            {
                Email = email,
                FullName = fullName,
                History = AccountHistory.Create(),
                PasswordDetails = passwordDetails,
                Permissions = new List<IApplicationPermission>(),
                UserName = userName,
                ActiveSecurityTokens = new List<SecurityToken>(),
                id = id ?? Guid.NewGuid(),
                ThingIds = new List<Guid>(),
                Status = FlaggedState.Create<UserStates>()
            };
        }

        public bool AccountIsDisabled()
        {
            return Status.HasState(UserStates.AccountDisabled);
        }

        public bool EmailConfirmed()
        {
            return Status.HasState(UserStates.EmailConfirmed);
        }

        public bool HasPermission(IApplicationPermission permission)
        {
            return Permissions.Contains(permission);
        }

        public bool HasRequestedAPasswordReset()
        {
            return PasswordDetails.PasswordResetTokenHash != null;
        }

        public bool HasSecurityToken(SecurityToken token)
        {
            return ActiveSecurityTokens.Contains(token);
        }

        public bool NormalCredentialsMatch(Credentials credentials)
        {
            return UserName == credentials.Username && PasswordDetails.PasswordHash
                   == SecureHmacHash.CreateFrom(credentials.Password, PasswordDetails.HashIterations, PasswordDetails.HexSalt).HexHash;
        }

        public bool PasswordResetTokenHasExpired()
        {
            return PasswordDetails.PasswordResetTokenExpiry != null && PasswordDetails.PasswordResetTokenExpiry.Value < DateTime.UtcNow;
        }

        public bool TempCredentialsMatch(string username, string passwordResetToken)
        {
            return UserName == username && PasswordDetails.PasswordResetTokenHash
                   == SecureHmacHash.CreateFrom(passwordResetToken, PasswordDetails.HashIterations, PasswordDetails.HexSalt).HexHash;
        }

        public bool UserAccountIsLocked()
        {
            return History.InvalidLoginCount >= 5 && DateTime.UtcNow.Subtract(History.LastInvalidLoginAttempt.Value).Minutes > 5;
        }
    }
}