namespace Soap.Api.Sample.Messages.Events
{
    using Soap.Interfaces.Messages;
    using Soap.Pf.MessageContractsBase;

    public class E150v1Pong : AbstractPongEvent
    {
        public override ApiPermission Permission { get; }
    }
}