namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Soap.Interfaces.Messages;

    public class ApiPermissionGroup
    {
        public List<string> ApiPermissions;

        public string Description;
        
        public string Name;

        public Guid Id;
    }

    public static class ApiPermissionGroupExt
    {
        public static string AsClaim(this ApiPermissionGroup apiPermissionGroup) =>
            "execute:" + Regex.Replace(apiPermissionGroup.Name.ToLower(), "[^a-z0-9.]", "");
    }
}
