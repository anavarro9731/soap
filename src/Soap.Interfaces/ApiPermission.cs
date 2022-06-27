namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using CircuitBoard;

    public class ApiPermission: Enumeration
    {
        public ApiPermission() {}
        
        public ApiPermission(string key, string value)
        {
            base.Key = key;
            base.Value = value;
        }
        public List<string> DeveloperPermissions;

        public string Description;
        
    }
    
    public static class ApiPermissionExt
    {
        
        public static string AsAuth0Claim(this ApiPermission apiPermission, string environmentPartitionKey)
        {
            return (!string.IsNullOrEmpty(environmentPartitionKey) ? environmentPartitionKey + "::" : string.Empty) 
                   + Regex.Replace(apiPermission.Key.ToLower(), "[^a-z0-9./-]", string.Empty);
        }
    }
}