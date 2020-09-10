namespace Soap.Api.Sample.Afs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::Sample.Logic;
    using global::Sample.Messages.Commands;
    using global::Sample.Models.Aggregates;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Soap.PfBase.Api;
    using Soap.Utility.Functions.Extensions;

    public static class ReceiveMessage
    {
        [FunctionName("ReceiveMessage")]
        public static async Task RunAsync(
            
            [ServiceBusTrigger("Soap.Api.Sample.Messages", Connection = "AzureWebJobsServiceBus")] //* this uses peeklockmode
            string myQueueItem,
            string messageId,
            IDictionary<string, object> UserProperties,
            ILogger log)
        {
            try
            {
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