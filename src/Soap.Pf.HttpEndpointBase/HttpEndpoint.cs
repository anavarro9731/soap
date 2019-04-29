namespace Soap.Pf.HttpEndpointBase
{
    using System;
    using System.Reflection;
    using DataStore.Interfaces;
    using Soap.If.Interfaces;
    using Soap.Pf.ClientServerMessaging.Routing.Routes;
    using Soap.Pf.EndpointClients;

    public static class HttpEndpoint
    {
        public static HttpEndpointConfiguration<TUserAuthenticator> Configure<TUserAuthenticator>(
            Assembly domainLogicAssembly,
            Assembly domainMessagesAssembly,
            Func<IBusContext> busContextBuilder,
            Func<IDocumentRepository> documentRepositoryBuilder) where TUserAuthenticator : IAuthenticateUsers
        {
            return new HttpEndpointConfiguration<TUserAuthenticator>(domainLogicAssembly, domainMessagesAssembly, busContextBuilder, documentRepositoryBuilder);
        }

        public static BusApiClient CreateBusContext(IApplicationConfig appConfig, params MsmqMessageRoute[] msmqRoutes)
        {
            var endpointClient = new BusApiClient(msmqRoutes);

            endpointClient.Start().Wait();

            return endpointClient;
        }
    }
}