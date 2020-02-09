namespace Soap.MessagePipeline.Permissions
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard.Permissions;

    /// <summary>
    ///     a way to pull the derived class out of the container, rather than using the base class
    /// </summary>
    public interface IPermissionList
    {
        List<IPermission> ToList { get; }

        IPermission GetById(Guid permissionId);
    }
}