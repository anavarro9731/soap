namespace Soap.If.MessagePipeline.Models
{
    using System.Linq;
    using Newtonsoft.Json;
    using Soap.If.Interfaces;

    public class ApiEndpointSettings : IApiEndpointSettings
    {
        public ApiEndpointSettings(string httpEndpointUrl, string msmqEndpointAddress)
            : this(httpEndpointUrl, msmqEndpointAddress?.Split('@').ElementAtOrDefault(0), msmqEndpointAddress?.Split('@').ElementAtOrDefault(1))
        {
        }

        [JsonConstructor]
        public ApiEndpointSettings(string httpEndpointUrl, string msmqEndpointName, string msmqEndpointHost)
        {
            HttpEndpointUrl = httpEndpointUrl;
            MsmqEndpointName = msmqEndpointName;
            MsmqEndpointHost = msmqEndpointHost;
        }

        public string HttpEndpointUrl { get; }

        public string MsmqEndpointAddress => string.IsNullOrWhiteSpace(MsmqEndpointHost) ? MsmqEndpointName : $"{MsmqEndpointName}@{MsmqEndpointHost}";

        public string MsmqEndpointHost { get; }

        public string MsmqEndpointName { get; }

        public static IApiEndpointSettings Create(string httpEndpointUrl, string msmqEndpointAddress)
        {
            return new ApiEndpointSettings(httpEndpointUrl, msmqEndpointAddress);
        }
    }
}