namespace Soap.If.Interfaces
{
    public interface IApiEndpointSettings
    {
        string HttpEndpointUrl { get; }

        string MsmqEndpointAddress { get; }

        string MsmqEndpointHost { get; }

        string MsmqEndpointName { get; }
    }
}