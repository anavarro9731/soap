namespace Soap.Api.Sample.Afs
{
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
    using Soap.Config;
    using Soap.PfBase.Api.Functions;

    public static class BuiltIn
    {
        [FunctionName("AddBlob")]
        public static async Task<IActionResult> AddBlob(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            ILogger log) =>
            await PlatformFunctions.AddBlob(req, log);

        [FunctionName("AddToGroups")]
        public static async Task AddToGroup(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
            HttpRequest req,
            [SignalRTrigger("SoapApiSampleHub", "connections", "Connected")]
            InvocationContext invocationContext,
            ILogger logger,
            [SignalR(HubName = "SoapApiSampleHub", ConnectionStringSetting = "AzureSignalRConnectionString")]
            IAsyncCollector<SignalRGroupAction> signalRGroupActions)
        {
            var connectionId = invocationContext.ConnectionId;

            await signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    GroupName = EnvVars.GroupKey,
                    ConnectionId = connectionId,
                    Action = GroupAction.Add
                });
        }

        [FunctionName("CheckHealth")]
        public static HttpResponseMessage CheckHealth(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            [SignalR(HubName = "SoapApiSampleHub", ConnectionStringSetting = "AzureSignalRConnectionString")]
            IAsyncCollector<SignalRMessage> signalRBinding,
            ILogger log) =>
            Functions.CheckHealth<C100v1_Ping, E100v1_Pong, C105v1_SendLargeMessage, C106v1_LargeCommand, User>(
                req,
                new HandlerRegistration(),
                signalRBinding,
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

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequest req,
            [SignalRConnectionInfo(HubName = "SoapApiSampleHub")]
            SignalRConnectionInfo connectionInfo) =>
            connectionInfo;

        [FunctionName("PrintSchema")]
        public static IActionResult PrintSchema(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log) =>
            PlatformFunctions.PrintSchema(log, typeof(C100v1_Ping).Assembly);

        [FunctionName("ReceiveMessage")]
        public static async Task ReceiveMessage(
            [ServiceBusTrigger(
                "Soap.Api.Sample.Messages",
                Connection = "AzureWebJobsServiceBus")]
            //* this uses peeklockmode
            Message myQueueItem,
            string messageId,
            [SignalR(HubName = "SoapApiSampleHub", ConnectionStringSetting = "AzureSignalRConnectionString")]
            IAsyncCollector<SignalRMessage> signalRBinding,
            ILogger log)
        {
            await PlatformFunctions.HandleMessage<User>(myQueueItem, messageId, new HandlerRegistration(), signalRBinding, log);
        }

        [FunctionName("ValidateMessage")]
        public static async Task<IActionResult> ValidateMessage(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            ILogger log) =>
            await PlatformFunctions.ValidateMessage(req);
    }
}
