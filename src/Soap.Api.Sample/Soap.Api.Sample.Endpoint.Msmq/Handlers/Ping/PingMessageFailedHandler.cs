namespace Soap.Api.Sample.Endpoint.Msmq.Handlers.Ping
{
    using Soap.Api.Sample.Domain.Messages.Ping;
    using Soap.Pf.DomainLogicBase;
    using Soap.Pf.MessageContractsBase.Commands;
    using Soap.Pf.MsmqEndpointBase.Handlers.Commands;

    public class PingMessageFailedHandler : AbstractPingMessageFailedHandlerForMsmq<MessageFailedAllRetries<MsmqPingCommand>, MsmqPingCommand>
    {
        public PingMessageFailedHandler(MessageFailedAllRetriesLogItemOperations failedMessageLogItemOperations)
            : base(failedMessageLogItemOperations)
        {
        }
    }
}