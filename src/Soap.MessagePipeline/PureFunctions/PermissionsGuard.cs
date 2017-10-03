namespace Soap.MessagePipeline.PureFunctions
{
    using ServiceApi.Interfaces.LowLevel.Permissions;
    using Soap.Utility.PureFunctions;

    public static class PermissionsGuard
    {
        public static UserPermissionCheck AgainstUser(IUserWithPermissions user)
        {
            return new UserPermissionCheck(user);
        }

        public sealed class UserPermissionCheck
        {
            private readonly IUserWithPermissions user;

            public UserPermissionCheck(IUserWithPermissions user)
            {
                this.user = user;
            }

            public void NotHavingPermissionTo(IApplicationPermission permission)
            {
                Guard.Against(
                    this.user == null || this.user.HasPermission(permission) == false,
                    $"{this.user?.UserName ?? "Anonymous user"} does not have permission: {permission.PermissionName}");
            }
        }
    }
}
