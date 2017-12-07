namespace Palmtree.Api.Sso.Endpoint.Http.Handlers
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.HttpEndpointBase;

    public class RevokeAuthTokenHandler : CommandHandler<RevokeAuthToken>
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