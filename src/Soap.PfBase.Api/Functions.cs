namespace Soap.PfBase.Api
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Options;
    using DataStore.Providers.CosmosDb;
    using Destructurama;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;
    using Microsoft.CSharp.RuntimeBinder;
    using Serilog;
    using Serilog.Exceptions;
    using Soap.Api.Sample.Afs;
    using Soap.Auth0;
    using Soap.Bus;
    using Soap.Interfaces;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.NotificationServer;
    using Soap.PfBase.Alj;
    using Soap.Utility.Functions.Operations;

    public partial class Functions
    {
        public static async Task Execute<TUser>(
            string message,
            MapMessagesToFunctions mappingRegistration,
            string messageIdAsString,
            DataStoreOptions dataStoreOptions = null) where TUser : class, IApiIdentity, new()
        {
            {
                EnsureEnvironmentVars();
                
                ParseMessageId(messageIdAsString, out var messageId);

                CreateMessageAggregator(out var messageAggregator);

                ConfigFunctions.LoadFromRemoteRepo(new AppEnvIdentifier("SAP", SoapEnvironments.Development),  out var appConfig);

                CreateLogger(appConfig.LogSettings, out var logger);

                //TODO: resuse datastore repository on each call if its slow 
                CreateDataStore(messageAggregator, appConfig.DatabaseSettings, messageId, dataStoreOptions, out var dataStore);

                CreateNotificationServer(appConfig.NotificationSettings, out var notificationServer);

                CreateBusContext(messageAggregator, appConfig.BusSettings, out var bus);

                var context = new BoostrappedContext(
                    new Auth0Authenticator(() => new TUser()),
                    messageMapper: mappingRegistration,
                    appConfig: appConfig,
                    logger: logger,
                    bus: bus,
                    notificationServer: notificationServer,
                    dataStore: dataStore,
                    messageAggregator: messageAggregator);

                int retries = appConfig.BusSettings.NumberOfApiMessageRetries;
                int runs = retries + 1;
                try
                {
                    while (runs > 0)
                    {
                        await MessagePipeline.Execute(message, message.GetType().AssemblyQualifiedName, () => context);
                    }
                }    
                finally
                {
                    retries--;
                }
                
            }

            static void EnsureEnvironmentVars()
            {


                Guard.Against(string.IsNullOrWhiteSpace(ConfigId.AzureDevopsOrganisation), $"{nameof(ConfigId.AzureDevopsOrganisation)} environment variable not set");
                Guard.Against(string.IsNullOrWhiteSpace(ConfigId.AzureDevopsPat), $"{nameof(ConfigId.AzureDevopsPat)} environment variable not set");
                Guard.Against(string.IsNullOrWhiteSpace(ConfigId.SoapApplicationKey), $"{nameof(ConfigId.SoapApplicationKey)} environment variable not set");
                Guard.Against(string.IsNullOrWhiteSpace(ConfigId.SoapEnvironmentKey), $"{nameof(ConfigId.SoapEnvironmentKey)} environment variable not set");
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

                dataStore = new DataStore(databaseSettings.CreateRepository(), messageAggregator, dataStoreOptions);
            }

            static void CreateNotificationServer(NotificationServer.Settings settings, out NotificationServer notificationServer)
            {
                notificationServer = settings.CreateServer();
            }

            static void CreateMessageAggregator(out IMessageAggregator messageAggregator)
            {
                messageAggregator = new MessageAggregatorForTesting();
            }

            void CreateBusContext(IMessageAggregator messageAggregator, IBusSettings busSettings, out IBus busContext)
            {
                busContext = busSettings.CreateBus(messageAggregator);
            }

            static void CreateLogger(SeqServerConfig seqServerConfig, out ILogger logger)
            {
                var loggerConfiguration = new LoggerConfiguration().Enrich.WithProperty("Environment", "DomainTests")
                                                                   .Enrich.WithExceptionDetails()
                                                                   .Destructure.UsingAttributes()
                                                                   .WriteTo.Seq(
                                                                       seqServerConfig.ServerUrl,
                                                                       apiKey: seqServerConfig.ApiKey);

                logger = loggerConfiguration.CreateLogger(); // create serilog ILogger
                Log.Logger = logger; //set serilog default instance which is expected by most serilog plugins
            }

        }
    }
}