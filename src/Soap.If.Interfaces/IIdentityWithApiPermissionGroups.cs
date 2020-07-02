namespace Soap.Interfaces
{
    using System.Collections.Generic;

    public interface IIdentityWithApiPermissionGroups
    {
        List<ApiPermissionGroup> ApiPermissionGroups { get; set; }

        
    }
}