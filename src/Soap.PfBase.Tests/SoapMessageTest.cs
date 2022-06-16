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
    using Soap.Client;
    using Soap.Context.BlobStorage;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.Utility.Functions.Extensions;
    using Xunit.Abstractions;

    public class SoapMessageTest
    {
        private readonly MapMessagesToFunctions mappingRegistration;

        private readonly IMessageAggregator messageAggregatorForTesting = new MessageAggregatorForTesting();

        private readonly ITestOutputHelper output;

        //* copies over each call of Add and Execute retaining state across the whole test
        private readonly IDocumentRepository rollingRepo;

        private readonly InMemoryBlobStorage rollingStorage;

        private readonly ISecurityInfo securityInfo;

        private readonly SoapMessageTestContext soapTestContext = new SoapMessageTestContext();

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

            this.rollingRepo = new TestConfig().DatabaseSettings.CreateRepository();
            this.rollingStorage = new InMemoryBlobStorage(new InMemoryBlobStorage.Settings(this.messageAggregatorForTesting));
        }

        protected Result? Result { get; private set; }

        protected void SetupTestByAddingABlobStorageEntry(Blob blob, string containerName)
        {
            this.output.WriteLine($"Test Setup: Stored Blob of type {blob.Type.TypeString}");

            this.rollingStorage.Upload(
                new InMemoryBlobStorage.Events.BlobUploadEvent(
                    new InMemoryBlobStorage.Settings(this.messageAggregatorForTesting),
                    blob,
                    containerName));
        }

        protected void SetupTestByAddingABlobStorageEntry(Blob blob, CommonContainerNames containerName)
        {
            SetupTestByAddingABlobStorageEntry(blob, containerName.Key);
        }

        protected void SetupTestByAddingADatabaseEntry<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate, new()
        {
            var dataStore = new DataStore(this.rollingRepo, this.messageAggregatorForTesting);

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
            bool enableSlaWhenSecurityContextIsMissing = false,
            Transport clientTransport = Transport.ServiceBus) where TMessage : ApiMessage
        {
            Result = await ExecuteMessage(
                         msg,
                         identity,
                         retries,
                         beforeRunHook,
                         dataStoreOptions,
                         setupMocks,
                         authEnabled,
                         enableSlaWhenSecurityContextIsMissing,
                         clientTransport);
        }

        private async Task<Result> ExecuteMessage<TMessage>(
            TMessage msg,
            TestIdentity identity,
            byte retries,
            (Func<DataStore, IBlobStorage, int, Task> beforeRunHook, Guid? runHookUnitOfWorkId) beforeRunHook = default,
            DataStoreOptions? dataStoreOptions = null,
            Action<MessageAggregatorForTesting>? setup = null,
            bool authEnabled = true,
            bool enableSlaWhenSecurityContextIsMissing = false,
            Transport clientTransport = Transport.ServiceBus) where TMessage : ApiMessage
        {
            msg = msg.Clone(); //* ensure changes to this after this call cannot affect the call, that includes previous runs affecting retries or calling test code

            if (msg is ApiCommand && identity != null && msg.Headers.GetIdentityChain() == null)
            {
                msg.Headers.SetIdentityChain(identity.IdChainSegment);
                msg.Headers.SetIdentityToken(identity.IdToken(new TestConfig().EncryptionKey));
                msg.Headers.SetAccessToken(identity.AccessToken);
            }

            msg.SetDefaultHeadersForIncomingTestMessages();

            if (msg is ApiCommand && clientTransport == Transport.ServiceBus
                                  && msg.ToBlob().Bytes.Length > 256000)
            {
                //* simulate what the js or soap clients would do
                msg.Headers.SetBlobId(msg.Headers.GetMessageId());
                SetupTestByAddingABlobStorageEntry(msg.ToBlob(), CommonContainerNames.LargeMessages);
                msg.ClearAllPublicPropertyValuesExceptHeaders();
            }

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

        protected class CommonContainerNames : TypedEnumeration<CommonContainerNames>
        {
            public static CommonContainerNames Content = Create("content", "content");

            public static CommonContainerNames LargeMessages = Create("large-messages", "large-messages");

            public static CommonContainerNames UnitsOfWork = Create("units-of-work", "units-of-work");
        }
    }
}