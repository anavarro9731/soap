namespace Soap.MessagePipeline.Context
{
    using System.Reflection;
    using DataStore.Interfaces;
    using Soap.Bus;
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

        public SeqServerConfig LogSettings { get; set; }

        public NotificationServer.Settings NotificationSettings { get; set; } = new NotificationServer.Settings();

        public bool ReturnExplicitErrorMessages { get; set; }
    }
}