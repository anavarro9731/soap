namespace Soap.Endpoint.Clients
{
    using System;
    using System.Reflection;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class MessageAssemblyRoutingDefinition : IRoutingDefinition
    {
        public MessageAssemblyRoutingDefinition(Assembly messageAssembly, string endpointName, string endpointMachine)
        {
            EndpointMachine = endpointMachine;
            EndpointName = endpointName;
            MessageAssembly = messageAssembly;
        }

        public MessageAssemblyRoutingDefinition(Assembly messageAssembly, string fullyQualifiedEndpointName)
        {
            var split = fullyQualifiedEndpointName.Split('@');
            if (split.Length < 2)
            {
                throw new ArgumentException($"{nameof(fullyQualifiedEndpointName)} must be of the format endpointName@machineName");
            }
            EndpointName = split[0];
            EndpointMachine = split[1];
            MessageAssembly = messageAssembly;
        }

        public string EndpointMachine { get; set; }

        public string EndpointName { get; set; }

        public Assembly MessageAssembly { get; set; }

        public bool CanRoute(IApiMessage message)
        {
            var accept = MessageAssembly.FullName == message.GetType().Assembly.FullName;
            return accept;
        }
    }
}
