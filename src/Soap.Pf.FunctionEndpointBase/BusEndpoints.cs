namespace Soap.Pf.FunctionEndpointBase
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;

    public static class BusEndpoints
    {
        [FunctionName("Function1")]
        public static void Run(
            [ServiceBusTrigger("myqueue", Connection = "")]
            string myQueueItem,
            ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        }
    }
}