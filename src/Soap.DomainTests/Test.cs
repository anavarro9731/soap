namespace Soap.DomainTests
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Options;
    using Destructurama;
    using Serilog;
    using Serilog.Exceptions;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.Logging;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.Utility.Functions.Extensions;
    using Xunit.Abstractions;

    public static class Test
    {
        public static Task Execute(ApiMessage message, ITestOutputHelper testOutputHelper, out IDataStore dataStore, out IBusContext busContext)
        {
            {
                CreateMessageAggregator(out var messageAggregator);

                CreateLogger(messageAggregator, testOutputHelper, out var logger);

                CreateDataStore(messageAggregator, message, out dataStore);

                CreateAppConfig(messageAggregator, out var appConfig);

                CreateBusContext(messageAggregator, out busContext);

                var context = new BootrappedContext(
                    authenticator: null, //TODO
                    messageMapper: null, //TODO
                    appConfig: appConfig,
                    logger: logger,
                    busContext: busContext,
                    dataStore: dataStore,
                    messageAggregator: messageAggregator);

                return MessagePipeline.Execute(message.ToJson(), message.GetType().AssemblyQualifiedName, () => context);
            }

            static void CreateMessageAggregator(out IMessageAggregator messageAggregator)
            {
                messageAggregator = new MessageAggregatorForTesting();
            }

            static void CreateBusContext(IMessageAggregator messageAggregator, out IBusContext busContext)
            {
                busContext = new InMemoryBusContext(messageAggregator);
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

            static void CreateAppConfig(IMessageAggregator messageAggregator, out ApplicationConfig applicationConfig)
            {
                throw new NotImplementedException();
            }
        }
    }
}