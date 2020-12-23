//##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Afs
{
    using DataStore.Providers.CosmosDb;
    using Soap.Bus;
    using Soap.Config;
    using Soap.Interfaces;

    /// <summary>
    /// This is used to as a template to make a new config repo
    /// </summary>
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
