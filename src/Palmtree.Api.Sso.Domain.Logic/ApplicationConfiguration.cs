namespace Palmtree.Api.Sso.Domain.Logic
{
    using DataStore.Impl.SqlServer;
    using Newtonsoft.Json;
    using Soap.Endpoint.Infrastructure;
    using Soap.Interfaces;
    using Soap.ThirdPartyClients.Mailgun;

    public class ApplicationConfiguration : IApplicationConfig
    {
        [JsonConstructor]
        private ApplicationConfiguration(
            string environmentName,
            IApiServerSettings apiServerSettings,
            SqlServerDbSettings sqlServerDbSettings,
            MailgunEmailSenderSettings mailgunEmailSenderSettings,
            byte numberOfApiMessageRetries,
            string defaultExceptionMessage,
            bool returnExplicitErrorMessages,
            string applicationName,
            SeqLoggingConfig seqLoggingConfig)
        {
            EnvironmentName = environmentName;
            ApiServerSettings = apiServerSettings;
            SqlServerDbSettings = sqlServerDbSettings;
            MailgunEmailSenderSettings = mailgunEmailSenderSettings;
            NumberOfApiMessageRetries = numberOfApiMessageRetries;
            DefaultExceptionMessage = defaultExceptionMessage;
            ReturnExplicitErrorMessages = returnExplicitErrorMessages;
            ApplicationName = applicationName;
            SeqLoggingConfig = seqLoggingConfig;
        }

        public IApiServerSettings ApiServerSettings { get; }

        public string ApplicationName { get; }

        public string DefaultExceptionMessage { get; }

        public string EnvironmentName { get; }

        public MailgunEmailSenderSettings MailgunEmailSenderSettings { get; }

        public byte NumberOfApiMessageRetries { get; set; }

        public bool ReturnExplicitErrorMessages { get; }

        public SeqLoggingConfig SeqLoggingConfig { get; }

        public SqlServerDbSettings SqlServerDbSettings { get; }

        public static ApplicationConfiguration Create(
            string environmentName,
            IApiServerSettings apiServerSettings,
            SqlServerDbSettings sqlServerDbSettings,
            MailgunEmailSenderSettings mailgunEmailSenderSettings,
            byte numberOfApiMessageRetries = 1,
            string unexpectedExceptionMessage = "An Error Has Occurred",
            bool returnExplicitErrorMessages = false,
            string applicationName = null,
            SeqLoggingConfig seqLoggingConfig = null)
        {
            return new ApplicationConfiguration(
                environmentName,
                apiServerSettings,
                sqlServerDbSettings,
                mailgunEmailSenderSettings,
                numberOfApiMessageRetries,
                unexpectedExceptionMessage,
                returnExplicitErrorMessages,
                applicationName,
                seqLoggingConfig);
        }
    }
}