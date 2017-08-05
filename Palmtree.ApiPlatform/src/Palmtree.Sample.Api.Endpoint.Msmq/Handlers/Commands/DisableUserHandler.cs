namespace Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.Sample.Api.Domain.Logic.Operations;
    using Palmtree.Sample.Api.Domain.Messages.Commands;

    public class DisableUserHandler : MessageHandler<DisableUser>
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
