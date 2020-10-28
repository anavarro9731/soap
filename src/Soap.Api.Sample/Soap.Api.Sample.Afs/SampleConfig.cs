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
            StorageConnectionString =
                "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=soapapisamplevnext;AccountKey=IK4cxV9Oz60n72GO3zTaB5NYYGuncDxfXz1FDMDHwmhwrtgdeVSs+VyMNcfw0LHDnPyFl7Hd/72d21zQDecffg==";
        }
    }
}