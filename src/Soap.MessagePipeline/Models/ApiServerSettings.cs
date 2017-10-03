namespace Soap.MessagePipeline.Models
{
    using System.Linq;
    using Newtonsoft.Json;
    using Soap.Interfaces;

    public class ApiServerSettings : IApiServerSettings
    {
        public ApiServerSettings(string httpEndpointUrl, string msmqEndpointAddress)
            : this(httpEndpointUrl, msmqEndpointAddress?.Split('@').ElementAtOrDefault(0), msmqEndpointAddress?.Split('@').ElementAtOrDefault(1))
        {
        }

        [JsonConstructor]
        public ApiServerSettings(string httpEndpointUrl, string msmqEndpointName, string msmqEndpointHost)
        {
            HttpEndpointUrl = httpEndpointUrl;
            MsmqEndpointName = msmqEndpointName;
            MsmqEndpointHost = msmqEndpointHost;
        }

        public string HttpEndpointUrl { get; }

        public string MsmqEndpointAddress => string.IsNullOrWhiteSpace(MsmqEndpointHost) ? MsmqEndpointName : $"{MsmqEndpointName}@{MsmqEndpointHost}";

        public string MsmqEndpointHost { get; }

        public string MsmqEndpointName { get; }

        public static IApiServerSettings Create(string httpEndpointUrl, string msmqEndpointAddress)
        {
            return new ApiServerSettings(httpEndpointUrl, msmqEndpointAddress);
        }
    }
}
