//##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Afs
{
    using DataStore.Providers.CosmosDb;
    using Soap.Bus;
    using Soap.Config;
    using Soap.Interfaces;
    using Soap.NotificationServer;

    /// <summary>
    /// This is used to as a template to make a new config repo
    /// </summary>
    public class AppConfig : ApplicationConfig
    {
        public AppConfig() : base(SoapEnvironments.Development, EnvVars.AppId)
        {
            AppFriendlyName = "Soap Api";
            BusSettings = new AzureBus.Settings(3, EnvVars.AzureWebJobsServiceBus, EnvVars.AzureResourceGroup, EnvVars.AzureBusNamespace, EnvVars.EnvironmentPartitionKey);
            DatabaseSettings = new CosmosSettings(EnvVars.CosmosDbKey, EnvVars.EnvironmentPartitionKey, EnvVars.CosmosDbDatabaseName, EnvVars.CosmosDbEndpointUri);
            StorageConnectionString = EnvVars.AzureWebJobsStorage;
            HttpApiEndpoint = EnvVars.FunctionAppHostUrl;
            NotificationSettings = new NotificationServer.Settings();
        }
    }
}
