namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.MsmqEndpointBase;

    public class EnableUserHandler : CommandHandler<EnableUser>
    {
        private readonly UserOperations userOperations;

        public EnableUserHandler(UserOperations userOperations)
        {
            this.userOperations = userOperations;
        }

        protected override async Task Handle(EnableUser message, ApiMessageMeta meta)
        {
            await this.userOperations.EnableUserAccount(message.IdOfUserToEnable);
        }
    }
}