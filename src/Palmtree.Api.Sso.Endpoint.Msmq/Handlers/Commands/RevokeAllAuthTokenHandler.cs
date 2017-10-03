namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Models;

    public class RevokeAllAuthTokenHandler : MessageHandler<RevokeAllAuthTokens>
    {
        private readonly UserOperations userOperations;

        public RevokeAllAuthTokenHandler(UserOperations userOperations)
        {
            this.userOperations = userOperations;
        }

        protected override async Task Handle(RevokeAllAuthTokens message, ApiMessageMeta meta)
        {
            await this.userOperations.RevokeAllAuthTokens(message, meta);
        }
    }
}
