namespace Soap.Pf.ClientServerMessaging.Routing
{
    using System;
    using Soap.If.Utility.PureFunctions;

    public class EndpointAddress_Http
    {
        public EndpointAddress_Http(string endpointAddress)
        {
            Guard.Against(!Uri.IsWellFormedUriString(endpointAddress, UriKind.Absolute), $"Endpoint Address {endpointAddress} is not a well-formed URI");

            EndpointUri = new Uri(endpointAddress);
        }

        public Uri EndpointUri { get; set; }

        public override string ToString()
        {
            return EndpointUri.ToString();
        }
    }
}