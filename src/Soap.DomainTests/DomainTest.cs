namespace Soap.DomainTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces.LowLevel;
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
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.MessagePipeline.ProcessesAndOperations.ProcessMessages;
    using Soap.NotificationServer;
    using Soap.NotificationServer.Channels;
    using Soap.Utility.Functions.Extensions;
    using Xunit.Abstractions;

    public class DomainTest
    {
        private BoostrappedContext context;

        //* copies over each call of Add and Execute retaining state across the whole test
        internal DataStore rollingStore;

        public TAggregate GetAdd<TAggregate>(
            TAggregate aggregate,
            ITestOutputHelper outputHelper) where TAggregate : Aggregate, new()
        {
            CreateDataStore(new MessageAggregatorForTesting(), Guid.NewGuid(),  out var dataStore);
            
            var result = dataStore.Create(aggregate).Result;
            dataStore.CommitChanges();
            return result;

        }

        public Func<ApiMessage, IApiIdentity, Task<Result>> GetExecute(
            MapMessagesToFunctions mapper,
            ITestOutputHelper outputHelper, int retries)
        {
            return async (message, identity) =>
                {
                message.Headers.EnsureRequiredHeaders();
                var x = new Result();
                await Execute(message, mapper, outputHelper, identity, x, retries);
                return x;
                };
        }

        private void CreateDataStore(IMessageAggregator messageAggregator, Guid unitOfWorkId, out DataStore dataStore)
        {
            dataStore = new DataStore(
                this.rollingStore?.DocumentRepository ?? new InMemoryDocumentRepository(),
                messageAggregator,
                DataStoreOptions.Create().SpecifyUnitOfWorkId(unitOfWorkId));
            this.rollingStore = dataStore;
        }

        private async Task Execute(
            ApiMessage message,
            MapMessagesToFunctions messageMapper,
            ITestOutputHelper testOutputHelper,
            IApiIdentity identity,
            Result result,
            int retries)
        {
            {
                CreateMessageAggregator(out var messageAggregator);

                CreateLogger(messageAggregator, testOutputHelper, out var logger);

                CreateDataStore(messageAggregator, message.Headers.GetMessageId(), out var dataStore);

                CreateNotificationServer(out var notificationServer);

                CreateBusContext(messageAggregator, out var bus);

                CreateAppConfig(retries, out var appConfig);

                this.context = new BoostrappedContext(
                    new FakeMessageAuthenticator(identity),
                    messageMapper: messageMapper,
                    appConfig: appConfig,
                    logger: logger,
                    bus: bus,
                    notificationServer: notificationServer,
                    dataStore: dataStore,
                    messageAggregator: messageAggregator);

                await MessagePipeline.Execute(
                    message.ToNewtonsoftJson(),
                    message.GetType().AssemblyQualifiedName,
                    () => this.context);

                /*  what we are primarily trying to achieve is to make sure that each execute set the activeprocesstate,
                 if there is a statefulprocess handling the message, to the statefulprocess that it affects. 
                Not being able to see directly into the pipeline makes this challenging,
                and just looking at the list of processstates in play will not help as that will include items from previous
                executes. what we will do is look at the debug messages created in the bus for THIS message and find if there 
                are any statefulprocessstarted or continued events we can use to determine the id of the processtate to return. */

                var statefulProcessLaunchedByThisMessage = messageAggregator
                                                           .AllMessages
                                                           .Where(
                                                               m => m.GetType()
                                                                     .InheritsOrImplements(
                                                                         typeof(IAssociateProcessStateWithAMessage)))
                                                           .SingleOrDefault(
                                                               m => ((IAssociateProcessStateWithAMessage)m).ByMessage
                                                                    == message.Headers.GetMessageId())
                                                           .As<IAssociateProcessStateWithAMessage>();

                if (statefulProcessLaunchedByThisMessage != null)
                {
                    var processStateId = statefulProcessLaunchedByThisMessage.ProcessStateId;
                    var activeProcessState = await dataStore.ReadById<ProcessState>(processStateId);
                    result.ActiveProcessState = activeProcessState;
                }

                result.MessageBus = bus;
                result.DataStore = dataStore;
                result.NotificationServer = notificationServer;
            }

            static void CreateNotificationServer(out NotificationServer notificationServer)
            {
                var settings = new NotificationServer.Settings
                {
                    ChannelSettings = new List<INotificationChannelSettings>
                    {
                        new InMemoryChannel.Settings()
                    }
                };

                notificationServer = settings.CreateServer();
            }

            static void CreateMessageAggregator(out IMessageAggregator messageAggregator)
            {
                messageAggregator = new MessageAggregatorForTesting();
            }

            static void CreateBusContext(IMessageAggregator messageAggregator, out IBus busContext)
            {
                busContext = new Bus(new InMemoryBus(messageAggregator), messageAggregator);
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

            static void CreateAppConfig(int retries, out BoostrappedContext.ApplicationConfig applicationConfig)
            {
                applicationConfig = new BoostrappedContext.ApplicationConfig
                {
                    NumberOfApiMessageRetries = retries,
                    EnvironmentName = "Test",
                    ApplicationName = $"Test Application {Assembly.GetEntryAssembly().GetName().Name}",
                    ReturnExplicitErrorMessages = true,
                    DefaultExceptionMessage = "An Error Has Occurred",
                    ApplicationVersion = "0.0.0"
                };
            }
        }

        public class Result
        {
            public ProcessState ActiveProcessState;

            public DataStore DataStore;

            public IBus MessageBus;

            public NotificationServer NotificationServer;
        }
    }
}