#nullable enable
namespace Soap.PfBase.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Options;
    using Soap.Auth0;
    using Soap.Context.BlobStorage;
    using Soap.Context.Logging;
    using Soap.Context.MessageMapping;
    using Soap.Context.UnitOfWork;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;
    using Xunit.Abstractions;

    public class SoapMessageTest
    {
        private readonly MapMessagesToFunctions mappingRegistration;

        private readonly ISecurityInfo securityInfo;

        private readonly ITestOutputHelper output;

        private readonly SoapMessageTestContext soapTestContext = new SoapMessageTestContext();

        //* copies over each call of Add and Execute retaining state across the whole test
        private IDocumentRepository rollingRepo = new TestConfig().DatabaseSettings.CreateRepository();

        private InMemoryBlobStorage rollingStorage = new InMemoryBlobStorage(new InMemoryBlobStorage.Settings());

        private IMessageAggregator messageAggregatorForTesting = new MessageAggregatorForTesting();
        
        protected SoapMessageTest(
            ITestOutputHelper output,
            MapMessagesToFunctions mappingRegistration,
            List<TestIdentity> testIdentities,
            ISecurityInfo securityInfo)
        {
            this.output = output;
            this.mappingRegistration = mappingRegistration;
            this.securityInfo = securityInfo;
            SoapMessageTestContext.TestIdentities = testIdentities;
        }

        protected Result? Result { get; private set; }
        
        protected void SetupTestByAddingABlobStorageEntry(Blob blob, string containerName) 
        {
            
            this.output.WriteLine($"Test Setup: Stored Blob of type {blob.Type.TypeString}");

            this.rollingStorage.Upload(new InMemoryBlobStorage.Events.BlobUploadEvent(new InMemoryBlobStorage.Settings(), blob, containerName));
        }

        protected class CommonContainerNames : TypedEnumeration<CommonContainerNames>
        {
            public static CommonContainerNames LargeMessages = Create("large-messages", "large-messages");
            public static CommonContainerNames Content = Create("content", "content");
            public static CommonContainerNames UnitsOfWork = Create("units-of-work", "units-of-work");
        }
        protected void SetupTestByAddingABlobStorageEntry(Blob blob, CommonContainerNames containerName) 
        {
            SetupTestByAddingABlobStorageEntry(blob, containerName.Key);
        }
        
        protected void SetupTestByAddingADatabaseEntry<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate, new()
        {
            
            var dataStore = new DataStore(this.rollingRepo, messageAggregatorForTesting);

            this.output.WriteLine($"Test Setup: Created {typeof(TAggregate).Name}");

            dataStore.Create(aggregate).Wait();
            dataStore.CommitChanges().Wait();
        }

        protected void SetupTestByProcessingAMessage<TMessage>(
            TMessage msg,
            TestIdentity identity,
            Action<MessageAggregatorForTesting>? setup = null,
            bool authEnabled = false,
            bool enableSlaWhenSecurityContextIsMissing = true) where TMessage : ApiMessage
        {
            this.output.WriteLine($"Test Setup: Received Message {typeof(TMessage).Name}");
            
            /* unlike blobstorage and datastore there is no need to keep the bus state from previous messages
             but you do need to make sure any state they create in datastore or blobs is retained */
            Result = ExecuteMessage(
                    msg,
                    identity,
                    0,
                    setup: setup,
                    authEnabled: authEnabled,
                    enableSlaWhenSecurityContextIsMissing: enableSlaWhenSecurityContextIsMissing)
                .Result;
            if (Result.Success == false) throw Result.UnhandledError;
        }

        protected async Task TestMessage<TMessage>(
            TMessage msg,
            TestIdentity identity,
            byte retries = 0,
            (Func<DataStore, IBlobStorage, int, Task> beforeRunHook, Guid? runHookUnitOfWorkId) beforeRunHook = default,
            DataStoreOptions? dataStoreOptions = null,
            Action<MessageAggregatorForTesting>? setupMocks = null,
            bool authEnabled = true,
            bool enableSlaWhenSecurityContextIsMissing = false) where TMessage : ApiMessage
        {
            Result = await ExecuteMessage(
                         msg,
                         identity,
                         retries,
                         beforeRunHook,
                         dataStoreOptions,
                         setupMocks,
                         authEnabled,
                         enableSlaWhenSecurityContextIsMissing);
        }

        private async Task<Result> ExecuteMessage<TMessage>(
            TMessage msg,
            TestIdentity identity,
            byte retries,
            (Func<DataStore, IBlobStorage, int, Task> beforeRunHook, Guid? runHookUnitOfWorkId) beforeRunHook = default,
            DataStoreOptions? dataStoreOptions = null,
            Action<MessageAggregatorForTesting>? setup = null,
            bool authEnabled = true,
            bool enableSlaWhenSecurityContextIsMissing = false) where TMessage : ApiMessage
        {
            msg = msg.Clone(); //* ensure changes to this after this call cannot affect the call, that includes previous runs affecting retries or calling test code

            if (identity != null && msg is ApiCommand)
            {
                msg.Headers.SetIdentityChain(identity.IdChainSegment);
                msg.Headers.SetIdentityToken(identity.IdToken(new TestConfig().EncryptionKey));
                msg.Headers.SetAccessToken(identity.AccessToken);
            }

            msg.SetDefaultHeadersForIncomingTestMessages();

            return await this.soapTestContext.Execute(
                       msg,
                       this.mappingRegistration,
                       this.output,
                       this.securityInfo,
                       retries,
                       authEnabled,
                       enableSlaWhenSecurityContextIsMissing,
                       this.rollingStorage,
                       this.rollingRepo,
                       beforeRunHook,
                       dataStoreOptions,
                       setup);
        }
    }
}
