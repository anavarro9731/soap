namespace Soap.Auth0
{
    using System;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class Auth0Authenticator<TApiIdentity> : IAuthenticate where TApiIdentity : IApiIdentity, new()
    {
        private readonly string identityToken;

        private readonly string accessToken;

        public Auth0Authenticator(string identityToken, string accessToken)
        {
            this.identityToken = identityToken;
            this.accessToken = accessToken;
        }

        public IApiIdentity Authenticate(ApiMessage message)
        {
            //TODO turn tokens into user
            var x = new TApiIdentity
            {
                UserName = ""
            };

            return x;
        }
    }
}
