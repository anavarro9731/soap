namespace Soap.Interfaces
{
    using DataStore.Interfaces.LowLevel.Permissions;

    public interface IApiIdentity : IIdentityWithApiPermissionGroups, IIdentityWithDatabasePermissions, IHaveRoles
    {
        string UserName { get; set; }
    }
}