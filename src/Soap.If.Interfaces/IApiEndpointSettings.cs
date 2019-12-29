namespace Soap.If.Interfaces
{
    public interface IApiEndpointSettings
    {
        string HttpEndpointUrl { get; }

        string QueueEndpointAddress { get; }

        string QueueEndpointHost { get; }

        string QueueEndpointName { get; }
    }
}