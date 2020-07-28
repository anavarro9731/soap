﻿namespace Sample.Models.Aggregates
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;
    using Newtonsoft.Json;
    using Soap.Interfaces;

    public class User : Aggregate, IApiIdentity
    {
        public List<ApiPermissionGroup> ApiPermissionGroups { get; set; }

        public List<DatabasePermissionInstance> DatabasePermissions { get; set; }

        public List<Role> Roles { get; set; }

        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}