namespace Palmtree.ApiPlatform.Endpoint.Clients
{
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public interface IRoutingDefinition
    {
        string EndpointMachine { get; set; }

        string EndpointName { get; set; }

        bool CanRoute(IApiMessage message);
    }
}
