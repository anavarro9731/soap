namespace Soap.Interfaces
{
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;

    public class ApplicationConfig
    {
        public ApplicationConfig(
            _ApiEndpointSettings apiEndpointSettings,
            string applicationName,
            string applicationVersion,
            _BusSettings busSettings,
            IDatabaseSettings databaseSettings,
            string defaultExceptionMessage,
            string environmentName,
            INotificationServerSettings notificationServerSettings,
            byte numberOfApiMessageRetries,
            bool returnExplicitErrorMessages,
            _SeqLoggingConfig seqLoggingSettings)
        {
            ApiEndpointSettings = apiEndpointSettings;
            ApplicationName = applicationName;
            ApplicationVersion = applicationVersion;
            BusSettings = busSettings;
            DatabaseSettings = databaseSettings;
            DefaultExceptionMessage = defaultExceptionMessage;
            EnvironmentName = environmentName;
            NotificationServerSettings = notificationServerSettings;
            NumberOfApiMessageRetries = numberOfApiMessageRetries;
            ReturnExplicitErrorMessages = returnExplicitErrorMessages;
            SeqLoggingSettings = seqLoggingSettings;
        }

        public interface INotificationServerSettings
        {
            INotificationServer CreateServer(IMessageAggregator messageAggregator);
        }

        public _ApiEndpointSettings ApiEndpointSettings { get; }

        public string ApplicationName { get; }

        public string ApplicationVersion { get; }

        public _BusSettings BusSettings { get; }

        public IDatabaseSettings DatabaseSettings { get; }

        public string DefaultExceptionMessage { get; }

        public string EnvironmentName { get; }

        public INotificationServerSettings NotificationServerSettings { get; }

        public byte NumberOfApiMessageRetries { get; }

        public bool ReturnExplicitErrorMessages { get; }

        public _SeqLoggingConfig SeqLoggingSettings { get; }

        public class _ApiEndpointSettings
        {
            private string HttpEndpointUrl { get; }
        }

        public class _BusSettings
        {
            private string QueueConnectionString { get; }
        }

        public class _SeqLoggingConfig
        {
            private string ApiKey { get; }

            private string ServerUrl { get; }
        }
    }
}