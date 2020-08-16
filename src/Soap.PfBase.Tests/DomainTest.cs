namespace Soap.PfBase.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces;
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
    using Soap.Utility.Functions.Extensions;
    using Xunit.Abstractions;

    public class DomainTest
    {
        internal IBus rollingBus;

        //* copies over each call of Add and Execute retaining state across the whole test
        internal DataStore rollingStore;

        private BoostrappedContext context;

        public TAggregate GetAdd<TAggregate>(TAggregate aggregate, ITestOutputHelper outputHelper)
            where TAggregate : Aggregate, new()
        {
            CreateDataStore(
                new MessageAggregatorForTesting(),
                new TestConfig().DatabaseSettings,
                Guid.NewGuid(),
                out var dataStore);

            var result = dataStore.Create(aggregate).Result;
            dataStore.CommitChanges();
            return result;
        }

        public Func<ApiMessage, IApiIdentity, Task<Result>> GetExecute(
            MapMessagesToFunctions mapper,
            ITestOutputHelper outputHelper,
            byte retries)
        {
            CreateAppConfig(retries, out var applicationConfig);

            return async (message, identity) =>
                {
                message.Headers.EnsureRequiredHeaders();
                var x = new Result();
                await Execute(message, applicationConfig, mapper, outputHelper, identity, x);
                return x;
                };

            static void CreateAppConfig(byte retries, out TestConfig applicationConfig)
            {
                applicationConfig = new TestConfig
                {
                    //* i assume this wierd syntax sets only the numberofmessageretries property and does not recreate the bussettings property
                    BusSettings =
                    {
                        NumberOfApiMessageRetries = retries
                    }
                };

            }
        }

        private void CreateDataStore(
            IMessageAggregator messageAggregator,
            IDatabaseSettings settings,
            Guid unitOfWorkId,
            out DataStore dataStore)
        {
            dataStore = new DataStore(
                this.rollingStore?.DocumentRepository ?? settings.CreateRepository(),
                messageAggregator,
                DataStoreOptions.Create().SpecifyUnitOfWorkId(unitOfWorkId));
            this.rollingStore = dataStore;
        }

        private async Task Execute(
            ApiMessage message,
            TestConfig appConfig,
            MapMessagesToFunctions messageMapper,
            ITestOutputHelper testOutputHelper,
            IApiIdentity identity,
            Result result)
        {
            {
                CreateMessageAggregator(out var messageAggregator);

                CreateLogger(messageAggregator, testOutputHelper, out var logger);

                CreateDataStore(messageAggregator, appConfig.DatabaseSettings, message.Headers.GetMessageId(), out var dataStore);

                CreateNotificationServer(appConfig.NotificationServerSettings, out var notificationServer);

                CreateBusContext(messageAggregator, appConfig.BusSettings, out var bus);

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

                /*  What we are primarily trying to achieve is to make sure that each execute set the activeprocesstate,
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

            static void CreateNotificationServer(NotificationServer.Settings settings, out NotificationServer notificationServer)
            {
                notificationServer = settings.CreateServer();
            }

            static void CreateMessageAggregator(out IMessageAggregator messageAggregator)
            {
                messageAggregator = new MessageAggregatorForTesting();
            }

            void CreateBusContext(
                IMessageAggregator messageAggregator,
                IBusSettings appConfigBusSettings,
                out IBus busContext)
            {
                this.rollingBus ??= appConfigBusSettings.CreateBus(messageAggregator);

                busContext = this.rollingBus;
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