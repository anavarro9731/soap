namespace Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.Sample.Api.Domain.Logic.Operations;
    using Palmtree.Sample.Api.Domain.Messages.Commands;

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
