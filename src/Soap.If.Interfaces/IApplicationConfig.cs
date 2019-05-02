namespace Soap.If.Interfaces
{
    using DataStore.Interfaces;

    public interface IApplicationConfig
    {
        IApiEndpointSettings ApiEndpointSettings { get; }

        ISeqLoggingConfig SeqLoggingSettings { get; }

        IDatabaseSettings DatabaseSettings { get; }

        INotificationServerSettings NotificationServerSettings { get; }

        string ApplicationVersion { get; }

        string ApplicationName { get; }

        string DefaultExceptionMessage { get; }

        string EnvironmentName { get; }

        byte NumberOfApiMessageRetries { get; }

        bool ReturnExplicitErrorMessages { get; }
    }
}