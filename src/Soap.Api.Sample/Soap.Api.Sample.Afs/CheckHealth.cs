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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
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
                    Content = GetContent<C100Ping, E150Pong, C105SendLargeMessage, C106LargeCommand, User>(new MappingRegistration(), logger, $"{req.Scheme}://{req.Host.ToUriComponent()}")
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

            static PushStreamContent GetContent<TInboundMessage, TOutboundMessage, TSendLargeMsg, TReceiveLargeMsg, TIdentity>(
                MapMessagesToFunctions messageMapper,
                Serilog.ILogger logger,
                string functionHost)
                where TInboundMessage : ApiCommand, new()
                where TIdentity : class, IApiIdentity, new()
                where TOutboundMessage : ApiEvent
                where TSendLargeMsg : ApiCommand, new() 
                where TReceiveLargeMsg : ApiMessage
            {
                return new PushStreamContent(
                    async (outputSteam, httpContent, transportContext) =>
                        await DiagnosticFunctions.OnOutputStreamReadyToBeWrittenTo<TInboundMessage, TOutboundMessage, TSendLargeMsg, TReceiveLargeMsg, TIdentity>(
                            outputSteam,
                            httpContent,
                            transportContext,
                            typeof(TInboundMessage).Assembly,
                            functionHost,
                            messageMapper, logger),
                    new MediaTypeHeaderValue("text/plain"));
            }
        }
    }
}