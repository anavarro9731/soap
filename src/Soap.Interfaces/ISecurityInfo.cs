namespace Soap.Interfaces
{
    using System.Collections.Generic;

    public interface ISecurityInfo 
    {
        List<ApiPermissionGroup> PermissionGroups { get; }

    }
}
