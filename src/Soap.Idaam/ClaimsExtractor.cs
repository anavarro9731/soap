namespace Soap.Idaam
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class ClaimsExtractor
    {
        public static IdentityClaims GetAppropriateClaimsFromAccessToken(ISecurityInfo securityInfo, IHaveRoles identity, ApiMessage apiMessage)
        {
            var messageName = apiMessage.GetType().Name;
            var claims = new IdentityClaims();

            foreach (var role in identity.Roles)
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
                        {
                            claims.DatabasePermissions.Add(new DatabasePermission("*", new List<AggregateReference>()));
                        }
                    }

                    foreach (var aggregateReference in role.ScopeReferences)
                    {
                        AddIfNotExists(role, aggregateReference, SecurableOperations.READ);
                        AddIfNotExists(role, aggregateReference, SecurableOperations.READPII);
                        AddIfNotExists(role, aggregateReference, SecurableOperations.UPDATE);
                        AddIfNotExists(role, aggregateReference, SecurableOperations.DELETE);
                        AddIfNotExists(role, aggregateReference, SecurableOperations.CREATE);
                    }
                }
            }

            return claims;

            void AddIfNotExists(RoleInstance role, AggregateReference aggregateReference, string operation)
            {
                var existingPerm = claims.DatabasePermissions.SingleOrDefault(x => { return x.PermissionName == operation; });
                if (existingPerm == null)
                {
                    claims.DatabasePermissions.Add(new DatabasePermission(operation, role.ScopeReferences));
                }
                else
                {
                    var existingScopeRef = existingPerm.ScopeReferences.SingleOrDefault(x => x.AggregateId == aggregateReference.AggregateId);
                    if (existingScopeRef == null)
                    {
                        existingPerm.ScopeReferences.Add(aggregateReference);
                    }
                }
            }
        }
    }
}