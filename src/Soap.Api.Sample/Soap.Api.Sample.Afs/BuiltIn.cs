namespace Soap.Api.Sample.Afs
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Extensions.SignalRService;
    using Microsoft.Extensions.Logging;
    using Soap.Api.Sample.Logic;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Api.Functions;
    using Soap.Utility.Functions.Extensions;

    public static class BuiltIn
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "SoapApiSampleHub")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("AddBlob")]
        public static async Task<IActionResult> AddBlob(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            ILogger log) =>
            await PlatformFunctions.AddBlob(req, log);

        [FunctionName("CheckHealth")]
        public static HttpResponseMessage CheckHealth(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log) =>
            Functions.CheckHealth<C100v1_Ping, E100v1_Pong, C105v1_SendLargeMessage, C106v1_LargeCommand, User>(
                req,
                new HandlerRegistration(),
                log);

        [FunctionName("GetBlob")]
        public static async Task<IActionResult> GetBlob(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log) =>
            await PlatformFunctions.GetBlob(req, log);

        [FunctionName("GetJsonSchema")]
        public static IActionResult GetJsonSchema(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log) =>
            PlatformFunctions.GetJsonSchema(log, typeof(C100v1_Ping).Assembly);

        [FunctionName("PrintSchema")]
        public static IActionResult PrintSchema(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log) =>
            PlatformFunctions.PrintSchema(log, typeof(C100v1_Ping).Assembly);

        [FunctionName("ReceiveMessage")]
        public static async Task ReceiveMessage(
            [ServiceBusTrigger("Soap.Api.Sample.Messages", Connection = "AzureWebJobsServiceBus")] //* this uses peeklockmode
            Message myQueueItem,
            string messageId,
            ILogger log)
        {
            await PlatformFunctions.HandleMessage<User>(myQueueItem, messageId, new HandlerRegistration(), log);
        }

        [FunctionName("SendSignalRMessage")]
        public static async Task SendSignalRMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            [SignalR(HubName = "SoapApiSampleHub", ConnectionStringSetting = "AzureSignalRConnectionString")]
            IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger logger)
        {
            string userId = req.Query["userId"];
            string type = req.Query["type"];
            var t = Type.GetType(type);
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var msg = (ApiMessage)requestBody.FromJson(t, SerialiserIds.ApiBusMessage);
        
            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    UserId = userId, // the message will only be sent to this user id, leave this off for broadcast
                    Target = "eventReceived", //client side function name
                    Arguments = new[] { "^^^" + msg.ToJson(SerialiserIds.ApiBusMessage) } 
                    /* don't let signalr do the serialising or it will use the wrong JSON settings, it's smart and it will recognise a JSON string, fool it with ^^^ */
                });
        }

        [FunctionName("ValidateMessage")]
        public static async Task<IActionResult> ValidateMessage(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            ILogger log) =>
            await PlatformFunctions.ValidateMessage(req);
    }
}
