namespace Soap.Pf.ClientServerMessaging.Routing
{
    using System;
    using Soap.If.Interfaces.Messages;

    public class MessageRoute_MessageTypeToMsmqEndpoint : MessageRoute_Msmq
    {
        protected MessageRoute_MessageTypeToMsmqEndpoint(Type messageType, string msmqEndpointAddress)
        {
            MsmqEndpointAddress = new EndpointAddress_Msmq(msmqEndpointAddress);

            MessageTypeName = messageType.AssemblyQualifiedName;
        }

        public string MessageTypeName { get; set; }

        public EndpointAddress_Msmq MsmqEndpointAddress { get; set; }

        protected override string From => MessageTypeName;

        public override bool CanRouteMessage(IApiMessage message)
        {
            return message.GetType().AssemblyQualifiedName == MessageTypeName;
        }
    }
}