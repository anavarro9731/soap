namespace Soap.PfBase.Api
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Providers.CosmosDb;
    using global::Auth0.ManagementApi;
    using global::Auth0.ManagementApi.Models;
    using global::Auth0.ManagementApi.Paging;
    using Mainwave.MimeTypes;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.SignalRService;
    using Microsoft.CSharp.RuntimeBinder;
    using Newtonsoft.Json;
    using RestSharp;
    using Serilog;
    using Soap.Bus;
    using Soap.Config;
    using Soap.Context;
    using Soap.Context.BlobStorage;
    using Soap.Context.Logging;
    using Soap.Context.MessageMapping;
    using Soap.Idaam;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;

    public static class DiagnosticFunctions
    {
        public static CachedSchema GetSchema(ApplicationConfig appConfig, Assembly messagesAssembly)
        {
            IEnumerable<ApiMessage> messages = messagesAssembly.GetTypes()
                                                               .Where(t => t.InheritsOrImplements(typeof(ApiMessage)))
                                                               .Select(x => Activator.CreateInstance(x) as ApiMessage)
                                                               .ToList();

            var schema = new CachedSchema(appConfig, messages);

            return schema;
        }

        public static async Task OnOutputStreamReadyToBeWrittenTo<TPing, TPong, TSendLargeMsg, TLargeMsg, TUserProfile>(
            Stream outputStream,
            HttpContent httpContent,
            HttpRequest httpRequest,
            TransportContext transportContext,
            Assembly messagesAssembly,
            string functionHost,
            MapMessagesToFunctions mapMessagesToFunctions,
            IAsyncCollector<SignalRMessage> signalRBinding,
            ISecurityInfo securityInfo,
            ILogger logger,
            IEnumerable<ApiCommand> startupCommands)
            where TPing : ApiCommand, new()
            where TPong : ApiMessage
            where TSendLargeMsg : ApiCommand, new()
            where TLargeMsg : ApiMessage
            where TUserProfile : class, IUserProfile, IAggregate, new()
        {
            ApplicationConfig appConfig = null;
            try
            {
                logger.Information("Starting Health Check");
                
                AzureFunctionContext.LoadAppConfig(out appConfig);

                /* WARNING DO NOT WRITE ANYTHING IF YOU ARE NOT IN DEV ENV OR USING INSTRUMENTATION KEY
                EXPOSING THINGS LIKE MESSAGE HEADERS ON HEALTH CHECK MESSAGES OR OTHER INTERNAL DETAILS COULD EXPOSE DETAILS ATTACKER 
                COULD USE TO COMPROMISE SYSTEM, POTENTIALLY AT A LOW LEVEL SINCE THE HEALTH CHECK RUNS WITH SERVICE LEVEL AUTHORITY */
                await PrintToOutputStreamIfInSecureEnvironment("RUNNING IN SECURE ENVIRONMENT. DETAILS WILL BE SHOWN THAT ARE NOT PRINTED IN OTHER ENVIRONMENTS.");

                await PrintToOutputStreamIfInSecureEnvironment("Loading Config...");

                await PrintToOutputStreamIfInSecureEnvironment(GetConfigDetails(appConfig));

                await CheckDatabaseExists(appConfig, PrintToOutputStreamIfInSecureEnvironment);

                await CheckServiceBusConfiguration(appConfig, messagesAssembly, mapMessagesToFunctions, logger, PrintToOutputStreamIfInSecureEnvironment);

                await CheckBlobStorage(appConfig, new MessageAggregator(), PrintToOutputStreamIfInSecureEnvironment, functionHost);
                
                bool success1 = await SendMessageWaitForReply<TPing, TPong, TUserProfile>(
                    logger,
                    appConfig,
                    securityInfo,
                    mapMessagesToFunctions,
                    signalRBinding,
                    PrintToOutputStreamIfInSecureEnvironment);

                bool success2 = await SendMessageWaitForReply<TSendLargeMsg, TLargeMsg, TUserProfile>(
                    logger,
                    appConfig,
                    securityInfo,
                    mapMessagesToFunctions,
                    signalRBinding,
                    PrintToOutputStreamIfInSecureEnvironment);

                var idaamProvider = new IdaamProvider(appConfig);
                await CheckAuth0Setup(idaamProvider, securityInfo, appConfig, PrintToOutputStreamIfInSecureEnvironment);
                
                
                //* this needs 2 run last, order important
                List<bool> startupCommandResults = new List<bool>();
                if (startupCommands != null)
                {
                    await PrintToOutputStreamIfInSecureEnvironment("Running Startup Commands ...");
                    foreach (var startupCommand in startupCommands)
                    {
                        var startupCommandSuccess = await ExecuteCommandInline<TUserProfile>(startupCommand,                   
                                                        logger,
                                                        appConfig,
                                                        securityInfo,
                                                        mapMessagesToFunctions,
                                                        signalRBinding,
                                                        PrintToOutputStreamIfInSecureEnvironment);
                        startupCommandResults.Add(startupCommandSuccess);
                        
                    }
                }
                
                var healthCheckCompleted = "Health Check Completed";
                logger.Information(healthCheckCompleted);
                
                var finalString= healthCheckCompleted + Environment.NewLine + ((success1 && success2 && startupCommandResults.All(x => x)) ? "+" : "-");
                await outputStream.WriteAsync(Encoding.UTF8.GetBytes(finalString));
                
                
                async ValueTask PrintToOutputStreamIfInSecureEnvironment(string s)
                {
                    /* WARNING DO NOT WRITE ANYTHING IF YOU ARE NOT IN DEV ENV OR USING INSTRUMENTATION KEY
                    EXPOSING THINGS LIKE MESSAGE HEADERS ON HEALTH CHECK MESSAGES OR OTHER INTERNAL DETAILS COULD EXPOSE DETAILS ATTACKER 
                    COULD USE TO COMPROMISE SYSTEM, POTENTIALLY AT A LOW LEVEL SINCE THE HEALTH CHECK RUNS WITH SERVICE LEVEL AUTHORITY */
                    if (appConfig.Environment == SoapEnvironments.Development || httpRequest.Query["key"] == Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY"))
                    {
                        await outputStream.WriteAsync(Encoding.UTF8.GetBytes($"{s}{Environment.NewLine}"));
                        await outputStream.FlushAsync();
                    }
                }
            }
            catch (Exception e)
            {
                if (appConfig?.Environment == SoapEnvironments.Development)
                {
                    /* WARNING DO NOT WRITE ANYTHING TO THE CONSOLE IF YOU ARE NOT IN DEV ENVIRONMENT AFTER THIS UNLESS YOU HAVE THE SPECIAL KEY
                    EXPOSING THINGS LIKE MESSAGE HEADERS ON HEALTH CHECK MESSAGES OR OTHER INTERNAL DETAILS COULD EXPOSE DETAILS ATTACKER 
                    COULD USE TO COMPROMISE SYSTEM, POTENTIALLY AT A LOW LEVEL*/
                    await outputStream.WriteAsync(Encoding.UTF8.GetBytes(e.ToString()));    
                }
                
                logger.Error(e, "Error Checking Health {0}");
            }
            finally
            {
                await outputStream.FlushAsync();

                await outputStream.DisposeAsync();
            }
            
            static async Task CheckAuth0Setup(
                IdaamProvider idaamProvider,
                ISecurityInfo securityInfo,
                ApplicationConfig applicationConfig,
                Func<string, ValueTask> writeLine)
            {
                {
                    if (ConfigIsEnabledForAuth0Integration(applicationConfig))
                    {
                        if (await idaamProvider.IsApiRegisteredWithProvider())
                        {
                            await idaamProvider.UpdateApiRegistrationWithProvider(securityInfo, writeLine);
                        }
                        else
                        {
                            await idaamProvider.RegisterApiWithProvider(securityInfo, writeLine);
                        }
                    }

                    static bool ConfigIsEnabledForAuth0Integration(ApplicationConfig applicationConfig) => applicationConfig.AuthLevel.Enabled;
                }
            }
        }

        private static async Task CheckBlobStorage(
            ApplicationConfig applicationConfig,
            IMessageAggregator messageAggregator,
            Func<string, ValueTask> writeLine,
            string functionHost)
        {
            var blobClient = new BlobStorage(
                new BlobStorage.Settings(applicationConfig.StorageConnectionString, messageAggregator));

            var base64String =
                @"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";

            var blobId = Guid.NewGuid();

            await blobClient.SaveBase64StringAsBlob(base64String, blobId, MimeType.Image.Png);
            await writeLine("Test Blob Saved Successfully");

            var blob = await blobClient.GetBlobOrError(blobId);
            Guard.Against(blob.Type.TypeClass != Blob.TypeClass.Mime, "Mime type not retrieved successfully");
            Guard.Against(blob.Type.TypeString != MimeType.Image.Png, "Mime type not retrieved successfully");
            var base64String2 = Convert.ToBase64String(blob.Bytes);
            Guard.Against(base64String != base64String2, "Blob did not return correct bytes");

            await writeLine($"Test Blob Retrieved Successfully. Url: {functionHost}/api/GetBlob?id={blobId.ToString()}");
        }

        private static async Task CheckDatabaseExists(ApplicationConfig applicationConfig, Func<string, ValueTask> writeLine)
        {
            //TODO make cosmossettings visible so you can read the values from there in case EnvPartitionKey and ContainerName diverge in future *FRAGILE* 
            await writeLine(
                $"Creating Container {EnvVars.EnvironmentPartitionKey} on Database {EnvVars.CosmosDbDatabaseName} if it doesn't exist...");
            await new CosmosDbUtilities().CreateDatabaseIfNotExists(applicationConfig.DatabaseSettings);
        }

        private static async Task CheckServiceBusConfiguration(
            ApplicationConfig applicationConfig,
            Assembly messagesAssembly,
            MapMessagesToFunctions mapMessagesToFunctions,
            ILogger logger,
            Func<string, ValueTask> writeLine)
        {
            {
                await writeLine("Checking Service Bus Configuration...");

                var busSettings = applicationConfig.BusSettings as AzureBus.Settings;
                Guard.Against(busSettings == null, "Expected type of AzureBus");

                await ServiceBusManagementFunctions.CreateQueueIfNotExist(messagesAssembly, busSettings, writeLine);

                await ServiceBusManagementFunctions.CreateTopicsIfNotExist(busSettings, messagesAssembly, writeLine);

                await ServiceBusManagementFunctions.CreateSubscriptionsIfNotExist(
                    busSettings,
                    mapMessagesToFunctions,
                    logger,
                    messagesAssembly,
                    writeLine);
            }
        }

        private static string GetConfigDetails(ApplicationConfig appConfig)
        {
            var ipAddress = Dns.GetHostEntryAsync(Dns.GetHostName())
                               .Result.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                               .ToString();

            var version = Assembly.GetEntryAssembly().GetName().Version.ToString(3);

            var configAsObject = new
            {
                HealthCheckedAt = DateTime.UtcNow,
                ApplicationName = appConfig.AppFriendlyName,
                appConfig.ApplicationVersion,
                appConfig.AppId,
                Environment = appConfig.Environment.Value,
                appConfig.DefaultExceptionMessage,
                EntryAssemblyVersion = version,
                Environment.MachineName,
                IpAddress = ipAddress
            };

            var json = configAsObject.ToJson(SerialiserIds.JsonDotNetDefault);

            return json;
        }

        private static async Task<bool> SendMessageWaitForReply<TSent, TReply, TUserProfile>(
            ILogger logger,
            ApplicationConfig appConfig,
            ISecurityInfo securityInfo,
            MapMessagesToFunctions mappings,
            IAsyncCollector<SignalRMessage> signalRBinding,
            Func<string, ValueTask> writeLine)
            where TSent : ApiCommand, new() where TReply : ApiMessage
            where TUserProfile : class, IUserProfile, IAggregate, new()

        {
            await writeLine("Running Message Test...");

            var message = new TSent();
            var sla = AuthorisationSchemes.GetServiceLevelAuthority(appConfig);
            message.Headers.SetAccessToken(sla.AccessToken);
            message.Headers.SetIdentityChain(sla.IdentityChainSegment);
            message.Headers.SetIdentityToken(sla.IdentityToken);
            message.SetDefaultHeadersForIncomingTestMessages();
            
            await writeLine($"Sending {typeof(TSent).Name} ...");

            //*  should publish/send pong
            var r = await AzureFunctionContext.Execute<TUserProfile>(
                        message.ToJson(SerialiserIds.ApiBusMessage),
                        mappings,
                        message.Headers.GetMessageId().ToString(),
                        message.GetType().ToShortAssemblyTypeName(),
                        securityInfo,
                        logger,
                        appConfig,
                        signalRBinding);

            await writeLine(r.ToJson(SerialiserIds.JsonDotNetDefault));

            if (r.Success)
            {
                Guid pongId;
                if (typeof(TReply).InheritsOrImplements(typeof(ApiEvent)))
                {
                    pongId = r.PublishedMessages.Single().Headers.GetMessageId();
                }
                else
                {
                    pongId = r.CommandsSent.Single().Headers.GetMessageId();
                }

                await writeLine($"Waiting for {typeof(TReply).Name} with id {pongId}");
                var tries = 15;
                while (tries > 0)
                {
                    await writeLine("Waiting 1 seconds ...");
                    await Task.Delay(1000);
                    var logged =
                        await new DataStore(appConfig.DatabaseSettings.CreateRepository()).ReadById<MessageLogEntry>(pongId, options => options.ProvidePartitionKeyValues(WeekInterval.FromUtcNow()));
                    if (logged != null && logged.ProcessingComplete)
                    {
                        await writeLine($"Received {typeof(TReply).Name} Message Test Succeeded.");
                        return true;
                    }

                    tries--;
                }

                await writeLine($"Did not receive {typeof(TReply).Name} response!. Message Test Failure.");
                return false;
            }

            return false;
        }

        private static async Task<bool> ExecuteCommandInline<TUserProfile>(
            ApiCommand message,
            ILogger logger,
            ApplicationConfig appConfig,
            ISecurityInfo securityInfo,
            MapMessagesToFunctions mappings,
            IAsyncCollector<SignalRMessage> signalRBinding,
            Func<string, ValueTask> writeLine)
            where TUserProfile : class, IUserProfile, IAggregate, new()

        {
            var sla = AuthorisationSchemes.GetServiceLevelAuthority(appConfig);
            message.Headers.SetAccessToken(sla.AccessToken);
            message.Headers.SetIdentityChain(sla.IdentityChainSegment);
            message.Headers.SetIdentityToken(sla.IdentityToken);
            message.SetDefaultHeadersForIncomingTestMessages();
            
            await writeLine($"Sending {message.GetType().Name} ...");
            
            var r = await AzureFunctionContext.Execute<TUserProfile>(
                        message.ToJson(SerialiserIds.ApiBusMessage),
                        mappings,
                        message.Headers.GetMessageId().ToString(),
                        message.GetType().ToShortAssemblyTypeName(),
                        securityInfo,
                        logger,
                        appConfig,
                        signalRBinding);

            await writeLine(r.ToJson(SerialiserIds.JsonDotNetDefault));

            return r.Success;
        }
    }
}
