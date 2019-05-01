namespace Soap.Pf.MsmqEndpointBase.Handlers
{
    using System.Threading.Tasks;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.DomainLogicBase;
    using Soap.Pf.MessageContractsBase.Commands;
    using Soap.Pf.MsmqEndpointBase;

    public class AbstractPingMessageFailedHandler<TPingFailed, TPing, TPingResponseViewModel> : CommandHandler<TPingFailed>
        where TPingFailed : MessageFailedAllRetries<TPing>, new()
        where TPing : AbstractPingCommand<TPingResponseViewModel>, new()
        where TPingResponseViewModel : AbstractPingCommand<TPingResponseViewModel>.AbstractResponseModel, new()

    {
        private readonly MessageFailedAllRetriesLogItemOperations failedMessageLogItemOperations;

        public AbstractPingMessageFailedHandler(MessageFailedAllRetriesLogItemOperations failedMessageLogItemOperations)
        {
            this.failedMessageLogItemOperations = failedMessageLogItemOperations;
        }

        protected override async Task Handle(TPingFailed message, ApiMessageMeta meta)
        {
            await this.failedMessageLogItemOperations.AddLogItem(message.IdOfMessageThatFailed);
        }
    }
}