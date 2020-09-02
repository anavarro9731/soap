namespace Soap.Api.Sample.Afs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::Sample.Logic;
    using global::Sample.Models.Aggregates;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Soap.PfBase.Api;

    public static class ReceiveMessage
    {
        [FunctionName("ReceiveMessage")]
        public static async Task RunAsync(
            [ServiceBusTrigger("testqueue1", Connection = "AzureWebJobsServiceBus")]
            string myQueueItem,
            string messageId,
            IDictionary<string, object> UserProperties,
            ILogger log)
        {
            try
            {
                HelperFunctions.SetAppKey();

                AzureFunctionContext.LoadAppConfig(out var logger, out var appConfig);

                await AzureFunctionContext.Execute<User>(
                    myQueueItem,
                    new MappingRegistration(),
                    messageId,
                    UserProperties,
                    logger,
                    appConfig);
            }
            catch (Exception e)
            {
                log.LogCritical(e, "Could not execute pipeline");
            }
        }
    }
}