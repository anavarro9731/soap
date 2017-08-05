namespace Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.Sample.Api.Domain.Logic.Operations;
    using Palmtree.Sample.Api.Domain.Messages.Commands;

    public class EnableUserHandler : MessageHandler<EnableUser>
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
