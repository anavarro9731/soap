namespace Soap.Pf.ClientServerMessaging.Routing.Routes
{
    using Soap.Pf.ClientServerMessaging.Routing.Addresses;

    public abstract class HttpMessageRoute : MessageRoute
    {
        public HttpEndpointAddress EndpointAddressHttp { get; set; }

        protected override string To => EndpointAddressHttp.ToString();
    }
}