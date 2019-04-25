namespace Soap.Api.Sso.Domain.Models.Aggregates
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard.Permissions;
    using DataStore.Interfaces.LowLevel;
    using Destructurama.Attributed;
    using Soap.Api.Sso.Domain.Models.Entities;
    using Soap.Api.Sso.Domain.Models.ValueObjects;
    using Soap.If.Utility;

    public class User : Aggregate, IUserWithPermissions
    {
        public enum UserStates
        {
            NULL = 0,

            EmailConfirmed = 1,

            AccountDisabled = 2
        }

        public List<SecurityToken> ActiveSecurityTokens { get; set; } = new List<SecurityToken>();

        public string Email { get; set; }

        public string FullName { get; set; }

        public AccountHistory History { get; set; } = new AccountHistory();

        [NotLogged]
        public PasswordDetails PasswordDetails { get; set; }  = new PasswordDetails();

        public List<IApplicationPermission> Permissions { get; set; } = new List<IApplicationPermission>();

        public FlaggedState Status { get; set; } = FlaggedState.Create(User.UserStates.NULL);

        public List<Guid> TagIds { get; set; } = new List<Guid>();

        public string UserName { get; set; }


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
            return PasswordDetails.PasswordResetStatefulProcessId != null;
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

        public bool PasswordResetRequestHasExpired()
        {
            return PasswordDetails.PasswordResetTokenExpiry != null && PasswordDetails.PasswordResetTokenExpiry.Value < DateTime.UtcNow;
        }

        public bool TempCredentialsMatch(string username, Guid passwordResetStatefulProcessId)
        {
            return UserName == username && PasswordDetails.PasswordResetStatefulProcessId == passwordResetStatefulProcessId;
        }

        public bool UserAccountIsLocked()
        {
            return History.InvalidLoginCount >= 5 && DateTime.UtcNow.Subtract(History.LastInvalidLoginAttempt.Value).Minutes > 5;
        }
    }
}