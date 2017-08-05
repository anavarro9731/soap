namespace Palmtree.ApiPlatform.Interfaces
{
    public interface IApiServerSettings
    {
        string HttpEndpointUrl { get; }

        string MsmqEndpointAddress { get; }

        string MsmqEndpointHost { get; }

        string MsmqEndpointName { get; }
    }
}
