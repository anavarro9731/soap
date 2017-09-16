namespace Palmtree.Sample.Api.Domain.Messages.Commands
{
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class RevokeAuthToken : ApiCommand
    {
        public RevokeAuthToken(string authToken)
        {
            AuthToken = authToken;
        }

        public string AuthToken { get; }
    }
}
