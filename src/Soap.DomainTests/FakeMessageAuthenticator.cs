namespace Soap.DomainTests
{
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;

    internal class FakeMessageAuthenticator : IAuthenticateUsers
    {
        private readonly IApiIdentity identity;

        public FakeMessageAuthenticator(IApiIdentity identity)
        {
            this.identity = identity;
        }

        public IApiIdentity Authenticate(ApiMessage message)
        {
            return this.identity;
        }
    }
}