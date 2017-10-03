namespace Soap.MessagePipeline.Permissions
{
    using System;
    using System.Collections.Generic;
    using ServiceApi.Interfaces.LowLevel.Permissions;

    /// <summary>
    ///     a way to pull the derived class out of the container, rathen than using the base class
    /// </summary>
    public interface IPermissionList
    {
        List<IApplicationPermission> ToList { get; }

        IApplicationPermission GetById(Guid permissionId);
    }
}
