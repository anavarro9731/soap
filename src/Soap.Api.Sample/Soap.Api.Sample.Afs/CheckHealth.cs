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
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.Pf.HttpEndpointBase.Controllers;

    public static class CheckHealth
    {
        [FunctionName("CheckHealth")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            try
            {
                var result = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = GetContent<C100Ping, User>(new MappingRegistration())
                };

                return result;
            }
            catch (Exception e)
            {
                log.LogCritical(e.ToString());

                var result = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(e.ToString(), Encoding.UTF8, "text/plain")
                };

                return result;
            }

            static PushStreamContent GetContent<TInboundMessage, TIdentity>(MapMessagesToFunctions messageMapper)
                where TInboundMessage : ApiMessage, new() where TIdentity : class, IApiIdentity, new()
            {
                return new PushStreamContent(
                    async (outputSteam, httpContent, transportContext) =>
                        await DiagnosticFunctions.OnOutputStreamReadyToBeWrittenTo<TInboundMessage, TIdentity>(
                            outputSteam,
                            httpContent,
                            transportContext,
                            typeof(TInboundMessage).Assembly,
                            messageMapper),
                    new MediaTypeHeaderValue("text/plain"));
            }
        }
    }
}