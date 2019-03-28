namespace Soap.Pf.ClientServerMessaging.Routing.Addresses
{
    using System;
    using Soap.If.Utility.PureFunctions;

    public class HttpEndpointAddress
    {
        public HttpEndpointAddress(string endpointAddress)
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