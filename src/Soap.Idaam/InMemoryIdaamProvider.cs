namespace Soap.Idaam
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;

    public class InMemoryIdaamProvider : IIdaamProvider
    {
        private readonly IBootstrapVariables bootstrapVariables;

        private readonly Dictionary<string, TestIdentity> identities;

        private readonly ISecurityInfo securityInfo;

        public InMemoryIdaamProvider(List<TestIdentity> testIdentities, IBootstrapVariables bootstrapVariables, ISecurityInfo securityInfo)
        {
            this.bootstrapVariables = bootstrapVariables;
            this.securityInfo = securityInfo;
            this.identities = testIdentities.ToDictionary(x => x.UserProfile.IdaamProviderId);
        }

        public Task AddRoleToUser(string idaamProviderUserId, Role role)
        {
            return AddRoleToUser(idaamProviderUserId, role, new List<AggregateReference>());
        }

        public Task AddRoleToUser(string idaamProviderUserId, Role role, AggregateReference scopeReferenceToAdd)
        {
            return AddRoleToUser(idaamProviderUserId, role, new List<AggregateReference> { scopeReferenceToAdd });
        }

        public Task AddRoleToUser(string idaamProviderUserId, Role role, List<AggregateReference> scopeReferencesToAdd)
        {
            Guard.Against(!this.identities.ContainsKey(idaamProviderUserId), $"An IDAAM user with ID {idaamProviderUserId} does not exist.");

            var roleInstance = this.identities[idaamProviderUserId].Roles.SingleOrDefault(x => x.RoleKey == role.Key);

            scopeReferencesToAdd ??= new List<AggregateReference>();

            if (roleInstance == null)
            {
                this.identities[idaamProviderUserId]
                    .Roles.Add(
                        new RoleInstance
                        {
                            RoleKey = role.Key,
                            ScopeReferences = scopeReferencesToAdd
                        });
            }
            else
            {
                //* merge scopes
                foreach (var scopeReferenceToAdd in scopeReferencesToAdd.Where(
                             scopeReferenceToAdd => !roleInstance.ScopeReferences.Contains(scopeReferenceToAdd)))
                    roleInstance.ScopeReferences.Add(scopeReferenceToAdd);
            }

            return Task.CompletedTask;
        }

        public Task AddScopeToUserRole(string idaamProviderUserId, Role role, List<AggregateReference> scopeReferencesToAdd)
        {
            return AddRoleToUser(idaamProviderUserId, role, scopeReferencesToAdd);
        }

        public Task AddScopeToUserRole(string idaamProviderUserId, Role role, AggregateReference scopeReferenceToAdd)
        {
            return AddRoleToUser(idaamProviderUserId, role, scopeReferenceToAdd);
        }

        public Task<string> AddUser(IIdaamProvider.AddUserArgs args)
        {
            Guard.Against(
                this.identities.ContainsKey(args.Profile.IdaamProviderId),
                "This IDAAM user already exists, you cannot add it a second time.");
            this.identities.Add(args.Profile.IdaamProviderId, new TestIdentity(new List<RoleInstance>(), args.Profile));

            return Task.FromResult(args.Profile.IdaamProviderId);
        }

        public Task<string> BlockUser(string idaamProviderId)
        {
            throw new NotImplementedException();
        }

        public Task<string> UnblockUser(string idaamProviderId)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateUserProfile(string idaamProvider, IIdaamProvider.UpdateUserArgs updateUserArgs)
        {
            throw new NotImplementedException();
        }

        public Task ChangeUserPassword(string idaamProviderId, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityClaims> GetAppropriateClaimsFromAccessToken(string accessToken, ApiMessage apiMessage, ISecurityInfo securityInfo)
        {
            var exists = this.identities.Any(x => x.Value.AccessToken == accessToken);

            Guard.Against(!exists, "No identity was found with this accessToken");

            var identity = this.identities.Single(x => x.Value.AccessToken == accessToken).Value;

            return Task.FromResult(ClaimsExtractor.GetAppropriateClaimsFromAccessToken(securityInfo, identity, apiMessage));
        }

      
        public Task<IIdaamProvider.User> GetLimitedUserProfileFromIdentityToken(string idToken)
        {
            var exists = this.identities.Any(x => x.Value.IdToken(this.bootstrapVariables.EncryptionKey) == idToken);

            Guard.Against(!exists, "No identity was found with this idToken");

            return Task.FromResult(
                this.identities.Single(x => x.Value.IdToken(this.bootstrapVariables.EncryptionKey) == idToken)
                    .Value.UserProfile.Map(
                        x => new IIdaamProvider.User
                        {
                            Email = x.Email,
                            FirstName = x.FirstName,
                            LastName = x.LastName,
                            IdaamProviderId = x.IdaamProviderId
                        }));
        }

        public Task RemoveRoleFromUser(string idaamProviderUserId, Role role)
        {
            Guard.Against(!this.identities.ContainsKey(idaamProviderUserId), $"An IDAAM user with ID {idaamProviderUserId} does not exist.");

            this.identities[idaamProviderUserId].Roles.RemoveAll(x => x.RoleKey == role.Key);

            return Task.CompletedTask;
        }

        public Task RemoveScopeFromUserRole(string idaamProviderUserId, Role role, AggregateReference scopeReferenceToRemove)
        {
            Guard.Against(!this.identities.ContainsKey(idaamProviderUserId), $"An IDAAM user with ID {idaamProviderUserId} does not exist.");

            var identity = this.identities[idaamProviderUserId];

            Guard.Against(!identity.Roles.Exists(x => x.RoleKey == role.Key), "User does not have the role the requested change is for");

            identity.Roles.Single(x => x.RoleKey == role.Key).ScopeReferences.RemoveAll(x => x == scopeReferenceToRemove);

            return Task.CompletedTask;
        }

        public Task RemoveUser(string idaamProviderId)
        {
            this.identities.Remove(idaamProviderId);

            return Task.CompletedTask;
        }

        public Task<List<RoleInstance>> GetRolesForAUser(string idaamProviderUserId)
        {
            Guard.Against(!this.identities.ContainsKey(idaamProviderUserId), $"An IDAAM user with ID {idaamProviderUserId} does not exist.");

            var identity = this.identities[idaamProviderUserId];

            return Task.FromResult(identity.Roles);
        }
    }
}