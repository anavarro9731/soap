namespace Soap.Api.Sample.Afs
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Soap.Api.Sample.Logic;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Api;

    public static class CheckHealth
    {
        [FunctionName("CheckHealth")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);
                
                var result = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = GetContent<C100Ping, E150Pong, User>(new MappingRegistration(), logger)
                };

                return result;
            }
            catch (Exception e)
            {
                logger?.Fatal(e, "Could not execute function");
                log.LogCritical(e.ToString());

                var result = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(e.ToString(), Encoding.UTF8, "text/plain")
                };

                return result;
            }

            static PushStreamContent GetContent<TInboundMessage, TOutboundMessage, TIdentity>(
                MapMessagesToFunctions messageMapper,
                Serilog.ILogger logger)
                where TInboundMessage : ApiCommand, new()
                where TIdentity : class, IApiIdentity, new()
                where TOutboundMessage : ApiEvent
            {
                return new PushStreamContent(
                    async (outputSteam, httpContent, transportContext) =>
                        await DiagnosticFunctions.OnOutputStreamReadyToBeWrittenTo<TInboundMessage, TOutboundMessage, TIdentity>(
                            outputSteam,
                            httpContent,
                            transportContext,
                            typeof(TInboundMessage).Assembly,
                            messageMapper, logger),
                    new MediaTypeHeaderValue("text/plain"));
            }
        }
    }
}