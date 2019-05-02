namespace Soap.Api.Sso.Domain.Messages.Ping
{
    using Soap.Pf.MessageContractsBase.Commands;

    public class PingCommand : AbstractPingCommand<PingCommand.PongViewModel>
    {
        public PingCommand()
        {
        }

        public PingCommand(string pingedBy)
            : base(pingedBy)
        {
        }

        public class PongViewModel : AbstractResponseModel
        {
        }
    }
}