namespace Palmtree.Api.Sso.Endpoint.Http
{
    using DataStore.Impl.SqlServer;
    using Palmtree.Api.Sso.Domain.Logic;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.Pf.EndpointInfrastructure;
    using Soap.Pf.HttpEndpointBase;

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
                            () => RebusBusContext.Create(applicationConfiguration, domainModelsAssembly),
                            () => new SqlServerRepository(applicationConfiguration.SqlServerDbSettings))
                        .Start();
        }
    }
}