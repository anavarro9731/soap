namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using Soap.Interfaces.Messages;

    public class RevokeAuthToken : ApiCommand
    {
        public RevokeAuthToken(string authToken)
        {
            AuthToken = authToken;
        }

        public string AuthToken { get; }
    }
}