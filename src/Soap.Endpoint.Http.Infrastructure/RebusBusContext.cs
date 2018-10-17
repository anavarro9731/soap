namespace Soap.Pf.HttpEndpointBase
{
    using System.Collections.Generic;
    using System.Reflection;
    using Rebus.Config;
    using Rebus.Transport;
    using Soap.If.Interfaces;
    using Soap.Pf.ClientServerMessaging.Routing;
    using Soap.Pf.EndpointClients;

    public class RebusBusContext : MsmqApiClient
    {
        private RebusBusContext(IEnumerable<MsmqMessageRoute> routingDefinitions)
            : base(routingDefinitions)
        {
        }

        public static IBusContext Create(IApplicationConfig appConfig, Assembly domainModelsAssembly)
        {
            var endpointClient = new RebusBusContext(
                new List<MsmqMessageRoute>
                {
                    new MessageAssemblyToMsmqEndpointRoute(domainModelsAssembly, appConfig.ApiEndpointSettings.MsmqEndpointAddress)
                });

            endpointClient.Start().Wait();

            return endpointClient;
        }

        protected override void ConfigureRebusTransport(StandardConfigurer<ITransport> configurer)
        {
            configurer.UseMsmqAsOneWayClient();
        }
    }
}