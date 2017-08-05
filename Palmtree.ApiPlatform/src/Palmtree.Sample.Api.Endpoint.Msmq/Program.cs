namespace Palmtree.Sample.Api.Endpoint.Msmq
{
    using DataStore.Impl.SqlServer;
    using Palmtree.ApiPlatform.Endpoint.Infrastructure;
    using Palmtree.ApiPlatform.Endpoint.Msmq.Infrastructure;
    using Palmtree.Sample.Api.Domain.Logic;
    using Palmtree.Sample.Api.Domain.Logic.Operations;
    using Palmtree.Sample.Api.Domain.Messages.Commands;

    public class Program
    {
        public static void Main(string[] args)
        {
            var applicationConfiguration = (ApplicationConfiguration)EndpointSetup.GetConfiguration().Variables;
            var domainModelsAssembly = typeof(SeedDatabase).Assembly;
            var domainLogicAssembly = typeof(UserOperations).Assembly;
            var serviceSettings = new MsmqEndpointWindowsServiceSettings
            {
                Name = "Palmtree.ApiPlatform.Endpoint.Msmq",
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
