namespace Soap.Endpoint.Clients
{
    using Soap.Interfaces.Messages;

    public interface IRoutingDefinition
    {
        string EndpointMachine { get; set; }

        string EndpointName { get; set; }

        bool CanRoute(IApiMessage message);
    }
}