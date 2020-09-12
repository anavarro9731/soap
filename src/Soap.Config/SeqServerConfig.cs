namespace Soap.Config
{
    public class SeqServerConfig 
    {
        public SeqServerConfig(string serverUrl, string apiKey)
        {
            ApiKey = apiKey;
            ServerUrl = serverUrl;
        }

        public string ApiKey { get; }

        public string ServerUrl { get; }
    }
}