namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using CircuitBoard;
    using DataStore.Interfaces.LowLevel.Permissions;

    public class Role
    {
        public List<ApiPermissionId> ApiPermissions;

        public string Description;

        public RoleId Id;
        
    }
    
    public class RoleId : TypedEnumeration<RoleId>
    {
        public RoleId() {}
        
        public RoleId(string key, string value)
        {
            base.Key = key;
            base.Value = value;
        }
        
    }

    public static class RoleExt
    {
        public static string AsAuth0Name(this RoleId role, string environmentPartitionKey)
        {
            return (!string.IsNullOrEmpty(environmentPartitionKey) ? environmentPartitionKey + "::" : string.Empty) + "builtin:"
                   + Regex.Replace(role.Key.ToLower(), "[^a-z0-9./-]", string.Empty);
        }
        
        public static string AsAuth0Name(this Role role, string environmentPartitionKey)
        {
            return (!string.IsNullOrEmpty(environmentPartitionKey) ? environmentPartitionKey + "::" : string.Empty) + "builtin:"
                   + Regex.Replace(role.Id.Key.ToLower(), "[^a-z0-9./-]", string.Empty);
        }
    }

    public class RoleInstance
    {
        public RoleId RoleId { get; set; }
        
        public List<DatabaseScopeReference> ScopeReferences { get; set; }
    }
}