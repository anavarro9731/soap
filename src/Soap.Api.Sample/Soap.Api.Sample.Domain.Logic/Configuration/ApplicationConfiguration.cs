namespace Soap.Api.Sample.Domain.Logic.Configuration
{
    using DataStore.Providers.CosmosDb;
    using Newtonsoft.Json;
    using Soap.If.Interfaces;
    using Soap.Integrations.Mailgun;
    using Soap.Pf.EndpointInfrastructure;

    public class ApplicationConfiguration : IApplicationConfig
    {
        [JsonConstructor]
        private ApplicationConfiguration(
            string environmentName,
            string applicationVersion,
            IApiEndpointSettings apiEndpointSettings,
            CosmosSettings cosmosStoreSettings,
            MailgunEmailSenderSettings mailgunEmailSenderSettings,
            byte numberOfApiMessageRetries,
            string defaultExceptionMessage,
            bool returnExplicitErrorMessages,
            string applicationName,
            SeqLoggingConfig seqLoggingConfig)
        {
            EnvironmentName = environmentName;
            ApiEndpointSettings = apiEndpointSettings;
            CosmosStoreSettings = cosmosStoreSettings;
            MailgunEmailSenderSettings = mailgunEmailSenderSettings;
            NumberOfApiMessageRetries = numberOfApiMessageRetries;
            DefaultExceptionMessage = defaultExceptionMessage;
            ReturnExplicitErrorMessages = returnExplicitErrorMessages;
            ApplicationName = applicationName;
            SeqLoggingConfig = seqLoggingConfig;
            ApplicationVersion = this.GetType().Assembly.GetName().Version.ToString(3);
        }

        public IApiEndpointSettings ApiEndpointSettings { get; }

        public string ApplicationVersion { get; }

        public string ApplicationName { get; }

        public string DefaultExceptionMessage { get; }

        public string EnvironmentName { get; }

        public MailgunEmailSenderSettings MailgunEmailSenderSettings { get; }

        public byte NumberOfApiMessageRetries { get; set; }

        public bool ReturnExplicitErrorMessages { get; }

        public SeqLoggingConfig SeqLoggingConfig { get; }

        public CosmosSettings CosmosStoreSettings { get; }

        public static ApplicationConfiguration Create(
            string environmentName,
            string applicationVersion,
            IApiEndpointSettings apiEndpointSettings,
            CosmosSettings cosmosStoreSettings,
            MailgunEmailSenderSettings mailgunEmailSenderSettings,
            byte numberOfApiMessageRetries = 1,
            string unexpectedExceptionMessage = "An Error Has Occurred",
            bool returnExplicitErrorMessages = false,
            string applicationName = null,
            SeqLoggingConfig seqLoggingConfig = null)
        {
            return new ApplicationConfiguration(
                environmentName,
                applicationVersion,
                apiEndpointSettings,
                cosmosStoreSettings,
                mailgunEmailSenderSettings,
                numberOfApiMessageRetries,
                unexpectedExceptionMessage,
                returnExplicitErrorMessages,
                applicationName,
                seqLoggingConfig);
        }
    }
}