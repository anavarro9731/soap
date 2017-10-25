namespace Palmtree.Api.Sso.Domain.Models.ValueObjects
{
    using System.Collections.Generic;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Permissions;
    using DataStore.Interfaces.LowLevel;

    public class UserPermission
    {
        public UserPermission(IApplicationPermission permission, IScopeReference[] permissionScope)
        {
            Permission = permission;

            var scopeReferences = new ReadOnlyCapableList<IScopeReference>();
            scopeReferences.AddRange(permissionScope);
            PermissionScope = scopeReferences;
        }

        public IApplicationPermission Permission { get; set; }

        public IReadOnlyList<IScopeReference> PermissionScope { get; set; }
    }
}