namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using Soap.Interfaces.Messages;

    public class ApiPermissionGroup
    {
        public List<ApiPermission> ApiPermissions;

        public string Name;
    }
}