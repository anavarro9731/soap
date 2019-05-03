namespace Soap.Api.Sample.Domain.Messages.Ping
{
    using Soap.Pf.MessageContractsBase.Commands;

    public class HttpPingCommand : AbstractPingCommandForHttp<HttpPingCommand.PongViewModel>
    {
        public HttpPingCommand()
        {
        }

        public HttpPingCommand(string pingedBy)
            : base(pingedBy)
        {
        }

        public class PongViewModel : AbstractResponseModel
        {
        }
    }
}