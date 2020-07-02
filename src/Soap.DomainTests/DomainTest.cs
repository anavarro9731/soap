namespace Soap.DomainTests
{
    using System;
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
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.Logging;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.Utility.Functions.Extensions;
    using Xunit.Abstractions;



    public static class DomainTest
    {
        private static Task Execute(
            ApiMessage message,
            MapMessagesToFunctions messageMapper,
            ITestOutputHelper testOutputHelper,
            IApiIdentity identity,
            out DataStore dataStore,
            out IBus bus)
        {
            {
                CreateMessageAggregator(out var messageAggregator);

                CreateLogger(messageAggregator, testOutputHelper, out var logger);

                CreateDataStore(messageAggregator, message, out dataStore);

                CreateBusContext(messageAggregator, out bus);

                CreateAppConfig(out var appConfig);

                var context = new BoostrappedContext(
                    new FakeMessageAuthenticator(identity),
                    messageMapper: messageMapper,
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

        public static Func<ApiMessage, IApiIdentity, Task<Result>> WireExecute(
            MapMessagesToFunctions mapper,
            ITestOutputHelper outputHelper)
        {
            return async (message, identity) =>
                {
                var x = new Result();
                await Execute(message, mapper, outputHelper, identity, out x.DataStore, out x.MessageBus);
                return x;
                };
        }

        public class Result
        {
            public DataStore DataStore;

            public IBus MessageBus;
        }
    }
}