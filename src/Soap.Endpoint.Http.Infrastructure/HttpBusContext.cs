namespace Soap.Endpoint.Http.Infrastructure
{
    using System.Collections.Generic;
    using System.Reflection;
    using Soap.Endpoint.Clients;
    using Soap.Interfaces;

    public class HttpBusContext
    {
        public static IBusContext Create(IApplicationConfig appConfig, Assembly domainModelsAssembly)
        {
            var endpointClient = new RebusApiClient(
                new List<IRoutingDefinition>
                {
                    new MessageAssemblyRoutingDefinition(domainModelsAssembly, appConfig.ApiEndpointSettings.MsmqEndpointAddress)
                });

            endpointClient.Start().Wait();

            return endpointClient;
        }
    }
}