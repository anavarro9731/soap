namespace Soap.Pf.HttpEndpointBase
{
    using System.Collections.Generic;
    using System.Reflection;
    using Soap.If.Interfaces;
    using Soap.Pf.ClientServerMessaging;
    using Soap.Pf.EndpointClients;

    public class HttpBusContext
    {
        public static IBusContext Create(IApplicationConfig appConfig, Assembly domainModelsAssembly)
        {
            var endpointClient = new MsmqApiClient(
                new List<IRoutingDefinition>
                {
                    new MessageAssemblyRoutingDefinition(domainModelsAssembly, appConfig.ApiEndpointSettings.MsmqEndpointAddress)
                });

            endpointClient.Start().Wait();

            return endpointClient;
        }
    }
}