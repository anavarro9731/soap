namespace Soap.Pf.ClientServerMessaging.Routing.Routes
{
    using System;
    using Soap.If.Interfaces.Messages;
    using Soap.Pf.ClientServerMessaging.Commands;
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
            if (message is IForwardCommandFromHttpToMsmq forwardedMessage)
            {
                return forwardedMessage.CommandToForward.GetType().AssemblyQualifiedName == MessageTypeName;
            }
            else
            {
                return message.GetType().AssemblyQualifiedName == MessageTypeName;
            }
        }
    }
}