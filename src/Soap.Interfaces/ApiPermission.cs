namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using CircuitBoard;

    public class ApiPermission
    {
        public List<string> DeveloperPermissions;

        public string Description;

        public Enumeration Id;
    }

    public static class ApiPermissionExt
    {
        public static string AsAuth0Claim(this ApiPermission apiPermissionGroup, string environmentPartitionKey)
        {
            return (!string.IsNullOrEmpty(environmentPartitionKey) ? environmentPartitionKey + "::" : string.Empty) + "execute:"
                   + Regex.Replace(apiPermissionGroup.Id.Key.ToLower(), "[^a-z0-9.]", string.Empty);
        }
    }
}