namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;

    public class IdentityClaims : IIdentityWithApiPermissions, IIdentityWithDatabasePermissions, IIdentityWithRoles
    {
        public List<ApiPermission> ApiPermissions { get; set; } = new List<ApiPermission>();

        public List<DatabasePermission> DatabasePermissions { get; set; } = new List<DatabasePermission>();

        public List<RoleInstance> Roles { get; set; } = new List<RoleInstance>();
    }
    
}
