namespace Soap.Config
{
    using System.Reflection;
    using System.Reflection.Metadata;
    using DataStore.Interfaces;
    using FluentValidation;
    using Soap.Bus;
    using Soap.Interfaces;
    using Soap.NotificationServer;

    /* if you add anything to this config you need to add guards for it and possibly
     also add it to the localsettings.json file and envvars class depending on how it is used */
    
    public class ApplicationConfig : IBootstrapVariables
    {
        protected ApplicationConfig(SoapEnvironments environment, string azureAppName)
        {
            Environment = environment;
            AppId = azureAppName;
        }

        public SoapEnvironments Environment { get; set; }

        public string AppFriendlyName { get; set; }

        public string AppId { get; set; }
        
        public string StorageConnectionString { get; set; }

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
                RuleFor(x => x.Environment).NotNull();
                RuleFor(x => x.AppId).NotNull();
                RuleFor(x => x.AppFriendlyName).NotEmpty();
                RuleFor(x => x.BusSettings).NotNull();
                RuleFor(x => x.DatabaseSettings).NotNull();
                RuleFor(x => x.StorageConnectionString).NotEmpty();
            }
        }
    }
}