namespace Palmtree.Api.Sso.Domain.Logic
{
    using DataStore.Impl.SqlServer;
    using Newtonsoft.Json;
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
            string applicationName)
        {
            EnvironmentName = environmentName;
            ApiServerSettings = apiServerSettings;
            SqlServerDbSettings = sqlServerDbSettings;
            MailgunEmailSenderSettings = mailgunEmailSenderSettings;
            NumberOfApiMessageRetries = numberOfApiMessageRetries;
            DefaultExceptionMessage = defaultExceptionMessage;
            ReturnExplicitErrorMessages = returnExplicitErrorMessages;
            ApplicationName = applicationName;
        }

        public IApiServerSettings ApiServerSettings { get; }

        public string ApplicationName { get; }

        public string DefaultExceptionMessage { get; }

        public string EnvironmentName { get; }

        public MailgunEmailSenderSettings MailgunEmailSenderSettings { get; }

        public byte NumberOfApiMessageRetries { get; set; }

        public bool ReturnExplicitErrorMessages { get; }

        public SqlServerDbSettings SqlServerDbSettings { get; }

        public static ApplicationConfiguration Create(
            string environmentName,
            IApiServerSettings apiServerSettings,
            SqlServerDbSettings sqlServerDbSettings,
            MailgunEmailSenderSettings mailgunEmailSenderSettings,
            byte numberOfApiMessageRetries = 1,
            string unexpectedExceptionMessage = "An Error Has Occurred",
            bool returnExplicitErrorMessages = false,
            string applicationName = null)
        {
            return new ApplicationConfiguration(
                environmentName,
                apiServerSettings,
                sqlServerDbSettings,
                mailgunEmailSenderSettings,
                numberOfApiMessageRetries,
                unexpectedExceptionMessage,
                returnExplicitErrorMessages,
                applicationName);
        }
    }
}