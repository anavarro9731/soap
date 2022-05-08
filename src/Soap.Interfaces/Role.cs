namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using CircuitBoard;

    public class Role
    {
        public List<Enumeration> ApiPermissions;

        public string Description;

        public Enumeration Id;

        
    }

    public static class RoleExt
    {
        public static string AsAuth0Name(this Role role, string environmentPartitionKey)
        {
            return (!string.IsNullOrEmpty(environmentPartitionKey) ? environmentPartitionKey + "::" : string.Empty) + "builtin:"
                   + Regex.Replace(role.Id.Key.ToLower(), "[^a-z0-9./]", string.Empty);
        }
        
        
    }

    // public class RoleInstance : IIdentityWithApiPermissions, IIdentityWithDatabasePermissions
    // {
    //     public string Name;
    //
    //     public Role Role { get; set; }
    //     
    //     public List<DatabasePermissionInstance> DatabasePermissions { get; set; }
    //
    //     public List<string> ApiPermissions { get; set; }
    //
    //     public List<DatabaseScopeReference> ScopeReferences { get; set; }
    // }
}