namespace Soap.Endpoint.Clients
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class MessageTypeRoutingDefinition : IRoutingDefinition
    {
        public MessageTypeRoutingDefinition(Type messageType, string endpointName, string endpointMachine)
        {
            EndpointMachine = endpointMachine;
            EndpointName = endpointName;
            MessageType = messageType;
        }

        public string EndpointMachine { get; set; }

        public string EndpointName { get; set; }

        public Type MessageType { get; set; }

        public bool CanRoute(IApiMessage message)
        {
            var accept = MessageType.FullName == message.GetType().FullName;
            return accept;
        }
    }
}
