namespace Palmtree.Sample.Api.Endpoint.Http
{
    using DataStore.Impl.SqlServer;
    using Palmtree.ApiPlatform.Endpoint.Http.Infrastructure;
    using Palmtree.ApiPlatform.Endpoint.Infrastructure;
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

            HttpEndpoint.Configure<UserAuthenticator>(
                            domainLogicAssembly,
                            domainModelsAssembly,
                            () => HttpBusContext.Create(applicationConfiguration, domainModelsAssembly),
                            () => new SqlServerRepository(applicationConfiguration.SqlServerDbSettings))
                        .Start();
        }
    }
}
