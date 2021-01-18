namespace Soap.Interfaces
{
    using Soap.Interfaces.Messages;

    /// <summary>
    ///     allows people to write their own method of authenticating a user from a message
    ///     the method should set the IUserWithPermissions property on IApiMessage using the
    ///     AuhthenticationToken to do so
    ///
    ///     At present its not awaitable, as we are trying to avoid a database
    ///     call and get all data from token for best performance
    /// </summary>

    public interface IAuthenticate {
        IApiIdentity Authenticate(ApiMessage message);
    }
}
