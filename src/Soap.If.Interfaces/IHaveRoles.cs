namespace Soap.Interfaces
{
    using System.Collections.Generic;

    public interface IHaveRoles
    {
        List<Role> Roles { get; set; }
    }
}