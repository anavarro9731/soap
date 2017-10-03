namespace Soap.Interfaces
{
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using ServiceApi.Interfaces.LowLevel.Permissions;

    /// <summary>
    ///     allows people to write their own method of authenticating a user from a message
    ///     the method should set the IUserWithPermissions property on IApiMessage using the
    ///     AuhthenticationToken to do so
    /// </summary>
    public interface IAuthenticateUsers
    {
        IUserWithPermissions Authenticate(IApiMessage message);
    }
}
