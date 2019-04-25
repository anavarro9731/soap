namespace Soap.Api.Sso.Endpoint.Http.Handlers
{
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Logic.Operations;
    using Soap.Api.Sso.Domain.Messages.Commands;
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
            await this.userOperations.RevokeAuthToken(meta.RequestedBy.id, message.AuthToken);
        }
    }
}