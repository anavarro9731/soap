#nullable enable
namespace Sample.Tests
{
    using System;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.PfBase.Tests;
    using Soap.Utility.Functions.Extensions;
    using Xunit.Abstractions;

    public class SoapMessageTest
    {
        private readonly MapMessagesToFunctions mappingRegistration;

        private readonly ITestOutputHelper output;

        //* copies over each call of Add and Execute retaining state across the whole test
        private IDocumentRepository rollingRepo;

        private readonly SoapMessageTestContext soapTestContext = new SoapMessageTestContext();

        protected SoapMessageTest(ITestOutputHelper output, MapMessagesToFunctions mappingRegistration)
        {
            this.output = output;
            this.mappingRegistration = mappingRegistration;
        }

        protected Result Result { get; private set; } 

        protected void Add<T>(T aggregate) where T : Aggregate, new()
        {
            var dataStore = new DataStore(
                this.rollingRepo ??= new TestConfig().DatabaseSettings.CreateRepository(),
                new MessageAggregatorForTesting());

            this.output.WriteLine($"Created {typeof(T).Name}");

            dataStore.Create(aggregate).Wait();
            dataStore.CommitChanges().Wait();
        }

        protected void Execute<T>(T msg, IApiIdentity identity) where T : ApiMessage
        {
            TestMessage(msg, identity, 0).Wait();
            if (Result.Success == false) throw Result.UnhandledError;
        }

        protected async Task TestMessage<T>(
            T msg,
            IApiIdentity identity,
            byte retries,
            Func<DataStore, int, Task> beforeRunHook = null,
            Guid? runHookUnitOfWorkId = null) where T : ApiMessage
        {
            msg = msg.Clone(); //* ensure changes to this after this call cannot affect the call, that includes previous runs affecting retries or calling test code

            this.rollingRepo ??= new TestConfig().DatabaseSettings.CreateRepository();
            
            Result = await this.soapTestContext.Execute(
                         msg,
                         this.mappingRegistration,
                         this.output,
                         identity,
                         retries,
                         this.rollingRepo,
                         beforeRunHook,
                         runHookUnitOfWorkId);
        }
    }
}