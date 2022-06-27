namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard;

    public interface ISecurityInfo 
    {
        List<ApiPermission> ApiPermissions { get; }
        List<Role> BuiltInRoles { get; }
    }

}
