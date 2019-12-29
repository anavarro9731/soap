namespace Soap.If.MessagePipeline.Models
{
    using System.Linq;
    using Soap.If.Interfaces;

    public class ApiEndpointSettings : IApiEndpointSettings
    {
        internal ApiEndpointSettings() { }

        public ApiEndpointSettings(string httpEndpointUrl, string queueEndpointAddress)
            : this(httpEndpointUrl, queueEndpointAddress?.Split('@').ElementAtOrDefault(0), queueEndpointAddress?.Split('@').ElementAtOrDefault(1))
        {
        }

        public ApiEndpointSettings(string httpEndpointUrl, string queueEndpointName, string queueEndpointHost)
        {
            HttpEndpointUrl = httpEndpointUrl;
            QueueEndpointName = queueEndpointName;
            QueueEndpointHost = queueEndpointHost;
        }

        public string HttpEndpointUrl { get; }

        public string QueueEndpointAddress => string.IsNullOrWhiteSpace(QueueEndpointHost) ? QueueEndpointName : $"{QueueEndpointName}@{QueueEndpointHost}";

        public string QueueEndpointHost { get; }

        public string QueueEndpointName { get; }

        public static IApiEndpointSettings Create(string httpEndpointUrl, string msmqEndpointAddress)
        {
            return new ApiEndpointSettings(httpEndpointUrl, msmqEndpointAddress);
        }
    }
}