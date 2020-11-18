namespace Soap.Api.Sample.Afs
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Soap.Api.Sample.Logic;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.PfBase.Api;
    using Soap.Utility.Functions.Extensions;

    public static class ReceiveMessage
    {
        [FunctionName("ReceiveMessage")]
        public static async Task RunAsync(
            
            [ServiceBusTrigger("Soap.Api.Sample.Messages", Connection = "AzureWebJobsServiceBus")] //* this uses peeklockmode
            Message myQueueItem,
            string messageId,
            ILogger log)
        {
            
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);
                
                AzureFunctionContext.LoadAppConfig(out var appConfig);
                
                await AzureFunctionContext.Execute<User>(
                    Encoding.UTF8.GetString(myQueueItem.Body),
                    new MappingRegistration(),
                    messageId,
                    myQueueItem.Label,
                    logger,
                    appConfig);
            }
            catch (Exception e)
            {
                logger?.Fatal(e, "Could not execute function");
                log.LogCritical(e.ToString());
            }
            
        }
    }
}