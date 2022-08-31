namespace Soap.PfBase.Api.Functions
{
    using System;
    using System.IO;
    using System.Runtime.ExceptionServices;
    using System.Text;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.SignalRService;
    using Microsoft.Extensions.Logging;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces;
    using Soap.Utility;
    using Soap.Utility.Enums;

    public static partial class PlatformFunctions
    {
        
        public static async Task HandleMessage<TUserProfile>(
            string body,
            string messageId,
            string type,
            MapMessagesToFunctions messageFunctionRegistration,
            ISecurityInfo securityInfo,
            IAsyncCollector<SignalRMessage> signalRBinding,
            ILogger log,
            DataStoreOptions dataStoreOptions = null) where TUserProfile : class, IHaveIdaamProviderId, IUserProfile, IAggregate, new()
        {
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);

                AzureFunctionContext.LoadAppConfig(out var appConfig);

                var result = await AzureFunctionContext.Execute<TUserProfile>(
                                 body,
                                 messageFunctionRegistration,
                                 messageId,
                                 type,
                                 securityInfo,
                                 logger,
                                 appConfig,
                                 signalRBinding, 
                                 dataStoreOptions);

                if (result.Success == false)
                {
                    
                    ExceptionDispatchInfo.Capture(result.UnhandledError).Throw();
                }
            }
            catch (Exception e)
            {
                logger?.Fatal(
                    e,
                    $"Could not execute function {nameof(HandleMessage)} for message type ${type ?? "unknown"} with id {messageId ?? "unknown"}");
                log.LogCritical(e.ToString());
            }
        }
        
        public static async Task HandleMessage<TUserProfile>(
            Message myQueueItem,
            string messageId,
            MapMessagesToFunctions messageFunctionRegistration,
            ISecurityInfo securityInfo,
            IAsyncCollector<SignalRMessage> signalRBinding,
            ILogger log,
            DataStoreOptions dataStoreOptions = null) where TUserProfile : class, IHaveIdaamProviderId, IUserProfile, IAggregate, new()
        {
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);

                AzureFunctionContext.LoadAppConfig(out var appConfig);

                var result = await AzureFunctionContext.Execute<TUserProfile>(
                                 Encoding.UTF8.GetString(myQueueItem.Body),
                                 messageFunctionRegistration,
                                 messageId,
                                 myQueueItem.Label,
                                 securityInfo,
                                 logger,
                                 appConfig,
                                 signalRBinding, 
                                 dataStoreOptions);

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
        
        public static async Task HandleMessage<TUserProfile>(
            HttpRequest req,
            MapMessagesToFunctions messageFunctionRegistration,
            ISecurityInfo securityInfo,
            IAsyncCollector<SignalRMessage> signalRBinding,
            ILogger log,
            DataStoreOptions dataStoreOptions = null) where TUserProfile : class, IHaveIdaamProviderId, IUserProfile, IAggregate, new()
        {
            Serilog.ILogger logger = null;
            string messageType = null;
            string messageId = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);

                AzureFunctionContext.LoadAppConfig(out var appConfig);
                
                using StreamReader streamReader = new StreamReader(req.Body, Encoding.UTF8);
                    
                string requestBody = await streamReader.ReadToEndAsync();
                
                GetMessageId(req, out messageId);
                GetMessageTypeString(req, out messageType);
                
                var result = await AzureFunctionContext.Execute<TUserProfile>(
                                 requestBody,
                                 messageFunctionRegistration,
                                 messageId,
                                 messageType,
                                 securityInfo,
                                 logger,
                                 appConfig,
                                 signalRBinding, 
                                 dataStoreOptions);

                if (result.Success == false)
                {
                    ExceptionDispatchInfo.Capture(result.UnhandledError).Throw();
                }
                
                static void GetMessageTypeString(HttpRequest req, out string messageType)
                {
                    messageType = req.Query["type"];
                    Guard.Against(
                        string.IsNullOrWhiteSpace(messageType),
                        "Could not find a value for the Type of Message from Querystring param 'type'",
                        ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly);
                }
                
                static void GetMessageId(HttpRequest req, out string messageId)
                {
                    string idParameter = req.Query["id"];
                    var parsed = Guid.TryParse(idParameter, out var id);
                    Guard.Against(
                        !parsed,
                        "Could not parse Id of Message from Querystring param 'id'",
                        ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly);
                    messageId = id.ToString();
                }
                
            }
            catch (Exception e)
            {
                logger?.Fatal(
                    e,
                    $"Could not execute function {nameof(HandleMessage)} for message type ${messageType ?? "unknown"} with id {messageId ?? "unknown"}");
                log.LogCritical(e.ToString());
            }
        }
    }
    
}
