namespace Soap.Config
{
    using System.Reflection;
    using DataStore.Interfaces;
    using FluentValidation;
    using Soap.Bus;
    using Soap.Interfaces;
    using Soap.NotificationServer;

    public class ApplicationConfig : IBootstrapVariables
    {
        public ApplicationConfig(string appKey, SoapEnvironments environment)
        {
            AppEnvId = new AppEnvIdentifier(appKey, environment);
        }

        public ApplicationConfig(AppEnvIdentifier appEnvId)
        {
            AppEnvId = appEnvId;
        }

        public AppEnvIdentifier AppEnvId { get; }

        public string ApplicationName { get; set; }

        public string ApplicationVersion => Assembly.GetEntryAssembly().GetName().Version.ToString();

        public IBusSettings BusSettings { get; set; }

        public IDatabaseSettings DatabaseSettings { get; set; }

        public string DefaultExceptionMessage { get; set; } = "An Error Occurred";

        public NotificationServer.Settings NotificationSettings { get; set; } = new NotificationServer.Settings();

        public bool ReturnExplicitErrorMessages { get; set; }

        public void Validate() => new Validator().ValidateAndThrow(this);

        public class Validator : AbstractValidator<ApplicationConfig>
        {
            public Validator()
            {
                RuleFor(x => x.AppEnvId).NotNull();
                RuleFor(x => x.ApplicationName).NotEmpty();
                RuleFor(x => x.BusSettings).NotNull();
                RuleFor(x => x.DatabaseSettings).NotNull();
            }
        }
    }
}