namespace Soap.Api.Sample.Domain.Logic.Configuration
{
    using DataStore.Providers.CosmosDb;
    using Soap.If.Interfaces;
    using Soap.Integrations.MailGun;

    public class ApplicationConfiguration : IApplicationConfig
    {
        //TODO: CosmosSetting and Mailgun to IAppConfig
        public ApplicationConfiguration(
            string environmentName,
            string applicationVersion,
            IApiEndpointSettings apiEndpointSettings,
            CosmosSettings cosmosStoreSettings,
            MailGunEmailSenderSettings mailGunEmailSenderSettings,
            bool returnExplicitErrorMessages = false,
            byte numberOfApiMessageRetries = 1,
            string defaultExceptionMessage = "An error has occurred.",
            string applicationName = null,
            ISeqLoggingConfig seqLoggingSettings = null)
        {
            EnvironmentName = environmentName;
            ApiEndpointSettings = apiEndpointSettings;
            CosmosStoreSettings = cosmosStoreSettings;
            MailGunEmailSenderSettings = mailGunEmailSenderSettings;
            NumberOfApiMessageRetries = numberOfApiMessageRetries;
            DefaultExceptionMessage = defaultExceptionMessage;
            ReturnExplicitErrorMessages = returnExplicitErrorMessages;
            ApplicationName = applicationName;
            SeqLoggingSettings = seqLoggingSettings;
            ApplicationVersion = GetType().Assembly.GetName().Version.ToString(3);
        }

        public IApiEndpointSettings ApiEndpointSettings { get; }

        public string ApplicationName { get; }

        public string ApplicationVersion { get; }

        public CosmosSettings CosmosStoreSettings { get; }

        public string DefaultExceptionMessage { get; }

        public string EnvironmentName { get; }

        public MailGunEmailSenderSettings MailGunEmailSenderSettings { get; }

        public byte NumberOfApiMessageRetries { get; set; }

        public bool ReturnExplicitErrorMessages { get; }

        public ISeqLoggingConfig SeqLoggingSettings { get; }

        public class SeqLoggingConfig : ISeqLoggingConfig
        {
            public SeqLoggingConfig(string serverUrl, string apiKey)
            {
                ApiKey = apiKey;
                ServerUrl = serverUrl;
            }

            public string ApiKey { get; }

            public string ServerUrl { get; }
        }
    }
}