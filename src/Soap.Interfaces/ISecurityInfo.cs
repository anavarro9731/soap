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

    public static class ISecurityInfoExt
    {
        public static ApiPermission ApiPermissionFromEnum(this ISecurityInfo securityInfo, Enumeration apiPermissionEnum) => securityInfo.ApiPermissions.Single(x => x.Id == apiPermissionEnum);   
    }
}
