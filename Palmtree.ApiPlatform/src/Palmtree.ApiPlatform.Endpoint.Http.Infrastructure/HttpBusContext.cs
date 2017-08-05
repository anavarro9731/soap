namespace Palmtree.ApiPlatform.Endpoint.Http.Infrastructure
{
    using System.Collections.Generic;
    using System.Reflection;
    using Palmtree.ApiPlatform.Endpoint.Clients;
    using Palmtree.ApiPlatform.Interfaces;

    public class HttpBusContext
    {
        public static IBusContext Create(IApplicationConfig appConfig, Assembly domainModelsAssembly)
        {
            var endpointClient = new RebusApiClient(
                new List<IRoutingDefinition>
                {
                    new MessageAssemblyRoutingDefinition(domainModelsAssembly, appConfig.ApiServerSettings.MsmqEndpointAddress)
                });

            endpointClient.Start().Wait();

            return endpointClient;
        }
    }
}
