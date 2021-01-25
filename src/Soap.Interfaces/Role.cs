﻿namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;

    public class Role : IIdentityWithApiPermissions, IIdentityWithDatabasePermissions
    {
        public string Name;
        
        public List<DatabasePermissionInstance> DatabasePermissions { get; set; }

        public List<string> ApiPermissions { get; set; }
    }
}
