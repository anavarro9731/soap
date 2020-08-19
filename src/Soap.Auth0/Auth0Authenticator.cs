namespace Soap.Auth0
{
    using System;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class Auth0Authenticator : IAuthenticate
    {
        private readonly Func<IApiIdentity> createIdentity;

        public Auth0Authenticator(Func<IApiIdentity> createIdentity)
        {
            this.createIdentity = createIdentity;
        }

        public IApiIdentity Authenticate(ApiMessage message)
        {
            var x = this.createIdentity();
            x.UserName = "john.doe";
            return x;
        }
    }
}