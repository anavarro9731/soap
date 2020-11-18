namespace Soap.Api.Sample.Messages.Commands
{
    using Soap.Interfaces.Messages;
    using Soap.Pf.MessageContractsBase;

    public class C100v1Ping : AbstractPingCommand
    {
        public override ApiPermission Permission { get; }
    }
}