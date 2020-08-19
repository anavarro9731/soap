﻿namespace Soap.MessagePipeline.Permissions
{
    using System.Linq;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Operations;

    // ReSharper disable once InconsistentNaming
    public static class IIdentityWithApiPermissionsExtensions
    {
        public static void NotHavingPermissionToExecute(
            this IIdentityWithApiPermissionGroups identity,
            ApiMessage message,
            string userName)
        {
            Guard.Against(
                identity == null || identity.ApiPermissionGroups.All(pg => !pg.ApiPermissions.Contains(message.Permission)),
                $"{userName} does not have permission: {message.Permission.PermissionName}");
        }
    }
}