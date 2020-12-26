namespace Soap.PfBase.Api.Functions
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.SignalRService;
    using Microsoft.Extensions.Logging;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public static class Functions
    {
        public static HttpResponseMessage
            CheckHealth<TInboundMessage, TOutboundMessage, TSendLargeMsg, TReceiveLargeMsg, TIdentity>(
                HttpRequest req,
                MapMessagesToFunctions handlerRegistration,
                IAsyncCollector<SignalRMessage> signalRBinding,
                ILogger log)
            where TInboundMessage : ApiCommand, new()
            where TIdentity : class, IApiIdentity, new()
            where TOutboundMessage : ApiEvent
            where TSendLargeMsg : ApiCommand, new()
            where TReceiveLargeMsg : ApiMessage
        {
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);

                var content = new PushStreamContent(
                    async (outputSteam, httpContent, transportContext) =>
                        await DiagnosticFunctions
                            .OnOutputStreamReadyToBeWrittenTo<TInboundMessage, TOutboundMessage, TSendLargeMsg, TReceiveLargeMsg,
                                TIdentity>(
                                outputSteam,
                                httpContent,
                                transportContext,
                                typeof(TInboundMessage).Assembly,
                                $"{req.Scheme}://{req.Host.ToUriComponent()}",
                                handlerRegistration,
                                signalRBinding,
                                logger),
                    new MediaTypeHeaderValue("text/plain"));

                var result = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = content
                };

                return result;
            }
            catch (Exception e)
            {
                logger?.Fatal(e, $"Could not execute function {nameof(CheckHealth)}");
                log.LogCritical(e.ToString());

                var result = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(e.ToString(), Encoding.UTF8, "text/plain")
                };

                return result;
            }
        }
    }
}
