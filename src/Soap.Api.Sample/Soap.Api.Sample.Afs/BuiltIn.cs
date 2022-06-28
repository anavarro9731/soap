namespace Soap.Api.Sample.Afs
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using DataStore.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Extensions.SignalRService;
    using Microsoft.Extensions.Logging;
    using Soap.Api.Sample.Afs.Security;
    using Soap.Api.Sample.Constants;
    using Soap.Api.Sample.Logic;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Config;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Api.Functions;

    public static class BuiltIn
    {
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
            [SignalR(HubName = "SoapApiSampleHub", ConnectionStringSetting = "AzureSignalRConnectionString")]
            IAsyncCollector<SignalRMessage> signalRBinding,
            ILogger log) =>
            Functions.CheckHealth<C100v1_Ping, E100v1_Pong, C105v1_SendLargeMessage, C106v1_LargeCommand, UserProfile>(
                req,
                new MessageFunctionRegistration(),
                signalRBinding,
                new SecurityInfo(),
                log, 
                new ApiCommand[]{new C101v1_UpgradeTheDatabase(ReleaseVersions.V1), new C115v1_OnStartup()});

        [FunctionName("GetBlob")]
        public static async Task<IActionResult> GetBlob(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log) =>
            await PlatformFunctions.GetBlob(req, log);

        [FunctionName("GetJsonSchema")]
        public static async Task<IActionResult> GetJsonSchema(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log) =>
            await PlatformFunctions.GetJsonSchema(req, log, typeof(C100v1_Ping).Assembly);

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

        [FunctionName("ReceiveMessageHttp")]
        public static async  Task ReceiveMessageHttp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            [SignalR(HubName = "SoapApiSampleHub", ConnectionStringSetting = "AzureSignalRConnectionString")]
            IAsyncCollector<SignalRMessage> signalRBinding,
            ILogger log) =>
            await PlatformFunctions.HandleMessage<UserProfile>(req, new MessageFunctionRegistration(), new SecurityInfo(), signalRBinding, log);

        
        [FunctionName("ReceiveMessage")]
        public static async Task ReceiveMessage(
            [ServiceBusTrigger("Soap.Api.Sample.Messages.%EnvironmentPartitionKey%", Connection = "AzureWebJobsServiceBus")]
            //* this uses PeekLockMode
            Message myQueueItem,
            string messageId,
            [SignalR(HubName = "SoapApiSampleHub", ConnectionStringSetting = "AzureSignalRConnectionString")]
            IAsyncCollector<SignalRMessage> signalRBinding,
            ILogger log)
        {
            await PlatformFunctions.HandleMessage<UserProfile>(myQueueItem, messageId, new MessageFunctionRegistration(), new SecurityInfo(), signalRBinding, log);
        }

        [FunctionName("AddToGroup")]
        public static Task SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
            HttpRequest req,
            [SignalR(HubName = "SoapApiSampleHub")] IAsyncCollector<SignalRGroupAction> groups,
            ILogger logger)
        {
            var connectionId = req.Query["connectionId"];
            if (string.IsNullOrEmpty(connectionId))
            {
                logger.LogCritical("Missing Connection ID Parameter");
                throw new ArgumentException("Missing Connection ID Parameter");
            }

            return groups.AddAsync(
                new SignalRGroupAction
                {
                    Action = GroupAction.Add,
                    ConnectionId = connectionId,
                    GroupName = EnvVars.EnvironmentPartitionKey
                });
        }

        [FunctionName("ValidateMessage")]
        public static async Task<IActionResult> ValidateMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            ILogger log) =>
            await PlatformFunctions.ValidateMessage(req);
    }
}
