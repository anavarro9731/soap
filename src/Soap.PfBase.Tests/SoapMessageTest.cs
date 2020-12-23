#nullable enable
namespace Soap.PfBase.Tests
{
    using System;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Options;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.Utility.Functions.Extensions;
    using Xunit.Abstractions;

    public class SoapMessageTest
    {
        private readonly MapMessagesToFunctions mappingRegistration;

        private readonly ITestOutputHelper output;

        private readonly SoapMessageTestContext soapTestContext = new SoapMessageTestContext();

        //* copies over each call of Add and Execute retaining state across the whole test
        private IDocumentRepository? rollingRepo;

        protected SoapMessageTest(ITestOutputHelper output, MapMessagesToFunctions mappingRegistration)
        {
            this.output = output;
            this.mappingRegistration = mappingRegistration;
        }

        protected Result? Result { get; private set; }

        protected void SetupTestByAddingADatabaseEntry<T>(T aggregate) where T : Aggregate, new()
        {
            var dataStore = new DataStore(
                this.rollingRepo ??= new TestConfig().DatabaseSettings.CreateRepository(),
                new MessageAggregatorForTesting());

            this.output.WriteLine($"Created {typeof(T).Name}");

            dataStore.Create(aggregate).Wait();
            dataStore.CommitChanges().Wait();
        }

        protected void SetupTestByProcessingAMessage<T>(T msg, IApiIdentity identity, Action<MessageAggregatorForTesting>? setup = null)
            where T : ApiMessage
        {
            Result = ExecuteMessage(msg, identity, 0, setup:setup).Result;
            if (Result.Success == false) throw Result.UnhandledError;
        }

        protected async Task TestMessage<T>(
            T msg,
            IApiIdentity identity,
            byte retries,
            (Func<DataStore, int, Task> beforeRunHook, Guid? runHookUnitOfWorkId) beforeRunHook = default,
            DataStoreOptions? dataStoreOptions = null,
            Action<MessageAggregatorForTesting>? setup = null) where T : ApiMessage
        {
            Result = await ExecuteMessage(msg, identity, retries, beforeRunHook, dataStoreOptions, setup);
        }

        private async Task<Result> ExecuteMessage<T>(
            T msg,
            IApiIdentity identity,
            byte retries,
            (Func<DataStore, int, Task> beforeRunHook, Guid? runHookUnitOfWorkId) beforeRunHook = default,
            DataStoreOptions? dataStoreOptions = null,
            Action<MessageAggregatorForTesting>? setup = null) where T : ApiMessage
        {
            msg = msg.Clone(); //* ensure changes to this after this call cannot affect the call, that includes previous runs affecting retries or calling test code
            msg.SetDefaultHeadersForIncomingTestMessages();

            this.rollingRepo ??= new TestConfig().DatabaseSettings.CreateRepository();

            return await this.soapTestContext.Execute(
                         msg,
                         this.mappingRegistration,
                         this.output,
                         identity,
                         retries,
                         this.rollingRepo,
                         beforeRunHook,
                         dataStoreOptions,
                         setup);
        }
    }
}
