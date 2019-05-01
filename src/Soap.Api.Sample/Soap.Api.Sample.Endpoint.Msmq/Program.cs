namespace Soap.Api.Sample.Endpoint.Msmq
{
    using DataStore.Providers.CosmosDb;
    using Soap.Api.Sample.Domain.Logic.Configuration;
    using Soap.Api.Sample.Domain.Logic.Operations;
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.Pf.ClientServerMessaging.Routing.Routes;
    using Soap.Pf.EndpointInfrastructure;
    using Soap.Pf.MsmqEndpointBase;

    public class Program
    {
        public static void Main(string[] args)
        {
            var applicationConfiguration = (ApplicationConfiguration)EndpointSetup.GetConfiguration().Variables;
            var domainMessagesAssembly = typeof(UpgradeTheDatabaseCommand).Assembly;
            var domainLogicAssembly = typeof(ServiceStateOperations).Assembly;
            var serviceSettings = new MsmqEndpointWindowsServiceSettings
            {
                Name = "Sample.Api.Endpoint.Msmq",
                DisplayName = applicationConfiguration.ApplicationName ?? "Sample API - MSMQ",
                Description = "Sample Api MSMQ Endpoint",
                StartAutomatically = true
            };

            MsmqEndpoint.Configure<UserAuthenticator>(
                            domainLogicAssembly,
                            domainMessagesAssembly,
                            container => MsmqEndpoint.CreateBusContext(
                                applicationConfiguration,
                                container,
                                new MessageAssemblyToMsmqEndpointRoute(domainMessagesAssembly, applicationConfiguration.ApiEndpointSettings.MsmqEndpointAddress)),
                            () => new CosmosDbRepository(applicationConfiguration.CosmosStoreSettings))
                        .Start(serviceSettings);
        }
    }
}