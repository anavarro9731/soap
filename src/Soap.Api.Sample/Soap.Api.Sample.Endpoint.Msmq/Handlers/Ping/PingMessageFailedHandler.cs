namespace Soap.Api.Sample.Endpoint.Msmq.Handlers.Ping
{
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.If.Interfaces;
    using Soap.Pf.DomainLogicBase;
    using Soap.Pf.MsmqEndpointBase.Handlers;

    public class PingMessageFailedHandler : AbstractPingMessageFailedHandler<MessageFailedAllRetries<PingCommand>, PingCommand, PingCommand.PongViewModel>
    {
        public PingMessageFailedHandler(MessageFailedAllRetriesLogItemOperations failedMessageLogItemOperations)
            : base(failedMessageLogItemOperations)
        {
        }
    }
}