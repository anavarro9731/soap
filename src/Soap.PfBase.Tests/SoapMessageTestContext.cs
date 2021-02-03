namespace Soap.PfBase.Tests
{
    using System;
    using System.Collections.Generic;
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
    using Soap.Api.Sample.Tests;
    using Soap.Bus;
    using Soap.Config;
    using Soap.Context;
    using Soap.Context.BlobStorage;
    using Soap.Context.Context;
    using Soap.Context.Logging;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.NotificationServer;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;
    using Xunit.Abstractions;

    public class SoapMessageTestContext
    {

        public static List<TestIdentity> TestIdentities;
        
        public async Task<Result> Execute(
            ApiMessage message,
            MapMessagesToFunctions messageMapper,
            ITestOutputHelper output,
            ISecurityInfo securityInfo,
            byte retries,
            bool authEnabled,
            bool enableSlaWhenSecurityContextIsMissing,
            IDocumentRepository rollingRepo,
            (Func<DataStore, int, Task> Function, Guid? RunHookUnitOfWorkId) beforeRunHook,
            DataStoreOptions dataStoreOptions,
            Action<MessageAggregatorForTesting> setup)

        {
            {
                var x = new Result();

                CreateAppConfig(retries, authEnabled, enableSlaWhenSecurityContextIsMissing, out var appConfig);

                CreateMessageAggregator(setup, out var messageAggregator);

                CreateLogger(messageAggregator, output, out var logger);

                try
                {
                    CreateDataStore(
                        messageAggregator,
                        rollingRepo,
                        dataStoreOptions,
                        message.Headers.GetMessageId(),
                        out var dataStore);

                    IUserProfile userProfile = null;
                    IdentityPermissions identityPermissions = null;
                    await AuthFunctions.AuthenticateandAuthoriseOrThrow<TestProfile>(message, appConfig, dataStore, new Dictionary<string, AuthFunctions.SchemeAuth<TestProfile>>()
                    {
                        { AuthSchemePrefixes.Tests, TestSchemeAuth<TestProfile> }
                    } ,securityInfo, v => identityPermissions = v, v => userProfile = v);
                    
                    CreateMessageMeta(message, identityPermissions, userProfile, out var meta);
                        
                    CreateNotificationServer(appConfig.NotificationServerSettings, out var notificationServer);

                    CreateBlobStorage(messageAggregator, out var blobStorage);

                    CreateBusContext(messageAggregator, appConfig, blobStorage, out var bus);

                    var context = new BoostrappedContext(
                        messageMapper: messageMapper,
                        appConfig: appConfig,
                        logger: logger,
                        bus: bus,
                        notificationServer: notificationServer,
                        dataStore: dataStore,
                        messageAggregator: messageAggregator,
                        blobStorage: blobStorage);

                    byte currentRun = 1;
                    var remainingRuns = retries;

                    logger.Information("OUTPUT LOG...");

                    do
                    {
                        if (currentRun > 1) remainingRuns -= 1;
                        logger.Information(
                            $@"\/\/\/\/\/\/\/\/\/\/\/\/ RUN {currentRun} STARTED {remainingRuns} retry(s) left /\/\/\/\/\/\/\/\/\/\/\/\\/"
                            + Environment.NewLine);

                        if (beforeRunHook != default)
                        {
                            try
                            {
                                logger.Information(
                                    @"---------------------- EXECUTING BEFORE RUN HOOK ----------------------"
                                    + Environment.NewLine);

                                await beforeRunHook.Function.Invoke(
                                    new DataStore(
                                        context.DataStore.DocumentRepository,
                                        dataStoreOptions: beforeRunHook.RunHookUnitOfWorkId.HasValue
                                                              ? DataStoreOptions.Create()
                                                                                .SpecifyUnitOfWorkId(
                                                                                    beforeRunHook.RunHookUnitOfWorkId.Value)
                                                              : null),
                                    currentRun);
                            }
                            catch (Exception e)
                            {
                                logger.Information(Environment.NewLine + e + Environment.NewLine);

                                logger.Information(
                                    Environment.NewLine
                                    + $@"\/\/\/\/\/\/\/\/\/\/\/\/  RUN {currentRun} ENDED in FAILURE, {remainingRuns} retry(s) left /\/\/\/\/\/\/\/\/\/\/\/\\/");
                                x.UnhandledError = e;
                                return x;
                            }
                        }

                        logger.Information(
                            @"---------------------- EXECUTING MESSAGE HANDLER ----------------------" + Environment.NewLine);

                        try
                        {
                            await MessagePipeline.Execute(message, meta, context);

                            x.Success = true;
                            /*  What we are primarily trying to achieve is to make sure that each execute sets the activeprocesstate,
                            if there is a statefulprocess handling the message, to the statefulprocess that it affects. 
                            Not being able to see directly into the pipeline makes this challenging,
                            and just looking at the list of processstates in play will not help as that will include items from previous
                            executes. what we will do is look at the debug messages created in the bus for THIS message and find if there 
                            are any statefulprocessstarted or continued events we can use to determine the id of the processtate to return. */

                            var statefulProcessLaunchedByThisMessage = messageAggregator.AllMessages
                                .Where(m => m.GetType().InheritsOrImplements(typeof(IAssociateProcessStateWithAMessage)))
                                .SingleOrDefault(
                                    m => ((IAssociateProcessStateWithAMessage)m).ByMessage == message.Headers.GetMessageId())
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
                                + $@"\/\/\/\/\/\/\/\/\/\/\/\/  RUN {currentRun} ENDED in SUCCESS, {remainingRuns} retry(s) left /\/\/\/\/\/\/\/\/\/\/\/\\/");
                            return x;
                        }
                        catch (Exception e)
                        {
                            logger.Information(
                                Environment.NewLine
                                + $@"\/\/\/\/\/\/\/\/\/\/\/\/  RUN {currentRun} ENDED in FAILURE, {remainingRuns} retry(s) left /\/\/\/\/\/\/\/\/\/\/\/\\/");

                            /* included these 3 lines later, not sure why I didn't at first
                             maybe it was to keep parallel structure with the output in azurefunctioncontext
                             but there these might be null if there is an error, while here they shoudl always
                             be set, which is of course ideal for testing */
                            x.MessageBus = bus;
                            x.DataStore = dataStore;
                            x.NotificationServer = notificationServer;

                            logger.Error(e, "Unhandled Error");
                            x.UnhandledError = e;
                        }

                        currentRun++;
                    }
                    while (remainingRuns > 0);
                }
                catch (Exception e)
                {
                    x.Success = false;
                    x.UnhandledError = e;
                    logger.Error(e, "Unhandled Error");
                }

                return x;
            }
            
            static void CreateMessageMeta(
                ApiMessage message,
                IdentityPermissions permissions,
                IUserProfile userProfile,
                out MessageMeta meta)
            {
                (DateTime receivedTime, long receivedTicks) timeStamp = (DateTime.UtcNow, StopwatchOps.GetStopwatchTimestamp());

                meta = new MessageMeta(timeStamp, permissions, userProfile);
            }
            
            static Task TestSchemeAuth<TUserProfile>(
                IBootstrapVariables bootstrapVariables,
                ApiMessage message,
                DataStore dataStore,
                ISecurityInfo securityInfo,
                string schemeValue,
                Action<IdentityPermissions> setPermissions,
                Action<IUserProfile> setProfile) where TUserProfile : class, IUserProfile, IAggregate, new()
            {
                var testIdentityId = AesOps.Decrypt(message.Headers.GetIdentityToken(), bootstrapVariables.EncryptionKey);
                Guard.Against(schemeValue != testIdentityId, "last scheme value should match decrypted id token");
                var testIdentity = TestIdentities.SingleOrDefault(t => t.UserProfile.id.ToString() == testIdentityId);
                if (testIdentity != null)
                {
                    setPermissions(testIdentity.IdentityPermissions);
                    setProfile(testIdentity.UserProfile);
                }
                setPermissions(null);
                setProfile(null);

                return Task.CompletedTask;
            }

            static void CreateAppConfig(byte retries, bool authEnabled, bool enableSlaWhenSecurityContextIsMissing, out TestConfig applicationConfig)
            {
                applicationConfig = new TestConfig
                {
                    /* i assume this wierd syntax sets only the numberofmessageretries
                     property and does not recreate the bussettings property */
                    BusSettings =
                    {
                        NumberOfApiMessageRetries = retries
                    },
                    AuthEnabled = authEnabled,
                    UseServiceLevelAuthorityInTheAbsenceOfASecurityContext = enableSlaWhenSecurityContextIsMissing
                };
            }

            static void CreateLogger(IMessageAggregator messageAggregator, ITestOutputHelper testOutputHelper, out ILogger logger)
            {
                var sink = new SerilogMessageAggregatorSink(messageAggregator);

                logger = new LoggerConfiguration().Enrich.WithProperty("Environment", "DomainTests")
                                                  .Enrich.WithExceptionDetails()
                                                  .Destructure.UsingAttributes()
                                                  .WriteTo.Sink(sink)
                                                  .WriteTo.TestOutput(testOutputHelper)
                                                  .CreateLogger();

                Log.Logger = logger; //set serilog default instance which is expected by most serilog plugins
            }

            static void CreateMessageAggregator(
                Action<MessageAggregatorForTesting> setup,
                out IMessageAggregator messageAggregator)
            {
                var messageAggregatorForTesting = new MessageAggregatorForTesting();
                setup?.Invoke(messageAggregatorForTesting);
                messageAggregator = messageAggregatorForTesting;
            }

            static void CreateNotificationServer(NotificationServer.Settings settings, out NotificationServer notificationServer)
            {
                notificationServer = settings.CreateServer();
            }

            static void CreateBusContext(
                IMessageAggregator messageAggregator,
                TestConfig applicationConfig,
                IBlobStorage blobStorage,
                out IBus busContext
                 )
            {
                busContext = applicationConfig.BusSettings.CreateBus(
                    messageAggregator,
                    blobStorage,
                    null,
                    () => AuthFunctions.GetServiceLevelAuthority(applicationConfig), applicationConfig);
                
            }

            static void CreateDataStore(
                IMessageAggregator messageAggregator,
                IDocumentRepository rollingRepo,
                DataStoreOptions dataStoreOptions,
                Guid unitOfWorkId,
                out DataStore dataStore)
            {
                dataStoreOptions = dataStoreOptions?.SpecifyUnitOfWorkId(unitOfWorkId)
                                   ?? DataStoreOptions.Create().SpecifyUnitOfWorkId(unitOfWorkId);

                dataStore = new DataStore(rollingRepo, messageAggregator, dataStoreOptions);
            }

            static void CreateBlobStorage(IMessageAggregator messageAggregator, out BlobStorage blobStorage)
            {
                blobStorage = new BlobStorage(new BlobStorage.Settings("fake-conn-string", messageAggregator));
            }
        }
    }
}
