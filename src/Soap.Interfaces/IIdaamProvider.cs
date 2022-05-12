namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel.Permissions;

    public record AddUserArgs(string FirstName, string LastName, string Email, string Password)
    {
        public bool Blocked { get; init; } = default;

        public bool EmailVerified { get; init; } = default;

        public bool VerifyEmail { get; init; } = true;
    }

    public interface IIdaamProvider
    {
        public Task AddRoleToUser(string idaamProviderUserId, RoleId roleId);

        public Task AddRoleToUser(string idaamProviderUserId, RoleId roleId, DatabaseScopeReference scopeReferenceToAdd);

        public Task AddRoleToUser(string idaamProviderUserId, RoleId roleId, List<DatabaseScopeReference> scopeReferencesToAdd);

        public Task AddScopeToUserRole(string idaamProviderUserId, RoleId roleId, List<DatabaseScopeReference> scopeReferencesToAdd);

        public Task AddScopeToUserRole(string idaamProviderUserId, RoleId roleId, DatabaseScopeReference scopeReferenceToAdd);

        public Task<string> AddUser(AddUserArgs userProfile);

        public Task RemoveRoleFromUser(string idaamProviderUserId, RoleId roleId);

        public Task RemoveScopeFromUserRole(string idaamProviderUserId, RoleId roleId, DatabaseScopeReference scopeReferenceToRemove);
    }
}