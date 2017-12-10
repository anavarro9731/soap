namespace Soap.Pf.ClientServerMessaging.Routing
{
    using System;
    using Soap.If.Interfaces.Messages;

    public class MessageRoute_MessageTypeToHttpEndpoint : MessageRoute_Http
    {
        protected MessageRoute_MessageTypeToHttpEndpoint(Type messageType, string httpEndpointAddress)
        {
            EndpointAddressHttp = new EndpointAddress_Http(httpEndpointAddress);

            MessageTypeName = messageType.AssemblyQualifiedName;
        }

        public EndpointAddress_Http EndpointAddressHttp { get; set; }

        public string MessageTypeName { get; set; }

        protected override string From => MessageTypeName;

        public override bool CanRouteMessage(IApiMessage message)
        {
            return message.GetType().AssemblyQualifiedName == MessageTypeName;
        }
    }
}