namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class ApiPermissionGroup
    {
        public List<string> ApiPermissions;

        public string Description;

        public Guid Id;

        public string Name;
    }

    public static class ApiPermissionGroupExt
    {
        public static string AsEnvironmentPartitionAwareClaim(this ApiPermissionGroup apiPermissionGroup, string environmentPartitionKey) =>
            (!string.IsNullOrEmpty(environmentPartitionKey) ? environmentPartitionKey + "::" : string.Empty) + "execute:"
            + Regex.Replace(apiPermissionGroup.Name.ToLower(), "[^a-z0-9.]", "") + ":"
            + apiPermissionGroup.Id.ToString().ToLower();
    }
}
