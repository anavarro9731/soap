namespace Palmtree.Sample.Api.Domain.Models.ValueObjects
{
    using DataStore.Interfaces.LowLevel;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;
    using ServiceApi.Interfaces.LowLevel.Permissions;

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

        public System.Collections.Generic.IReadOnlyList<IScopeReference> PermissionScope { get; set; }
    }
}
