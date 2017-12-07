namespace Palmtree.Api.Sso.Endpoint.Http.Handlers
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.HttpEndpointBase;

    public class RevokeAllAuthTokenHandler : CommandHandler<RevokeAllAuthTokens>
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