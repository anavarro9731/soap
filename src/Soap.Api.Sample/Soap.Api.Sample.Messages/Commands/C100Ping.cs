namespace Soap.Api.Sample.Messages.Commands
{
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Messages;

    public class C100Ping : AbstractPingCommand
    {
        public override ApiPermission Permission { get; }
    }
}