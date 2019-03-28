namespace Soap.Pf.ClientServerMessaging.Routing.Routes
{
    using System;
    using Soap.If.Interfaces.Messages;
    using Soap.Pf.ClientServerMessaging.Routing.Addresses;

    public class MessageTypeToMsmqEndpointRoute : MsmqMessageRoute
    {
        public MessageTypeToMsmqEndpointRoute(Type messageType, string msmqEndpointAddress)
        {
            MsmqEndpointAddress = new MsmqEndpointAddress(msmqEndpointAddress);

            MessageTypeName = messageType.AssemblyQualifiedName;

            MessageTypes.Add(messageType);
        }

        public string MessageTypeName { get; set; }

        protected override string From => MessageTypeName;

        public override bool CanRouteMessage(IApiMessage message)
        {
            return message.GetType().AssemblyQualifiedName == MessageTypeName;
        }
    }
}