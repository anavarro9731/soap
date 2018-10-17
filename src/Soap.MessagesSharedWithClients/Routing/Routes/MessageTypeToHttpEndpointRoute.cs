namespace Soap.Pf.ClientServerMessaging.Routing
{
    using System;
    using Soap.If.Interfaces.Messages;
    using Soap.Pf.ClientServerMessaging.Routing.Addresses;

    public class MessageTypeToHttpEndpointRoute : HttpMessageRoute
    {
        public MessageTypeToHttpEndpointRoute(Type messageType, string httpEndpointAddress)
        {
            EndpointAddressHttp = new HttpEndpointAddress(httpEndpointAddress);

            MessageTypeName = messageType.AssemblyQualifiedName;
        }

        public string MessageTypeName { get; set; }

        protected override string From => MessageTypeName;

        public override bool CanRouteMessage(IApiMessage message)
        {
            return message.GetType().AssemblyQualifiedName == MessageTypeName;
        }
    }
}