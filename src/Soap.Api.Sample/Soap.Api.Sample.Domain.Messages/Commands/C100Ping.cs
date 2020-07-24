namespace Sample.Messages.Commands
{
    using Soap.Interfaces.Messages;
    using Soap.Pf.MessageContractsBase;

    public class C100Ping : AbstractPingCommand
    {
        public override ApiPermission Permission { get; }
    }
}