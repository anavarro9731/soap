namespace Soap.Interfaces
{
    using CircuitBoard.Permissions;
    using Soap.Interfaces.Messages;

    /// <summary>
    ///     allows people to write their own method of authenticating a user from a message
    ///     the method should set the IUserWithPermissions property on IApiMessage using the
    ///     AuhthenticationToken to do so
    /// </summary>
    public interface IAuthenticateUsers
    {
        IIdentityWithPermissions Authenticate(ApiMessage message);
    }
}