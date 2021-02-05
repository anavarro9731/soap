#nullable enable
namespace Soap.PfBase.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Options;
    using Soap.Api.Sample.Tests;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessageAggregator;
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
        private IDocumentRepository? rollingRepo;

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

        protected void SetupTestByAddingADatabaseEntry<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate, new()
        {
            var dataStore = new DataStore(
                this.rollingRepo ??= new TestConfig().DatabaseSettings.CreateRepository(),
                new MessageAggregatorForTesting());

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
            (Func<DataStore, int, Task> beforeRunHook, Guid? runHookUnitOfWorkId) beforeRunHook = default,
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
            (Func<DataStore, int, Task> beforeRunHook, Guid? runHookUnitOfWorkId) beforeRunHook = default,
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

            this.rollingRepo ??= new TestConfig().DatabaseSettings.CreateRepository();

            return await this.soapTestContext.Execute(
                       msg,
                       this.mappingRegistration,
                       this.output,
                       this.securityInfo,
                       retries,
                       authEnabled,
                       enableSlaWhenSecurityContextIsMissing,
                       this.rollingRepo,
                       beforeRunHook,
                       dataStoreOptions,
                       setup);
        }
    }
}
