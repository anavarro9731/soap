namespace Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Interfaces;
    using Palmtree.Sample.Api.Domain.Logic.Operations;
    using Palmtree.Sample.Api.Domain.Messages.Commands;

    internal class PingMessageFailedHandler : MessageHandler<MessageFailedAllRetries<PingCommand>>
    {
        private readonly MessageFailedAllRetriesLogItemOperations operations;

        public PingMessageFailedHandler(MessageFailedAllRetriesLogItemOperations operations)
        {
            this.operations = operations;
        }

        protected override async Task Handle(MessageFailedAllRetries<PingCommand> message, ApiMessageMeta meta)
        {
            await this.operations.AddLogItem(message.IdOfMessageThatFailed);
        }
    }
}
