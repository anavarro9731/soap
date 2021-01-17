namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using Soap.Interfaces.Messages;

    public class ApiPermissionGroup
    {
        public List<string> ApiPermissions;

        public string Description;
        
        public string Name;
    }
}
