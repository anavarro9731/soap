namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Models;

    public class RevokeAuthTokenHandler : MessageHandler<RevokeAuthToken>
    {
        private readonly UserOperations userOperations;

        public RevokeAuthTokenHandler(UserOperations userOperations)
        {
            this.userOperations = userOperations;
        }

        protected override async Task Handle(RevokeAuthToken message, ApiMessageMeta meta)
        {
            await this.userOperations.RevokeAuthToken(message, meta);
        }
    }
}