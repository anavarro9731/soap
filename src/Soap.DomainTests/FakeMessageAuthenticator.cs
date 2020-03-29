namespace Soap.DomainTests
{
    using CircuitBoard.Permissions;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    internal class FakeMessageAuthenticator : IAuthenticateUsers
    {
        private readonly IIdentityWithPermissions identity;

        public FakeMessageAuthenticator(IIdentityWithPermissions identity)
        {
            this.identity = identity;
        }

        public IIdentityWithPermissions Authenticate(ApiMessage message)
        {
            return this.identity;
        }
    }
}