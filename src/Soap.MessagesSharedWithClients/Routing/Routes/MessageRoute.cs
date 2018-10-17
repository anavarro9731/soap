namespace Soap.Pf.ClientServerMessaging.Routing
{
    using Soap.If.Interfaces.Messages;

    public abstract partial class MessageRoute
    {
        public abstract bool CanRouteMessage(IApiMessage message);

        protected abstract string From { get; }

        protected abstract string To { get; }

        public override string ToString()
        {
            return $"{From}->{To}";
        }
    }
}