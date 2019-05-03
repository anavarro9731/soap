namespace Soap.Api.Sso.Endpoint.Msmq.Handlers.Ping
{
    using Soap.Api.Sso.Domain.Messages.Ping;
    using Soap.Pf.DomainLogicBase;
    using Soap.Pf.MessageContractsBase.Commands;
    using Soap.Pf.MsmqEndpointBase.Handlers;
    using Soap.Pf.MsmqEndpointBase.Handlers.Commands;

    public class PingMessageFailedHandler : AbstractPingMessageFailedHandlerForMsmq<MessageFailedAllRetries<MsmqPingCommand>, MsmqPingCommand>
    {
        public PingMessageFailedHandler(MessageFailedAllRetriesLogItemOperations failedMessageLogItemOperations)
            : base(failedMessageLogItemOperations)
        {
        }
    }
}