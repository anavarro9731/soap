namespace Soap.Api.Sso.Endpoint.Msmq.Handlers.Ping
{
    using Soap.Api.Sso.Domain.Messages.Ping;
    using Soap.Pf.DomainLogicBase;
    using Soap.Pf.MessageContractsBase.Commands;
    using Soap.Pf.MsmqEndpointBase.Handlers;

    //TODO write two endpoint tests for this
    public class PingMessageFailedHandler : AbstractPingMessageFailedHandler<MessageFailedAllRetries<PingCommand>, PingCommand, PingCommand.PongViewModel>
    {
        public PingMessageFailedHandler(MessageFailedAllRetriesLogItemOperations failedMessageLogItemOperations)
            : base(failedMessageLogItemOperations)
        {
        }
    }
}