namespace Soap.Config
{
    using DataStore.Interfaces;
    using FluentValidation;
    using Soap.Bus;
    using Soap.NotificationServer;

    public class EnvironmentVariables
    {
        public ApiEndpointSettings ApiEndpointSettings;

        public string ApplicationName;

        public string ApplicationVersion;

        public BusSettings BusSettings;

        public IDatabaseSettings DatabaseSettings;

        public string DefaultExceptionMessage;

        public string EnvironmentName;

        public NotificationServer.Settings NotificationServerSettings;

        public bool ReturnExplicitErrorMessages;

        public SeqLoggingConfig SeqLoggingSettings;

        public class Validator : AbstractValidator<EnvironmentVariables>
        {
            public Validator()
            {
                RuleFor(x => x.ApiEndpointSettings).NotNull().SetValidator(new ApiEndpointSettings.Validator());
                RuleFor(x => x.ApplicationName).NotEmpty();
                RuleFor(x => x.ApplicationVersion).NotEmpty();
                RuleFor(x => x.BusSettings).NotNull().SetValidator(new BusSettings.Validator());
                RuleFor(x => x.DatabaseSettings).NotNull();
                RuleFor(x => x.DefaultExceptionMessage).NotEmpty();
                RuleFor(x => x.EnvironmentName).NotEmpty();
                RuleFor(x => x.NotificationServerSettings).NotNull().SetValidator(new NotificationServer.Settings.Validator());
                RuleFor(x => x.SeqLoggingSettings).NotNull().SetValidator(new SeqLoggingConfig.Validator());
            }
        }
    }

    public class ApiEndpointSettings
    {
        public string HttpEndpointUrl;

        public class Validator : AbstractValidator<ApiEndpointSettings>
        {
            public Validator()
            {
                RuleFor(x => x.HttpEndpointUrl).NotEmpty();
            }
        }
    }

    public class SeqLoggingConfig
    {
        public string ApiKey;

        public string ServerUrl;

        public class Validator : AbstractValidator<SeqLoggingConfig>
        {
            public Validator()
            {
                RuleFor(x => x.ApiKey).NotEmpty();
                RuleFor(x => x.ServerUrl).NotEmpty();
            }
        }
    }
}