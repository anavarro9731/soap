namespace Soap.PfBase.Api
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Options;
    using Destructurama;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.SignalRService;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using Serilog;
    using Serilog.Exceptions;
    using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;
    using Soap.Auth0;
    using Soap.Bus;
    using Soap.Config;
    using Soap.Context;
    using Soap.Context.BlobStorage;
    using Soap.Context.Context;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.NotificationServer;
    using Soap.Utility.Functions.Extensions;

    public static class AzureFunctionContext
    {
        /* THIS IS THE ONLY STATIC VARIABLE IN THE WHOLE PRODUCTION PIPELINE
         the client should be thread-safe and has a spin up time of about
         1 sec so its a trade-off well work making. Something to remember
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

        public static async Task<Result> Execute<TApiIdentity>(
            string messageAsJson,
            MapMessagesToFunctions mappingRegistration,
            string messageIdAsString,
            string messageTypeShortAssemblyQualifiedName,
            ILogger logger,
            ApplicationConfig appConfig,
            IAsyncCollector<SignalRMessage> signalRBinding,
            DataStoreOptions dataStoreOptions = null) where TApiIdentity : class, IApiIdentity, new()

        {
            {
                var x = new Result();

                try
                {
                    ParseMessageId(messageIdAsString, out var messageId);

                    EnsureMessageType(messageTypeShortAssemblyQualifiedName, out var messageType);

                    DeserialiseMessage(messageAsJson, messageType, messageId, out var message);

                    await AuthoriseCall(appConfig, message.Headers.GetAccessToken());
                    
                    CreateMessageAggregator(out var messageAggregator);

                    CreateDataStore(
                        messageAggregator,
                        appConfig.DatabaseSettings,
                        messageId,
                        dataStoreOptions,
                        out var dataStore);

                    CreateNotificationServer(appConfig.NotificationSettings, out var notificationServer);

                    var blobStorage = await CreateBlobStorage(appConfig, messageAggregator);

                    CreateBusContext(messageAggregator, appConfig.BusSettings, blobStorage, signalRBinding, out var bus);

                    var context = new BoostrappedContext(
                        new Auth0Authenticator<TApiIdentity>(message.Headers.GetIdentityToken(), message.Headers.GetAccessToken()),
                        messageMapper: mappingRegistration,
                        appConfig: appConfig,
                        logger: logger,
                        bus: bus,
                        notificationServer: notificationServer,
                        dataStore: dataStore,
                        messageAggregator: messageAggregator,
                        blobStorage: blobStorage);

                    int retries = appConfig.BusSettings.NumberOfApiMessageRetries;

                    var remainingRuns = retries;
                    var currentRun = 1;
                    do
                    {
                        if (currentRun > 1) remainingRuns -= 1;
                        try
                        {
                            await MessagePipeline.Execute(message, context);
                            x.Success = true;
                            x.PublishedMessages.AddRange(bus.BusEventsPublished);
                            x.CommandsSent.AddRange(bus.CommandsSent);

                            return x;
                        }
                        catch (Exception e)
                        {
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

            static async Task AuthoriseCall(ApplicationConfig applicationConfig, string bearerToken)
            {
                var openIdConfig = await GetOpenIdConfig($"{applicationConfig.Auth0TenantDomain}");

                var validationParameters = new TokenValidationParameters
                {
                    RequireSignedTokens = true,
                    ValidAudience = EnvVars.FunctionAppHostUrlWithTrailingSlash,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    IssuerSigningKeys = openIdConfig.SigningKeys,
                    ValidIssuer = openIdConfig.Issuer
                };

                var handler = new JwtSecurityTokenHandler();
                try
                {
                    //* will validate formation and signature by default
                    var principal = handler.ValidateToken(bearerToken, validationParameters, out var validatedToken);

                    //* find the scope claims, get its contents and check you have the scope for this message
                    //Guard.Against(principal.Claims.Single(x => x.), "This access token is not valid for this message"); //TODO filter
                    
                }
                catch (SecurityTokenExpiredException ex)
                {
                    throw new ApplicationException("The access token is expired.", ex);
                }

                // Get the public keys from the jwks endpoint      
                async Task<OpenIdConnectConfiguration> GetOpenIdConfig(string tenantDomain)
                {
                    var openIdConfigurationEndpoint = $"{tenantDomain}.well-known/openid-configuration";
                    var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                        openIdConfigurationEndpoint,
                        new OpenIdConnectConfigurationRetriever());
                    var openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);
                    return openIdConfig;
                }
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
                    throw new ApplicationException("Cannot deserialise message", e);
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

            static void CreateBusContext(
                IMessageAggregator messageAggregator,
                IBusSettings busSettings,
                IBlobStorage blobStorage,
                IAsyncCollector<SignalRMessage> signalRBinding,
                out IBus busContext)
            {
                busContext = busSettings.CreateBus(messageAggregator, blobStorage, signalRBinding);
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
                    throw new ApplicationException("Could not verify message type", e);
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
                throw new ApplicationException("Error retrieving config", e);
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
