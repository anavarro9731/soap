namespace Soap.Pf.EndpointInfrastructure
{
    using Newtonsoft.Json;

    public class SeqLoggingConfig
    {
        [JsonConstructor]
        private SeqLoggingConfig(string serverUrl, string apiKey)
        {
            ApiKey = apiKey;
            ServerUrl = serverUrl;
        }

        public string ApiKey { get; }

        public string ServerUrl { get; }

        public static SeqLoggingConfig Create(string serverUrl, string apiKey = null)
        {
            return new SeqLoggingConfig(serverUrl, apiKey);
        }
    }
}