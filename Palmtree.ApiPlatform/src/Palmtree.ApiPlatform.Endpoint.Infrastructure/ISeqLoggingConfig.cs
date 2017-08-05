namespace Palmtree.ApiPlatform.Endpoint.Infrastructure
{
    public interface ISeqLoggingConfig
    {
        string SeqLogsServerApiKey { get; }

        string SeqLogsServerUrl { get; }
    }
}
