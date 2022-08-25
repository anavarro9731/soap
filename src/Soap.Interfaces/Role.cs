namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using CircuitBoard;
    using DataStore.Interfaces.LowLevel.Permissions;

    public class Role : Enumeration
    {
        public List<ApiPermission> ApiPermissions = new();

        public string Description;

        internal readonly string apiIdOfServiceThatOwnsThisRole;
        
        public Role()
        {
        }

        public Role(string key, string value, string apiId = null)
        {
            Key = key.ToLower();
            Value = value;
            this.apiIdOfServiceThatOwnsThisRole = apiId;
        }
    }

    public static class RoleExt
    {
        public static string AsAuth0Name(this Role role, string environmentPartitionKey, string apiIdOfHostService)
        {
            return (!string.IsNullOrEmpty(environmentPartitionKey) ? environmentPartitionKey + "::" : string.Empty) 
                   + (!string.IsNullOrEmpty(role.apiIdOfServiceThatOwnsThisRole) ? role.apiIdOfServiceThatOwnsThisRole : apiIdOfHostService) + ":builtin:"
                   + Regex.Replace(role.Key.ToLower(), "[^a-z0-9./-]", string.Empty);
        }
    }

    public class RoleInstance
    {
        public string RoleKey { get; set; }

        //* each role has a list of API permissions. the scope added to the role will apply for any API permission in the role
        public List<AggregateReference> ScopeReferences { get; set; } = new();
    }
}