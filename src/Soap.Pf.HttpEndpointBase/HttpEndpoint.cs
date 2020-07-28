namespace Soap.Pf.HttpEndpointBase
{
    using Soap.Pf.EndpointClients;

    public static class HttpEndpoint
    {
        public static HttpEndpointConfiguration<TUserAuthenticator> Configure<TUserAuthenticator>(
            Assembly domainLogicAssembly,
            Assembly domainMessagesAssembly,
            Func<IBusContext> busContextBuilder) where TUserAuthenticator : IAuthenticateUsers =>
            new HttpEndpointConfiguration<TUserAuthenticator>(domainLogicAssembly, domainMessagesAssembly, busContextBuilder);

        public static BusApiClient CreateBusContext(IApplicationConfig appConfig, params MsmqMessageRoute[] msmqRoutes)
        {
            var endpointClient = new BusApiClient(msmqRoutes);

            endpointClient.Start().Wait();

            return endpointClient;
        }
    }
}