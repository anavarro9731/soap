﻿namespace Soap.PfBase.Api
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Options;
    using Destructurama;
    using Microsoft.ApplicationInsights.Extensibility;
    using Newtonsoft.Json;
    using Serilog;
    using Serilog.Events;
    using Serilog.Exceptions;
    using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;
    using Soap.Auth0;
    using Soap.Bus;
    using Soap.Config;
    using Soap.Context.Context;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.NotificationServer;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public static class AzureFunctionContext
    {
        /* THIS IS THE ONLY STATIC VARIABLE IN THE WHOLE PRODUCTION PIPELINE
         the client should be thread-safe and has a spin up time of about
         1 sec so its a trade-off well work making. Something to remember
         and keep an eye on though. I am not sure if there is a performance
         hit if you have too many clients at once, ie. is there any state
         or locking on the client? */
        private static IDocumentRepository lifetimeRepositoryClient;

        public static async Task<Result> Execute<TApiIdentity>(
            string messageAsJson,
            MapMessagesToFunctions mappingRegistration,
            string messageIdAsString,
            IDictionary<string, object> userProperties,
            ILogger logger,
            ApplicationConfig appConfig,
            DataStoreOptions dataStoreOptions = null) where TApiIdentity : class, IApiIdentity, new()
        {
            {
                var x = new Result();

                try
                {
                    ParseMessageId(messageIdAsString, out var messageId);

                    EnsureMessageType(userProperties, out var messageType);

                    DeserialiseMessage(messageAsJson, messageType, messageId, out var message);

                    CreateMessageAggregator(out var messageAggregator);

                    CreateDataStore(
                        messageAggregator,
                        appConfig.DatabaseSettings,
                        messageId,
                        dataStoreOptions,
                        out var dataStore);

                    CreateNotificationServer(appConfig.NotificationSettings, out var notificationServer);

                    CreateBusContext(messageAggregator, appConfig.BusSettings, out var bus);

                    var context = new BoostrappedContext(
                        new Auth0Authenticator(() => new TApiIdentity()),
                        messageMapper: mappingRegistration,
                        appConfig: appConfig,
                        logger: logger,
                        bus: bus,
                        notificationServer: notificationServer,
                        dataStore: dataStore,
                        messageAggregator: messageAggregator);

                    int retries = appConfig.BusSettings.NumberOfApiMessageRetries;

                    var remainingRuns = retries;
                    var currentRun = 1;
                    do
                    {
                        try
                        {
                            await MessagePipeline.Execute(message, context);
                            x.Success = true;
                            x.PublishMessages = bus.EventsPublished;
                            x.CommandsSent = bus.CommandsSent;
                            return x;
                        }
                        catch (Exception e)
                        {
                            x.UnhandledError = e;
                        }

                        if (currentRun++ > 1) remainingRuns -= 1;
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

            void DeserialiseMessage(string messageJson, Type type, Guid messageId, out ApiMessage message)
            {
                try //* deserialise the message
                {
                    message = JsonConvert.DeserializeObject(messageJson, type).As<ApiMessage>();

                    Guard.Against(
                        messageId != message.Headers.GetMessageId(),
                        "MessageId parameter does not match MessageId header");
                }
                catch (Exception e)
                {
                    throw new Exception("Cannot deserialise message", e);
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

            static void CreateNotificationServer(NotificationServer.Settings settings, out NotificationServer notificationServer)
            {
                notificationServer = settings.CreateServer();
            }

            static void CreateMessageAggregator(out IMessageAggregator messageAggregator)
            {
                messageAggregator = new MessageAggregator();
            }

            void CreateBusContext(IMessageAggregator messageAggregator, IBusSettings busSettings, out IBus busContext)
            {
                busContext = busSettings.CreateBus(messageAggregator);
            }

            static void EnsureMessageType(IDictionary<string, object> userProperties, out Type messageType)
            {
                try
                {
                    var typeString = userProperties["Type"] as string;
                    Guard.Against(typeString == null, "'Type' property not provided");
                    var type = Type.GetType(typeString);
                    Guard.Against(type?.AssemblyQualifiedName != typeString, "Message type does not correspond to internal type");
                    messageType = type;
                }
                catch (Exception e)
                {
                    throw new Exception("Could not verify message type", e);
                }
            }
        }

        public static void CreateLogger(out ILogger logger)
        {
            var loggerConfiguration = new LoggerConfiguration()
                     .Enrich.WithExceptionDetails()
                     .Destructure.UsingAttributes();
            if (EnvVars.AppInsightsInstrumentationKey == null)
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
                throw new Exception("Error retrieving config", e);
            }

            static void EnsureEnvironmentVars()
            {
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.AzureDevopsOrganisation),
                    $"{nameof(EnvVars.AzureDevopsOrganisation)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.AzureDevopsPat),
                    $"{nameof(EnvVars.AzureDevopsPat)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.AppId),
                    $"{nameof(EnvVars.AppId)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.SoapEnvironmentKey),
                    $"{nameof(EnvVars.SoapEnvironmentKey)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.AzureWebJobsServiceBus),
                    $"{nameof(EnvVars.AzureWebJobsServiceBus)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.CosmosDbDatabaseName),
                    $"{nameof(EnvVars.CosmosDbDatabaseName)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.CosmosDbKey),
                    $"{nameof(EnvVars.CosmosDbKey)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.CosmosDbAccountName),
                    $"{nameof(EnvVars.CosmosDbAccountName)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.AzureBusNamespace),
                    $"{nameof(EnvVars.AzureBusNamespace)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.AzureResourceGroup),
                    $"{nameof(EnvVars.AzureResourceGroup)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.ServicePrincipal.ClientId),
                    $"{nameof(EnvVars.ServicePrincipal.ClientId)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.ServicePrincipal.ClientSecret),
                    $"{nameof(EnvVars.ServicePrincipal.ClientSecret)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.ServicePrincipal.TenantId),
                    $"{nameof(EnvVars.ServicePrincipal.TenantId)} environment variable not set");
                Guard.Against(
                    string.IsNullOrWhiteSpace(EnvVars.AzureStorageConnectionString),
                    $"{nameof(EnvVars.AzureStorageConnectionString)} environment variable not set");
            }


        }
    }
}