namespace Soap.Interfaces
{
    using Soap.Interfaces.Messages;

    /// <summary>
    ///     allows people to write their own method of authenticating a user from a message
    ///     the method should set the IUserWithPermissions property on IApiMessage using the
    ///     AuhthenticationToken to do so
    /// </summary>
    ///
    ///
    /// 
    public interface IAuthenticate {
        IApiIdentity Authenticate(ApiMessage message);
    }
}