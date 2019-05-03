namespace Soap.Api.Sample.Domain.Messages.Ping
{
    using Soap.Pf.MessageContractsBase.Commands;

    public class MsmqPingCommand : AbstractPingCommandForMsmq<PongCommand>
    {
        public MsmqPingCommand()
        {
        }

        public MsmqPingCommand(string pingedBy)
            : base(pingedBy)
        {
        }
    }
}