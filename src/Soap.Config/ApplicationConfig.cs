namespace Soap.MessagePipeline.Context
{
    using DataStore.Interfaces;
    using Soap.Bus;
    using Soap.NotificationServer;

    public class ApplicationConfig : IBootstrapVariables
    {
        
        public IBusSettings BusSettings { get; set; }
        public IDatabaseSettings DatabaseSettings { get; set; } 
        
        public NotificationServer.Settings NotificationSettings { get; set; }
        
        public SeqServerConfig LogSettings { get; set; }
        
        public string ApplicationName { get; set; }

        public string ApplicationVersion { get; set; }

        public string DefaultExceptionMessage { get; set; }

        public string EnvironmentName { get; set; }

        public bool ReturnExplicitErrorMessages { get; set; }
    }
}