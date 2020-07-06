namespace Soap.DomainTests
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Options;
    using Destructurama;
    using Serilog;
    using Serilog.Exceptions;
    using Soap.Bus;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.Logging;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.NotificationServer;
    using Soap.NotificationServer.Channels;
    using Soap.Utility.Functions.Extensions;
    using Xunit.Abstractions;

    public class DomainTest
    {
        private BoostrappedContext context;

        private Task Execute(
            ApiMessage message,
            MapMessagesToFunctions messageMapper,
            ITestOutputHelper testOutputHelper,
            IApiIdentity identity,
            out DataStore dataStore,
            out NotificationServer notificationServer,
            out InMemoryBus bus)
        {
            {
                if (this.context == null)
                {
                    CreateMessageAggregator(out var messageAggregator);

                    CreateLogger(messageAggregator, testOutputHelper, out var logger);

                    CreateDataStore(messageAggregator, message, out dataStore);

                    CreateNotificationServer(out notificationServer);

                    CreateBusContext(messageAggregator, out bus);

                    CreateAppConfig(out var appConfig);

                    context = new BoostrappedContext(
                        new FakeMessageAuthenticator(identity),
                        messageMapper: messageMapper,
                        appConfig: appConfig,
                        logger: logger,
                        bus: bus,
                        notificationServer: notificationServer,
                        dataStore: dataStore,
                        messageAggregator: messageAggregator);
                }

                dataStore = this.context.DataStore;
                bus = (InMemoryBus)this.context.Bus;
                notificationServer = this.context.NotificationServer;

                return MessagePipeline.Execute(message.ToJson(), message.GetType().AssemblyQualifiedName, () => context);
            }

            static void CreateMessageAggregator(out IMessageAggregator messageAggregator)
            {
                messageAggregator = new MessageAggregatorForTesting();
            }

            static void CreateBusContext(IMessageAggregator messageAggregator, out InMemoryBus busContext)
            {
                busContext = new InMemoryBus(messageAggregator);
            }

            static void CreateLogger(IMessageAggregator messageAggregator, ITestOutputHelper testOutputHelper, out ILogger logger)
            {
                var sink = new SerilogMessageAggregatorSink(messageAggregator);

                var loggerConfiguration = new LoggerConfiguration().Enrich.WithProperty("Environment", "DomainTests")
                                                                   .Enrich.WithExceptionDetails()
                                                                   .Destructure.UsingAttributes()
                                                                   .WriteTo.Sink(sink)
                                                                   .WriteTo.TestOutput(testOutputHelper);

                logger = loggerConfiguration.CreateLogger(); // create serilog ILogger
                Log.Logger = logger; //set serilog default instance which is expected by most serilog plugins
            }

            static void CreateDataStore(IMessageAggregator messageAggregator, ApiMessage message, out DataStore dataStore)
            {
                dataStore = new DataStore(
                    new InMemoryDocumentRepository(),
                    messageAggregator,
                    DataStoreOptions.Create().SpecifyUnitOfWorkId(message.MessageId));
            }

            static void CreateAppConfig(out BoostrappedContext.ApplicationConfig applicationConfig)
            {
                applicationConfig = new BoostrappedContext.ApplicationConfig
                {
                    NumberOfApiMessageRetries = 1,
                    EnvironmentName = "Test",
                    ApplicationName = $"Test Application {Assembly.GetEntryAssembly().GetName().Name}",
                    ReturnExplicitErrorMessages = true,
                    DefaultExceptionMessage = "An Error Has Occurred",
                    ApplicationVersion = "0.0.0"
                };
            }
        }

        private void CreateNotificationServer(out NotificationServer notificationServer)
        {
            var settings = new NotificationServer.Settings()
            {
                ChannelSettings = new List<INotificationChannelSettings>()
                {
                    {
                        new InMemoryChannel.Settings()
                    }
                }
            };

            notificationServer = settings.CreateServer();
        }

        public Func<ApiMessage, IApiIdentity, Task<Result>> WireExecute(
            MapMessagesToFunctions mapper,
            ITestOutputHelper outputHelper)
        {
            return async (message, identity) =>
                {
                    var x = new Result();
                    await Execute(message, mapper, outputHelper, identity, out x.DataStore, out x.NotificationServer, out x.MessageBus);
                    return x;
                };
        }

        public class Result
        {
            public DataStore DataStore;

            public NotificationServer NotificationServer;

            public InMemoryBus MessageBus;
        }
    }
}