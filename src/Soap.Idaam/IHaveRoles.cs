namespace Soap.Idaam
{
    using System.Collections.Generic;
    using Soap.Interfaces;

    public interface IHaveRoles
    {
        List<RoleInstance> Roles { get; }
    }
}