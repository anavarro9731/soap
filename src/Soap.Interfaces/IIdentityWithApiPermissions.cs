namespace Soap.Interfaces
{
    using System.Collections.Generic;

    public interface IIdentityWithApiPermissions
    {
        List<ApiPermission> ApiPermissions { get; set; }
    }
}
