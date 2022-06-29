namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel.Permissions;
    using Soap.Interfaces.Messages;

    public interface IIdaamProvider
    {
        Task AddRoleToUser(string idaamProviderUserId, Role role);

        Task AddRoleToUser(string idaamProviderUserId, Role role, AggregateReference scopeReferenceToAdd);

        Task AddRoleToUser(string idaamProviderUserId, Role role, List<AggregateReference> scopeReferencesToAdd);

        Task AddScopeToUserRole(string idaamProviderUserId, Role role, List<AggregateReference> scopeReferencesToAdd);

        Task AddScopeToUserRole(string idaamProviderUserId, Role role, AggregateReference scopeReferenceToAdd);

        Task<string> AddUser(AddUserArgs args);

        Task<string> BlockUser(string idaamProviderId);

        Task ChangeUserPassword(string idaamProviderId, string newPassword);

        Task<IdentityClaims> GetAppropriateClaimsFromAccessToken(string bearerToken, ApiMessage apiMessage, ISecurityInfo securityInfo);

        Task<User> GetLimitedUserProfileFromIdentityToken(string idToken);

        Task RemoveRoleFromUser(string idaamProviderUserId, Role role);

        Task RemoveScopeFromUserRole(string idaamProviderUserId, Role role, AggregateReference scopeReferenceToRemove);

        Task RemoveUser(string idaamProviderId);

        public class User : IdaamProviderProfile
        {
            public string Email { get; set; }

            public string FirstName { get; set; }

            public string IdaamProviderId { get; set; }

            public string LastName { get; set; }
        }

        public record AddUserArgs(IUserProfile Profile, string Password, bool EmailVerified)
        {
            public bool Blocked { get; init; } = default;

            public bool VerifyEmail { get; init; } = true;
        }
    }
}