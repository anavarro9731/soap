namespace Sample.Messages.Events
{
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Messages;

    public class E150Pong : AbstractPongEvent
    {
        public override ApiPermission Permission { get; }
    }
}