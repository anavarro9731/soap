namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;

    public class ApiIdentity : IIdentityWithApiPermissions, IIdentityWithDatabasePermissions
    {
        public string Id { get; set; }

        public List<string> ApiPermissions { get; set; } = new List<string>();

        public List<DatabasePermissionInstance> DatabasePermissions { get; set; } = new List<DatabasePermissionInstance>();

    }
}
