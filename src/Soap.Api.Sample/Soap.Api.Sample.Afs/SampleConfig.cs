namespace Soap.Api.Sample.Afs
{
    using DataStore.Providers.CosmosDb;
    using Soap.Bus;
    using Soap.Config;
    using Soap.Interfaces;

    public class AppConfig : ApplicationConfig
    {
        public AppConfig() : base(SoapEnvironments.Development, EnvVars.AppId)
        {
            AppFriendlyName = "Soap Api";
            BusSettings = new AzureBus.Settings(3, EnvVars.AzureWebJobsServiceBus, EnvVars.AzureResourceGroup, EnvVars.AzureBusNamespace);
            DatabaseSettings = new CosmosSettings(EnvVars.CosmosDbKey, EnvVars.CosmosDbDatabaseName,
                $"https://{EnvVars.CosmosDbAccountName}.documents.azure.com:443/");
            StorageConnectionString = EnvVars.AzureStorageConnectionString;
        }
    }
}