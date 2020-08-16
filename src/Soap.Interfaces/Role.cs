namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;

    public class Role : IIdentityWithApiPermissionGroups, IIdentityWithDatabasePermissions
    {
        public string Name;

        public List<ApiPermissionGroup> ApiPermissionGroups { get; set; }

        public List<DatabasePermissionInstance> DatabasePermissions { get; set; }
    }
}