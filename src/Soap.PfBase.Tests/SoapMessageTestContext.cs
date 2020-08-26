namespace Soap.PfBase.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces;
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

    public class SoapMessageTestContext
    {
        public async Task<Result> Execute(
            ApiMessage message,
            MapMessagesToFunctions messageMapper,
            ITestOutputHelper output,
            IApiIdentity identity,
            byte retries,
            IDocumentRepository rollingRepo,
            Func<DataStore, int, Task> beforeRunHook = null,
            Guid? runHookUnitOfWorkId = null)

        {
            {
                var x = new Result();

                message.Headers.EnsureRequiredHeaders();

                CreateAppConfig(retries, out var appConfig);

                CreateMessageAggregator(out var messageAggregator);

                CreateLogger(messageAggregator, output, out var logger);

                CreateDataStore(messageAggregator, rollingRepo, message.Headers.GetMessageId(), out var dataStore);

                CreateNotificationServer(appConfig.NotificationServerSettings, out var notificationServer);

                CreateBusContext(messageAggregator, appConfig.BusSettings, out var bus);

                var context = new BoostrappedContext(
                    new FakeMessageAuthenticator(identity),
                    messageMapper: messageMapper,
                    appConfig: appConfig,
                    logger: logger,
                    bus: bus,
                    notificationServer: notificationServer,
                    dataStore: dataStore,
                    messageAggregator: messageAggregator);

                byte currentRun = 1;
                var remainingRuns = retries;

                logger.Information("OUTPUT LOG...");

                do
                {
                    logger.Information(
                        $@"\/\/\/\/\/\/\/\/\/\/\/\/ RUN {currentRun} STARTED {remainingRuns} run(s) left /\/\/\/\/\/\/\/\/\/\/\/\\/"
                        + Environment.NewLine);

                    if (beforeRunHook != null)
                    {
                        try
                        {
                            logger.Information(
                                @"---------------------- EXECUTING BEFORE RUN HOOK ----------------------" + Environment.NewLine);

                            await beforeRunHook.Invoke(
                                new DataStore(
                                    context.DataStore.DocumentRepository,
                                    dataStoreOptions: runHookUnitOfWorkId.HasValue
                                                          ? DataStoreOptions.Create()
                                                                            .SpecifyUnitOfWorkId(runHookUnitOfWorkId.Value)
                                                          : null),
                                currentRun);
                        }
                        catch (Exception e)
                        {
                            logger.Information(Environment.NewLine + e + Environment.NewLine);

                            logger.Information(
                                Environment.NewLine
                                + $@"\/\/\/\/\/\/\/\/\/\/\/\/  RUN {currentRun} ENDED in FAILURE, {remainingRuns - 1} run(s) left /\/\/\/\/\/\/\/\/\/\/\/\\/");
                            x.UnhandledError = e;
                            return x;
                        }
                    }

                    logger.Information(
                        @"---------------------- EXECUTING MESSAGE HANDLER ----------------------" + Environment.NewLine);

                    try
                    {
                        await MessagePipeline.Execute(message, context);

                        x.Success = true;
                        /*  What we are primarily trying to achieve is to make sure that each execute sets the activeprocesstate,
                        if there is a statefulprocess handling the message, to the statefulprocess that it affects. 
                        Not being able to see directly into the pipeline makes this challenging,
                        and just looking at the list of processstates in play will not help as that will include items from previous
                        executes. what we will do is look at the debug messages created in the bus for THIS message and find if there 
                        are any statefulprocessstarted or continued events we can use to determine the id of the processtate to return. */

                        var statefulProcessLaunchedByThisMessage = messageAggregator.AllMessages
                                                                                    .Where(
                                                                                        m => m.GetType()
                                                                                            .InheritsOrImplements(
                                                                                                typeof(
                                                                                                    IAssociateProcessStateWithAMessage
                                                                                                )))
                                                                                    .SingleOrDefault(
                                                                                        m =>
                                                                                            ((IAssociateProcessStateWithAMessage)m
                                                                                            ).ByMessage == message.Headers
                                                                                                .GetMessageId())
                                                                                    .As<IAssociateProcessStateWithAMessage>();

                        if (statefulProcessLaunchedByThisMessage != null)
                        {
                            var processStateId = statefulProcessLaunchedByThisMessage.ProcessStateId;
                            var activeProcessState = await dataStore.ReadById<ProcessState>(processStateId);
                            x.ActiveProcessState = activeProcessState;
                        }

                        x.MessageBus = bus;
                        x.DataStore = dataStore;
                        x.NotificationServer = notificationServer;


                        logger.Information(
                            Environment.NewLine
                            + $@"\/\/\/\/\/\/\/\/\/\/\/\/  RUN {currentRun} ENDED in SUCCESS, {remainingRuns - 1} run(s) left /\/\/\/\/\/\/\/\/\/\/\/\\/");
                        return x;
                    }
                    catch (Exception e)
                    {
                        x.UnhandledError = e;
                    }

                    if (currentRun++ > 1) remainingRuns -= 1;
                }
                while (remainingRuns > 0);

                return x;
            }

            static void CreateAppConfig(byte retries, out TestConfig applicationConfig)
            {
                applicationConfig = new TestConfig
                {
                    /* i assume this wierd syntax sets only the numberofmessageretries
                     property and does not recreate the bussettings property */
                    BusSettings =
                    {
                        NumberOfApiMessageRetries = retries
                    }
                };
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

            static void CreateMessageAggregator(out IMessageAggregator messageAggregator)
            {
                messageAggregator = new MessageAggregatorForTesting();
            }

            static void CreateNotificationServer(NotificationServer.Settings settings, out NotificationServer notificationServer)
            {
                notificationServer = settings.CreateServer();
            }

            static void CreateBusContext(
                IMessageAggregator messageAggregator,
                IBusSettings appConfigBusSettings,
                out IBus busContext)
            {
                busContext = appConfigBusSettings.CreateBus(messageAggregator);
            }

            static void CreateDataStore(
                IMessageAggregator messageAggregator,
                IDocumentRepository rollingRepo,
                Guid unitOfWorkId,
                out DataStore dataStore)
            {
                dataStore = new DataStore(
                    rollingRepo,
                    messageAggregator,
                    DataStoreOptions.Create().SpecifyUnitOfWorkId(unitOfWorkId));
            }
        }
    }
}