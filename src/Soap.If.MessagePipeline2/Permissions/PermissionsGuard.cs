﻿namespace Soap.If.MessagePipeline.Permissions
{
    using CircuitBoard.Permissions;
    using Soap.If.Utility.PureFunctions;

    public static class PermissionsGuard
    {
        public static UserPermissionCheck AgainstUser(IIdentityWithPermissions user)
        {
            return new UserPermissionCheck(user);
        }

        public sealed class UserPermissionCheck
        {
            private readonly IIdentityWithPermissions user;

            public UserPermissionCheck(IIdentityWithPermissions user)
            {
                this.user = user;
            }

            public void NotHavingPermissionTo(IPermission permission)
            {
                Guard.Against(
                    this.user == null || this.user.HasPermission(permission) == false,
                    $"{this.user?.UserName ?? "Anonymous user"} does not have permission: {permission.PermissionName}");
            }
        }
    }
}