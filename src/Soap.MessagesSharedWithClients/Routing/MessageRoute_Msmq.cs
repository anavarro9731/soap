namespace Soap.Pf.ClientServerMessaging.Routing
{
    public abstract class MessageRoute_Msmq : MessageRoute
    {
        public EndpointAddress_Msmq MsmqEndpointAddress { get; set; }

        public override string To => MsmqEndpointAddress.ToString();
    }
}