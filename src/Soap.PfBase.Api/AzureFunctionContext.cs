namespace Soap.PfBase.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Threading.Tasks;
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Options;
    using Destructurama;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.SignalRService;
    using Serilog;
    using Serilog.Exceptions;
    using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;
    using Soap.Auth0;
    using Soap.Bus;
    using Soap.Config;
    using Soap.Context;
    using Soap.Context.BlobStorage;
    using Soap.Context.Context;
    using Soap.Context.Exceptions;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.NotificationServer;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public static class AzureFunctionContext
    {
        /* This along with the ServiceBusClient, BlobStorageClient, and NotificationServer
         are the only static variables in the whole pipeline and they are so
         as a best practice for sharing connections amongst azure functions.
         
         This client should be thread-safe and has a spin up time of about
         1 sec so its a trade-off well worth making. Something to remember
         and keep an eye on though. I am not sure if there is a performance
         hit if you have too many clients at once, ie. is there any state
         or locking on the client? */
        
        private static IDocumentRepository lifetimeRepositoryClient;

        public static void CreateLogger(out ILogger logger)
        {
            var loggerConfiguration = new LoggerConfiguration().Enrich.WithExceptionDetails().Destructure.UsingAttributes();
            if (string.IsNullOrEmpty(EnvVars.AppInsightsInstrumentationKey))
            {
                loggerConfiguration.WriteTo.ColoredConsole();
            }
            else
            {
                loggerConfiguration.WriteTo.ApplicationInsights(
                    EnvVars.AppInsightsInstrumentationKey,
                    new TraceTelemetryConverter());
            }

            logger = loggerConfiguration.CreateLogger();

            Log.Logger = logger; //set serilog default instance which is expected by most serilog plugins
        }

        public static async Task<Result> Execute<TUserProfile>(
            string messageAsJson,
            MapMessagesToFunctions mappingRegistration,
            string messageIdAsString,
            string messageTypeShortAssemblyQualifiedName,
            ISecurityInfo securityInfo,
            ILogger logger,
            ApplicationConfig appConfig,
            IAsyncCollector<SignalRMessage> signalRBinding,
            DataStoreOptions dataStoreOptions = null) where TUserProfile : class, IUserProfile, IAggregate, new()

        {
            {
                var x = new Result();

                try
                {
                    ParseMessageId(messageIdAsString, out var messageId);

                    EnsureMessageType(messageTypeShortAssemblyQualifiedName, out var messageType);

                    DeserialiseMessage(messageAsJson, messageType, messageId, out var message);

                    CreateMessageAggregator(out var messageAggregator);

                    CreateDataStore(
                        messageAggregator,
                        appConfig.DatabaseSettings,
                        messageId,
                        dataStoreOptions,
                        out var dataStore);
                    
                    var blobStorage = await CreateBlobStorage(appConfig, messageAggregator);

                    CreateBusContext(
                        messageAggregator,
                        appConfig.BusSettings,
                        blobStorage,
                        signalRBinding,
                        appConfig,
                        out var bus);
                    
                    IdentityPermissions identityPermissions = null;
                    IUserProfile userProfile = null;

                    try
                    {
                        await AuthFunctions.AuthenticateandAuthoriseOrThrow(
                            message,
                            appConfig,
                            dataStore,
                            new Dictionary<string, AuthFunctions.SchemeAuth<TUserProfile>>()
                            {
                                { AuthSchemePrefixes.User, AuthFunctions.UserSchemeAuth<TUserProfile> },
                                { AuthSchemePrefixes.Service, AuthFunctions.ServiceSchemeAuth<TUserProfile> }
                            },
                            securityInfo,
                            v => identityPermissions = v,
                            v => userProfile = v);
                    }
                    catch (Exception exception)
                    {
                        if (!(message is MessageFailedAllRetries)) 
                        {
                            /* sort of HACK, because of the location in the call,
                             we are sending a forced message outside of the unit of work 
                             to any wss sender so that the UI can inform a user. */
                            string errorMessage = "A Security policy violation is preventing this action from succeeding S00";
                            try
                            {
                                errorMessage = new FormattedExceptionInfo(exception, appConfig).SummaryOfExternalErrorMessages;
                            }
                            catch
                            {
                            }

                            var toWsClients = new E001v1_MessageFailed()
                            {
                                E001_ErrorMessage = errorMessage,
                                E001_MessageId = message.Headers.GetMessageId(),
                                E001_MessageTypeName = message.GetType().ToShortAssemblyTypeName(),
                                E001_StatefulProcessId = message.Headers.GetStatefulProcessId()
                            };

                            await bus.Publish(
                                toWsClients,
                                message,
                                new IBusClient.EventVisibilityFlags(IBusClient.EventVisibility.ReplyToWebSocketSender));
                            await messageAggregator.AllMessages.OfType<QueuedEventToPublish>().Single().CommitClosure();
                        }

                        throw new SecurityException("Security Exception", exception);
                    }

                    CreateMessageMeta(message, identityPermissions, userProfile, out var meta);

                    CreateNotificationServer(appConfig.NotificationSettings, messageAggregator, out var notificationServer);
                    
                    var context = new BoostrappedContext(
                        appConfig,
                        dataStore,
                        messageAggregator,
                        logger,
                        bus,
                        notificationServer,
                        blobStorage,
                        mappingRegistration);

                    int retries = appConfig.BusSettings.NumberOfApiMessageRetries;

                    var remainingRuns = retries;
                    var currentRun = 1;
                    var hadProcessIdWhenReceived = message.Headers.GetStatefulProcessId().HasValue;
                    do
                    {
                        if (currentRun > 1) remainingRuns -= 1;
                        try
                        {
                            await MessagePipeline.Execute(message, meta, context);
                            x.Success = true;
                            x.PublishedMessages.AddRange(bus.BusEventsPublished);
                            x.CommandsSent.AddRange(bus.CommandsSent);

                            return x;
                        }
                        catch (Exception e)
                        {
                            
                            if (!hadProcessIdWhenReceived)
                            {
                                /* the SPID could be mutated during processing and any mutations
                                would cause the hash check to fail  */
                                message.Headers.RemoveAll(x => x.Key == Keys.StatefulProcessId);
                            }
                            x.UnhandledError = e;
                        }

                        currentRun++;
                    }
                    while (remainingRuns > 0);
                }
                catch (Exception e)
                {
                    logger.Fatal(e, "Could not start message pipeline for message {@messageJson}", messageAsJson);
                    x.UnhandledError = e;
                }

                return x;
            }

            static void CreateMessageMeta(
                ApiMessage message,
                IdentityPermissions permissions,
                IUserProfile userProfile,
                out MessageMeta meta)
            {
                (DateTime receivedTime, long receivedTicks) timeStamp = (DateTime.UtcNow, StopwatchOps.GetStopwatchTimestamp());

                meta = new MessageMeta(timeStamp, permissions, userProfile, message.Headers.GetMessageId());
            }

            void DeserialiseMessage(string messageJson, Type type, Guid messageId, out ApiMessage message)
            {
                try //* deserialise the message
                {
                    message = messageJson.FromJson<ApiMessage>(SerialiserIds.ApiBusMessage, type.ToShortAssemblyTypeName());

                    Guard.Against(
                        messageId != message.Headers.GetMessageId(),
                        "MessageId parameter does not match MessageId header");
                }
                catch (Exception e)
                {
                    throw new CircuitException("Cannot deserialise message", e);
                }
            }

            static void ParseMessageId(string messageIdAsString, out Guid messageId)
            {
                messageId = Guid.Parse(messageIdAsString);
            }

            static void CreateDataStore(
                IMessageAggregator messageAggregator,
                IDatabaseSettings databaseSettings,
                Guid messageId,
                DataStoreOptions dataStoreOptions,
                out DataStore dataStore)
            {
                //* override anything already there
                dataStoreOptions ??= DataStoreOptions.Create();
                dataStoreOptions.SpecifyUnitOfWorkId(messageId);

                lifetimeRepositoryClient ??= databaseSettings.CreateRepository();

                dataStore = new DataStore(lifetimeRepositoryClient, messageAggregator, dataStoreOptions);
            }

            static void CreateNotificationServer(NotificationServer.Settings settings, IMessageAggregator messageAggregator, out NotificationServer notificationServer)
            {
                notificationServer = settings.CreateServer(messageAggregator);
            }

            static void CreateMessageAggregator(out IMessageAggregator messageAggregator)
            {
                messageAggregator = new MessageAggregator();
            }

            static void CreateBusContext(
                IMessageAggregator messageAggregator,
                IBusSettings busSettings,
                IBlobStorage blobStorage,
                IAsyncCollector<SignalRMessage> signalRBinding,
                ApplicationConfig applicationConfig,
                out IBus busContext)
            {
                busContext = busSettings.CreateBus(
                    messageAggregator,
                    blobStorage,
                    signalRBinding,
                    () => AuthFunctions.GetServiceLevelAuthority(applicationConfig),
                    applicationConfig);
            }

            async Task<BlobStorage> CreateBlobStorage(ApplicationConfig applicationConfig, IMessageAggregator messageAggregator)
            {
                var blobStorage = new BlobStorage(
                    new BlobStorage.Settings(applicationConfig.StorageConnectionString, messageAggregator));
                if (applicationConfig.Environment == SoapEnvironments.Development
                    && applicationConfig.StorageConnectionString.Contains("devstoreaccount1"))
                {
                    await blobStorage.DevStorageSetup();
                }

                return blobStorage;
            }

            static void EnsureMessageType(string typeString, out Type messageType)
            {
                try
                {
                    //* expect assembly qualified name
                    Guard.Against(typeString == null, "'Type' property not provided");
                    var type = Type.GetType(typeString);
                    Guard.Against(
                        type?.ToShortAssemblyTypeName() != typeString,
                        "Message type does not correspond to internal type");
                    messageType = type;
                }
                catch (Exception e)
                {
                    throw new CircuitException("Could not verify message type", e);
                }
            }
        }

        public static void LoadAppConfig(out ApplicationConfig applicationConfig)
        {
            try
            {
                EnsureEnvironmentVars();

                ConfigFunctions.LoadAppConfigFromRemoteRepo(out var applicationConfig1);

                applicationConfig = applicationConfig1;
            }
            catch (Exception e)
            {
                throw new CircuitException("Error retrieving config", e);
            }

            static void EnsureEnvironmentVars()
            {
                //* Also ensure items not directly accessed in code, but required through other means (e.g. function bindings)
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.AzureDevopsOrganisation),
                    $"{nameof(EnvVars.AzureDevopsOrganisation)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.AzureDevopsPat),
                    $"{nameof(EnvVars.AzureDevopsPat)} environment variable not set");
                Guard.Against(string.IsNullOrWhiteSpace(EnvVars.AppId), $"{nameof(EnvVars.AppId)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.SoapEnvironmentKey),
                    $"{nameof(EnvVars.SoapEnvironmentKey)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.AzureWebJobsServiceBus),
                    $"{nameof(EnvVars.AzureWebJobsServiceBus)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.AzureSignalRConnectionString),
                    $"{nameof(EnvVars.AzureSignalRConnectionString)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.CosmosDbDatabaseName),
                    $"{nameof(EnvVars.CosmosDbDatabaseName)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.CosmosDbKey),
                    $"{nameof(EnvVars.CosmosDbKey)} environment variable not set");
                Guard.Against(
                    EnvVars.SoapEnvironmentKey != SoapEnvironments.Development.Key
                    && string.IsNullOrWhiteSpace(EnvVars.CosmosDbEndpointUri),
                    $"{nameof(EnvVars.CosmosDbEndpointUri)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.AzureBusNamespace),
                    $"{nameof(EnvVars.AzureBusNamespace)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.AzureResourceGroup),
                    $"{nameof(EnvVars.AzureResourceGroup)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.CorsOrigin),
                    $"{nameof(EnvVars.CorsOrigin)} environment variable not set");
            }
        }
    }
}
