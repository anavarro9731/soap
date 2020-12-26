namespace Soap.PfBase.Api.Functions
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.SignalRService;
    using Microsoft.Extensions.Logging;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces;

    public static partial class PlatformFunctions
    {
        public static async Task HandleMessage<TApiIdentity>(
            Message myQueueItem,
            string messageId,
            MapMessagesToFunctions handlerRegistration,
            IAsyncCollector<SignalRMessage> signalRBinding,
            ILogger log) where TApiIdentity : class, IApiIdentity, new()
        {
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);

                AzureFunctionContext.LoadAppConfig(out var appConfig);

                var result = await AzureFunctionContext.Execute<TApiIdentity>(
                                 Encoding.UTF8.GetString(myQueueItem.Body),
                                 handlerRegistration,
                                 messageId,
                                 myQueueItem.Label,
                                 logger,
                                 appConfig,
                                 signalRBinding);

                if (result.Success == false)
                {
                    ExceptionDispatchInfo.Capture(result.UnhandledError).Throw();
                }
            }
            catch (Exception e)
            {
                logger?.Fatal(
                    e,
                    $"Could not execute function {nameof(HandleMessage)} for message type ${myQueueItem.Label ?? "unknown"} with id {myQueueItem.MessageId ?? "unknown"}");
                log.LogCritical(e.ToString());
            }
        }
    }
}
