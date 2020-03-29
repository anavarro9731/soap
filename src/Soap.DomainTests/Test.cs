namespace Soap.DomainTests
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Permissions;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Options;
    using Destructurama;
    using Serilog;
    using Serilog.Exceptions;
    using Soap.Bus;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.Logging;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.Utility.Functions.Extensions;
    using Xunit.Abstractions;

    public static class Test
    {
        public static Task Execute(
            ApiMessage message,
            ITestOutputHelper testOutputHelper,
            IIdentityWithPermissions identity,
            out IDataStore dataStore,
            out IBus bus)
        {
            {
                CreateMessageAggregator(out var messageAggregator);

                CreateLogger(messageAggregator, testOutputHelper, out var logger);

                CreateDataStore(messageAggregator, message, out dataStore);

                CreateBusContext(messageAggregator, out bus);

                CreateAppConfig(out BoostrappedContext.ApplicationConfig appConfig);

                var context = new BoostrappedContext(
                    authenticator: new FakeMessageAuthenticator(identity),
                    messageMapper: new MapMessagesToFunctions(),
                    appConfig: appConfig,
                    logger: logger,
                    bus: bus,
                    dataStore: dataStore,
                    messageAggregator: messageAggregator);

                return MessagePipeline.Execute(message.ToJson(), message.GetType().AssemblyQualifiedName, () => context);
            }

            static void CreateMessageAggregator(out IMessageAggregator messageAggregator)
            {
                messageAggregator = new MessageAggregatorForTesting();
            }

            static void CreateBusContext(IMessageAggregator messageAggregator, out IBus busContext)
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

            static void CreateDataStore(IMessageAggregator messageAggregator, ApiMessage message, out IDataStore dataStore)
            {
                dataStore = new DataStore(
                    new InMemoryDocumentRepository(),
                    messageAggregator,
                    DataStoreOptions.Create().SpecifyUnitOfWorkId(message.MessageId));
            }

            static void CreateAppConfig(out BoostrappedContext.ApplicationConfig applicationConfig)
            {
                applicationConfig = new BoostrappedContext.ApplicationConfig()
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
    }
}