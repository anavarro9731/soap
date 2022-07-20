namespace Soap.PfBase.Api.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using DataStore.Interfaces.LowLevel;
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
            CheckHealth<TPing, TPong, TSendLargeMsg, TLargeMsg, TUserProfile>(
                HttpRequest req,
                MapMessagesToFunctions messageFunctionRegistration,
                IAsyncCollector<SignalRMessage> signalRBinding,
                ISecurityInfo securityInfo,
                ILogger log,
                IEnumerable<ApiCommand> startupCommands = null)
            where TPing : ApiCommand, new()
            where TPong : ApiEvent
            where TSendLargeMsg : ApiCommand, new()
            where TLargeMsg : ApiMessage
            where TUserProfile : class, IUserProfile, IAggregate, new()
        {
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);

                var content = new PushStreamContent(
                    async (outputSteam, httpContent, transportContext) =>
                        await DiagnosticFunctions
                            .OnOutputStreamReadyToBeWrittenTo<TPing, TPong, TSendLargeMsg, TLargeMsg, TUserProfile>(
                                outputSteam,
                                httpContent,
                                req,
                                transportContext,
                                typeof(TPing).Assembly,
                                $"{req.Scheme}://{req.Host.ToUriComponent()}",
                                messageFunctionRegistration,
                                signalRBinding,
                                securityInfo,
                                logger,
                                startupCommands),
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
