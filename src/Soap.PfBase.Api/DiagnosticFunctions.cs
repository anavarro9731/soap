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
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Providers.CosmosDb;
    using Mainwave.MimeTypes;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.SignalRService;
    using Serilog;
    using Soap.Bus;
    using Soap.Config;
    using Soap.Context;
    using Soap.Context.BlobStorage;
    using Soap.Context.Logging;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.MessageAggregator;
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

        public static async Task OnOutputStreamReadyToBeWrittenTo<TPing, TPong, TSendLargeMsg, TReceiveLargeMsg, TIdentity>(
            Stream outputStream,
            HttpContent httpContent,
            TransportContext transportContext,
            Assembly messagesAssembly,
            string functionHost,
            MapMessagesToFunctions mapMessagesToFunctions,
            IAsyncCollector<SignalRMessage> signalRBinding,
            ILogger logger)
            where TPing : ApiCommand, new()
            where TIdentity : class, IApiIdentity, new()
            where TPong : ApiMessage
            where TSendLargeMsg : ApiCommand, new()
            where TReceiveLargeMsg : ApiMessage
        {
            async ValueTask WriteLine(string s)
            {
                await outputStream.WriteAsync(Encoding.UTF8.GetBytes($"{s}{Environment.NewLine}"));
                await outputStream.FlushAsync();
            }

            try
            {
                logger.Information("Starting Health Check");

                await WriteLine("Loading Config...");

                AzureFunctionContext.LoadAppConfig(out var appConfig);

                await WriteLine(GetConfigDetails(appConfig));

                await CheckDatabaseExists(appConfig, WriteLine);

                await CheckServiceBusConfiguration(appConfig, messagesAssembly, mapMessagesToFunctions, logger, WriteLine);

                await CheckBlobStorage(appConfig, new MessageAggregator(), WriteLine, functionHost);

                await GetPingPongMessageTestResults<TPing, TPong, TIdentity>(
                    logger,
                    appConfig,
                    mapMessagesToFunctions,
                    signalRBinding,
                    WriteLine);

                await GetPingPongMessageTestResults<TSendLargeMsg, TReceiveLargeMsg, TIdentity>(
                    logger,
                    appConfig,
                    mapMessagesToFunctions,
                    signalRBinding,
                    WriteLine);

                logger.Information("Health Check Completed");
            }
            catch (Exception e)
            {
                await outputStream.WriteAsync(Encoding.UTF8.GetBytes(e.ToString()));
                logger.Error(e, "Error Checking Health {0}");
            }
            finally
            {
                await outputStream.FlushAsync();

                await outputStream.DisposeAsync();
            }
        }

        private static async Task CheckDatabaseExists(ApplicationConfig applicationConfig, Func<string, ValueTask> writeLine)
        {
            //TODO make cosmossettings visible so you can read the values from there in case these diverge in future *FRAGILE* 
            await writeLine($"Creating Container {EnvVars.EnvironmentPartitionKey} on Database {EnvVars.CosmosDbDatabaseName} if it doesn't exist...");
            await new CosmosDbUtilities().CreateDatabaseIfNotExists(applicationConfig.DatabaseSettings);
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

            var blob = await blobClient.GetBlob(blobId);
            Guard.Against(blob.Type.TypeClass != Blob.TypeClass.Mime, "Mime type not retrieved successfully");
            Guard.Against(blob.Type.TypeString != MimeType.Image.Png, "Mime type not retrieved successfully");
            var base64String2 = Convert.ToBase64String(blob.Bytes);
            Guard.Against(base64String != base64String2, "Blob did not return correct bytes");

            await writeLine($"Test Blob Retrieved Successfully. Url: {functionHost}/api/GetBlob?id={blobId.ToString()}");
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

        private static async Task GetPingPongMessageTestResults<TPing, TPong, TIdentity>(
            ILogger logger,
            ApplicationConfig appConfig,
            MapMessagesToFunctions mappings,
            IAsyncCollector<SignalRMessage> signalRBinding,
            Func<string, ValueTask> writeLine)
            where TPing : ApiCommand, new() where TIdentity : class, IApiIdentity, new() where TPong : ApiMessage

        {
            await writeLine("Running Message Test...");

            var message = new TPing();
            message.SetDefaultHeadersForIncomingTestMessages();

            await writeLine($"Sending {typeof(TPing).Name} ...");

            //*  should publish/send pong
            var r = await AzureFunctionContext.Execute<TIdentity>(
                        message.ToJson(SerialiserIds.ApiBusMessage),
                        mappings,
                        message.Headers.GetMessageId().ToString(),
                        message.GetType().ToShortAssemblyTypeName(),
                        logger,
                        appConfig,
                        signalRBinding);

            await writeLine(r.ToJson(SerialiserIds.JsonDotNetDefault));

            if (r.Success)
            {
                Guid pongId;
                if (typeof(TPong).InheritsOrImplements(typeof(ApiEvent)))
                {
                    pongId = r.PublishedMessages.Single().Headers.GetMessageId();
                }
                else
                {
                    pongId = r.CommandsSent.Single().Headers.GetMessageId();
                }

                await writeLine($"Waiting for {typeof(TPong).Name} with id {pongId}");
                var tries = 5;
                while (tries > 0)
                {
                    await writeLine("Waiting 1 seconds ...");
                    await Task.Delay(1000);
                    var logged =
                        await new DataStore(appConfig.DatabaseSettings.CreateRepository()).ReadById<MessageLogEntry>(pongId);
                    if (logged != null && logged.ProcessingComplete)
                    {
                        await writeLine($"Received {typeof(TPong).Name} Message Test Succeeded.");
                        await writeLine("+");
                        return;
                    }

                    tries--;
                }

                await writeLine($"Did not receive {typeof(TPong).Name} response!. Message Test Failure.");
                await writeLine("-");
            }
        }
    }
}
