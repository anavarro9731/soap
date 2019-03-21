namespace Palmtree.Api.Sso.Endpoint.Http
{
    using DataStore;
    using DataStore.Impl.SqlServer;
    using Palmtree.Api.Sso.Domain.Logic;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.Pf.ClientServerMessaging.Routing.Routes;
    using Soap.Pf.EndpointInfrastructure;
    using Soap.Pf.HttpEndpointBase;

    public class Program
    {
        public static void Main(string[] args)
        {
            var applicationConfiguration = (ApplicationConfiguration)EndpointSetup.GetConfiguration().Variables;
            var domainMessagesAssembly = typeof(SeedDatabase).Assembly;
            var domainLogicAssembly = typeof(UserOperations).Assembly;

            HttpEndpoint.Configure<UserAuthenticator>(
                            domainLogicAssembly,
                            domainMessagesAssembly,
                            () => HttpEndpoint.CreateBusContext(
                                applicationConfiguration,
                                new MessageAssemblyToMsmqEndpointRoute(domainMessagesAssembly, applicationConfiguration.ApiEndpointSettings.MsmqEndpointAddress)),
                            () => new InMemoryDocumentRepository())
                        .Start();
        }
    }
}