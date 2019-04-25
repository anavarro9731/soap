namespace Soap.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Logic.Operations;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.MsmqEndpointBase;

    public class DisableUserHandler : CommandHandler<DisableUser>
    {
        private readonly UserOperations userOperations;

        public DisableUserHandler(UserOperations userOperations)
        {
            this.userOperations = userOperations;
        }

        protected override async Task Handle(DisableUser message, ApiMessageMeta meta)
        {
            await this.userOperations.DisableUserAccount(message.IdOfUserToDisable);
        }
    }
}