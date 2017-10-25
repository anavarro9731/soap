namespace Palmtree.Api.Sso.Endpoint.Msmq
{
    using DataStore.Impl.SqlServer;
    using Palmtree.Api.Sso.Domain.Logic;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.Endpoint.Infrastructure;
    using Soap.Endpoint.Msmq.Infrastructure;

    public class Program
    {
        public static void Main(string[] args)
        {
            var applicationConfiguration = (ApplicationConfiguration)EndpointSetup.GetConfiguration().Variables;
            var domainModelsAssembly = typeof(SeedDatabase).Assembly;
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
                            domainModelsAssembly,
                            container => RebusBusContext.Create<ConfirmEmail>(applicationConfiguration, container, false),
                            () => new SqlServerRepository(applicationConfiguration.SqlServerDbSettings))
                        .Start(serviceSettings);
        }
    }
}