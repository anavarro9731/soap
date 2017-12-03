namespace Soap.Pf.EndpointClients
{
    using Soap.If.Interfaces.Messages;

    public interface IRoutingDefinition
    {
        string EndpointMachine { get; set; }

        string EndpointName { get; set; }

        bool CanRoute(IApiMessage message);
    }
}