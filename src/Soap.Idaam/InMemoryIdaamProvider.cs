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

            var roleInstance = this.identities[idaamProviderUserId].RoleInstances.SingleOrDefault(x => x.RoleKey == role.Key);

            scopeReferencesToAdd ??= new List<AggregateReference>();

            if (roleInstance == null)
            {
                this.identities[idaamProviderUserId]
                    .RoleInstances.Add(
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

        public Task ChangeUserPassword(string idaamProviderId, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityClaims> GetAppropriateClaimsFromAccessToken(string accessToken, ApiMessage apiMessage, ISecurityInfo securityInfo)
        {
            var exists = this.identities.Any(x => x.Value.AccessToken == accessToken);

            Guard.Against(!exists, "No identity was found with this accessToken");

            var identity = this.identities.Single(x => x.Value.AccessToken == accessToken).Value;
            var messageName = apiMessage.GetType().Name;
            var claims = new IdentityClaims();

            foreach (var role in identity.RoleInstances)
            {
                var roleDef = securityInfo.BuiltInRoles.Single(x => x.Key == role.RoleKey);
                var apiPermissionDefsForThisRole = securityInfo.ApiPermissions.Where(x => roleDef.ApiPermissions.Contains(x)).ToList();

                /* the user gets API permissions for ALL roles they have, In production the provider
                currently the service (Auth0) sends us both roles and apipermissions in the token
                and we trust they are consistent and load them independently, but when testing
                if we were to load them from the identity independently we might not have consistency
                so this is the safer way to derive them always from the role */
                
                claims.Roles.Add(role);
                
                claims.ApiPermissions.AddRange(
                    apiPermissionDefsForThisRole.Where(newPermission => claims.ApiPermissions.TrueForAll(a => a != newPermission)));

                /* now we add db permissions from the scope of the roles that this user has for THIS message,
                they will still have all their roles and scopes for review, but only those
                relevant to this message will be used to create data permissions for accessing data.
                there could be cases where the same  message is in two role and in that case we will
                give both sets of scopes because we cannot determine which UI context this was meant from
                but technically if they have access to send that command with that scope it should be allowed */
                if (apiPermissionDefsForThisRole.Any(permission => permission.DeveloperPermissions.Contains(messageName)))
                {
                    /* we give all because when using datastore as a slave framework, the determination about what type of
                     operations you can perform is made implicitly by the developer by which method they call. the exception here
                     would be the readPII which they have no way of specifying from the callsite. Possible we could make this
                     something they could set as another argument when they add the rolescope in future but only for ReadPII or
                     you start to introduce confusion between the two approaches */
                    if (role.ScopeReferences.Any(s => s.AggregateId == Guid.Parse("1EEAF9CB-A2BE-4A08-A5E0-330C63D1D81F")))
                    {
                        if (claims.DatabasePermissions.TrueForAll(p => p.PermissionName != "*"))
                            claims.DatabasePermissions.Add(new DatabasePermission("*", new List<AggregateReference>()));
                    }
                    else
                    {
                        foreach (var aggregateReference in role.ScopeReferences)
                        {   
                            //ADD OR MERGE AND TEST
                        }
                        claims.DatabasePermissions.Add(new DatabasePermission(SecurableOperations.READ, role.ScopeReferences));
                        claims.DatabasePermissions.Add(new DatabasePermission(SecurableOperations.READPII, role.ScopeReferences));
                        claims.DatabasePermissions.Add(new DatabasePermission(SecurableOperations.UPDATE, role.ScopeReferences));
                        claims.DatabasePermissions.Add(new DatabasePermission(SecurableOperations.DELETE, role.ScopeReferences));
                        claims.DatabasePermissions.Add(new DatabasePermission(SecurableOperations.CREATE, role.ScopeReferences));
                    }
                }
            }

            return Task.FromResult(claims);
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

            this.identities[idaamProviderUserId].RoleInstances.RemoveAll(x => x.RoleKey == role.Key);

            return Task.CompletedTask;
        }

        public Task RemoveScopeFromUserRole(string idaamProviderUserId, Role role, AggregateReference scopeReferenceToRemove)
        {
            Guard.Against(!this.identities.ContainsKey(idaamProviderUserId), $"An IDAAM user with ID {idaamProviderUserId} does not exist.");

            var identity = this.identities[idaamProviderUserId];

            Guard.Against(!identity.RoleInstances.Exists(x => x.RoleKey == role.Key), "User does not have the role the requested change is for");

            identity.RoleInstances.Single(x => x.RoleKey == role.Key).ScopeReferences.RemoveAll(x => x == scopeReferenceToRemove);

            return Task.CompletedTask;
        }

        public Task RemoveUser(string idaamProviderId)
        {
            this.identities.Remove(idaamProviderId);

            return Task.CompletedTask;
        }
    }
}