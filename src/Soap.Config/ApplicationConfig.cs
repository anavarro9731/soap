namespace Soap.Config
{
    using System.Reflection;
    using DataStore.Interfaces;
    using FluentValidation;
    using Soap.Bus;
    using Soap.Interfaces;
    using Soap.NotificationServer;

    /* If you add anything to this config you need to add validators for it and
     if it relies on env variables also add it to the localsettings.json file and envvars class.
        with the exception of things (e.g. logger) which are read before the config is loaded or in an effort to load the config. 
     if it relies on env vars then you will need to also add it to sampleconfig.cs used by create-new-service.ps1 to setup new config repos 
        and to edit existing config.cs files in the soap demo config repo which requires republishing the Config nuget pkg first
     if it relies on an envvar which is not there by default then it also needs to go into the configure-local-environment.ps1 the part which sets local.settings.json and/or .env based on custom vnext envvars
         in some cases it can be hardcoded for dev and not downloaded but it still needs to be added   
     It may also need to be added to testconfig base class if it is required in TestRuns not to be null i.e. its part of the IBoostrapVariables interface  
     
    This all is ensures that the config remains the sole point of contact for config information, 
    and where it derives from envvars it provides an opportunity for an override, e.g. for a custom domain in live env 
    */

    public class ApplicationConfig : IBootstrapVariables, IConnectWithAuth0
    {
        protected ApplicationConfig(SoapEnvironments environment, string azureAppName)
        {
            Environment = environment;
            AppId = azureAppName;
        }
        
        public string FunctionAppHostName { get; set; }
        
        public string FunctionAppHostUrlWithTrailingSlash { get; set; }
        
        public string CorsOrigin { get; set; }

        public string AppFriendlyName { get; set; }

        public string AppId { get; set; }

        public string ApplicationVersion => Assembly.GetEntryAssembly().GetName().Version.ToString();

        public bool AuthEnabled { get; set; }

        public string Auth0EnterpriseAdminClientId { get; set; }

        public string Auth0EnterpriseAdminClientSecret { get; set; }

        public string Auth0TenantDomain { get; set; }

        public IBusSettings BusSettings { get; set; }

        public IDatabaseSettings DatabaseSettings { get; set; }

        public string DefaultExceptionMessage { get; set; } = "An Error Occurred";

        public SoapEnvironments Environment { get; set; }

        public NotificationServer.Settings NotificationSettings { get; set; }

        public string StorageConnectionString { get; set; }

        public void Validate() => new Validator().ValidateAndThrow(this);

        public class Validator : AbstractValidator<ApplicationConfig>
        {
            public Validator()
            {
                RuleFor(x => x.FunctionAppHostUrlWithTrailingSlash).NotEmpty();
                RuleFor(x => x.FunctionAppHostName).NotEmpty();
                RuleFor(x => x.Environment).NotNull();
                RuleFor(x => x.AppId).NotNull();
                RuleFor(x => x.AppFriendlyName).NotEmpty();
                RuleFor(x => x.BusSettings).NotNull();
                RuleFor(x => x.DatabaseSettings).NotNull();
                RuleFor(x => x.CorsOrigin).NotEmpty();
                RuleFor(x => x.StorageConnectionString).NotEmpty();
                RuleFor(x => x)
                    .Must(
                        x => x.AuthEnabled == false || !string.IsNullOrWhiteSpace(x.Auth0TenantDomain)
                             && !string.IsNullOrEmpty(x.Auth0EnterpriseAdminClientId)
                             && !string.IsNullOrEmpty(x.Auth0EnterpriseAdminClientSecret))
                    .WithMessage(
                        $"If {nameof(IConnectWithAuth0.AuthEnabled)} is set to true, then all Auth0 fields must be populated");
            }
        }
    }
}
