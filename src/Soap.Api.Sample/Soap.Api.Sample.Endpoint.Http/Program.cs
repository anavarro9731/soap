namespace Soap.Api.Sample.Endpoint.Http
{
    using DataStore.Providers.CosmosDb;
    using Soap.Api.Sample.Domain.Logic.Configuration;
    using Soap.Api.Sample.Domain.Logic.Operations;
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.Pf.ClientServerMessaging.Routing.Routes;
    using Soap.Pf.EndpointInfrastructure;
    using Soap.Pf.HttpEndpointBase;

    public class Program
    {
        public static void Main(string[] args)
        {
            var applicationConfiguration = (ApplicationConfiguration)EndpointSetup.GetConfiguration().Variables;
            var domainMessagesAssembly = typeof(UpgradeTheDatabase).Assembly;
            var domainLogicAssembly = typeof(ServiceStateOperations).Assembly;

            HttpEndpoint.Configure<UserAuthenticator>(
                            domainLogicAssembly,
                            domainMessagesAssembly,
                            () => HttpEndpoint.CreateBusContext(
                                applicationConfiguration,
                                new MessageAssemblyToMsmqEndpointRoute(domainMessagesAssembly, applicationConfiguration.ApiEndpointSettings.MsmqEndpointAddress)),
                            () => new CosmosDbRepository(applicationConfiguration.CosmosStoreSettings))
                        .Start();
        }
    }
}