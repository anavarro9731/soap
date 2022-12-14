//* ##REMOVE-IN-COPY##
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
            NotificationSettings = new NotificationServer.Settings();
            CorsOrigin = EnvVars.CorsOrigin;
            FunctionAppHostName = EnvVars.FunctionAppHostName;
            FunctionAppHostUrlWithTrailingSlash = EnvVars.FunctionAppHostUrlWithTrailingSlash;
            UseServiceLevelAuthorityInTheAbsenceOfASecurityContext = false;
            AuthLevel = AuthLevel.None;
            EncryptionKey = "Xhdz9Q3yaps69aHmVUvzHc3uEvkz5WPQBWY6XSnphqqXgRgAs6K3dRWQ4U4VepRW";
        }
    }
}
