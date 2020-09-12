namespace Soap.Api.Sample.Models.Aggregates
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;
    using Soap.Interfaces;

    public class User : Aggregate, IApiIdentity
    {
        public List<ApiPermissionGroup> ApiPermissionGroups { get; set; } = new List<ApiPermissionGroup>();

        public List<DatabasePermissionInstance> DatabasePermissions { get; set; } = new List<DatabasePermissionInstance>();

        public List<Role> Roles { get; set; } = new List<Role>();

        public virtual string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}