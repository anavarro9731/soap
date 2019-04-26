namespace Soap.Api.Sso.Endpoint.Msmq
{
    using DataStore.Providers.CosmosDb;
    using Soap.Api.Sso.Domain.Logic.Configuration;
    using Soap.Api.Sso.Domain.Logic.Operations;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Pf.ClientServerMessaging.Routing.Routes;
    using Soap.Pf.EndpointInfrastructure;
    using Soap.Pf.MsmqEndpointBase;

    public class Program
    {
        public static void Main(string[] args)
        {
            var applicationConfiguration = (ApplicationConfiguration)EndpointSetup.GetConfiguration().Variables;
            var domainMessagesAssembly = typeof(UpgradeTheDatabase).Assembly;
            var domainLogicAssembly = typeof(UserOperations).Assembly;
            var serviceSettings = new MsmqEndpointWindowsServiceSettings
            {
                Name = "Sso.Api.Endpoint.Msmq",
                DisplayName = applicationConfiguration.ApplicationName ?? "SSO API - MSMQ",
                Description = "SSO API MSMQ Endpoint",
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