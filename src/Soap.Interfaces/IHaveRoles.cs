namespace Soap.Interfaces
{
    using System.Collections.Generic;

    public interface IIdentityWithRoles
    {
        List<RoleInstance> Roles { get; set; }
    }
}