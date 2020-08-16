namespace Soap.PfBase.Tests
{
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    internal class FakeMessageAuthenticator : IAuthenticate
    {
        private readonly IApiIdentity identity;

        public FakeMessageAuthenticator(IApiIdentity identity)
        {
            this.identity = identity;
        }

        public IApiIdentity Authenticate(ApiMessage message) => this.identity;
    }
}