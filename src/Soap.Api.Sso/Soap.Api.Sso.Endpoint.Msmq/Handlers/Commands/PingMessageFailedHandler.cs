namespace Soap.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Logic.Operations;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.MsmqEndpointBase;

    internal class PingMessageFailedHandler : CommandHandler<MessageFailedAllRetries<PingCommand>>
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