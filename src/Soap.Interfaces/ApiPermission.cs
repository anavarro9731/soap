namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using CircuitBoard;

    public class ApiPermission
    {
        public List<string> DeveloperPermissions;

        public string Description;

        public ApiPermissionId Id;
    }
    
    public class ApiPermissionId : TypedEnumeration<ApiPermissionId>
    {
        public ApiPermissionId() {}
        
        public ApiPermissionId(string key, string value)
        {
            base.Key = key;
            base.Value = value;
        }
    }

    public static class ApiPermissionExt
    {
        
        public static string AsAuth0Claim(this ApiPermissionId apiPermission, string environmentPartitionKey)
        {
            return (!string.IsNullOrEmpty(environmentPartitionKey) ? environmentPartitionKey + "::" : string.Empty) 
                   + Regex.Replace(apiPermission.Key.ToLower(), "[^a-z0-9./-]", string.Empty);
        }
        
        public static string AsAuth0Claim(this ApiPermission apiPermission, string environmentPartitionKey)
        {
            return (!string.IsNullOrEmpty(environmentPartitionKey) ? environmentPartitionKey + "::" : string.Empty) 
                   + Regex.Replace(apiPermission.Id.Key.ToLower(), "[^a-z0-9./-]", string.Empty);
        }
    }
}