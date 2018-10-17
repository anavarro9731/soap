namespace Soap.Pf.ClientServerMessaging.Routing
{
    using Soap.Pf.ClientServerMessaging.Routing.Addresses;

    public abstract class MsmqMessageRoute : MessageRoute
    {
        public MsmqEndpointAddress MsmqEndpointAddress { get; set; }

        protected override string To => MsmqEndpointAddress.ToString();
    }
}