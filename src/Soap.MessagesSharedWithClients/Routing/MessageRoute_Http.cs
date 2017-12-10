namespace Soap.Pf.ClientServerMessaging.Routing
{
    public abstract class MessageRoute_Http : MessageRoute
    {
        public EndpointAddress_Http EndpointAddressHttp { get; set; }

        protected override string To => EndpointAddressHttp.ToString();
    }
}