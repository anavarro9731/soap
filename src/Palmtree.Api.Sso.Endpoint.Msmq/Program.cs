namespace Palmtree.Api.Sso.Endpoint.Msmq
{
    using DataStore;
    using DataStore.Impl.SqlServer;
    using Palmtree.Api.Sso.Domain.Logic;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.Pf.ClientServerMessaging.Routing.Routes;
    using Soap.Pf.EndpointInfrastructure;
    using Soap.Pf.MsmqEndpointBase;

    public class Program
    {
        public static void Main(string[] args)
        {
            var applicationConfiguration = (ApplicationConfiguration)EndpointSetup.GetConfiguration().Variables;
            var domainMessagesAssembly = typeof(SeedDatabase).Assembly;
            var domainLogicAssembly = typeof(UserOperations).Assembly;
            var serviceSettings = new MsmqEndpointWindowsServiceSettings
            {
                Name = "Soap.Endpoint.Msmq",
                DisplayName = applicationConfiguration.ApplicationName ?? "Service API - MSMQ",
                Description = "Service API MSMQ Endpoint",
                StartAutomatically = true
            };

            MsmqEndpoint.Configure<UserAuthenticator>(
                            domainLogicAssembly,
                            domainMessagesAssembly,
                            container => MsmqEndpoint.CreateBusContext(
                                applicationConfiguration,
                                container,
                                new MessageAssemblyToMsmqEndpointRoute(domainMessagesAssembly, applicationConfiguration.ApiEndpointSettings.MsmqEndpointAddress)),
                            () => new InMemoryDocumentRepository())
                        .Start(serviceSettings);
        }
    }
}