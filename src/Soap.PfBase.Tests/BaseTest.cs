#nullable enable
namespace Sample.Tests
{
    using System;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Options;
    using Soap.PfBase.Tests;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.Utility.Functions.Extensions;
    using Xunit.Abstractions;

    public class BeforeHookException : Exception
    {
        public BeforeHookException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
    
    public class BaseTest
    {
        private readonly MapMessagesToFunctions mappingRegistration;

        private readonly ITestOutputHelper output;

        private readonly DomainTest testContext = new DomainTest();

        protected BaseTest(ITestOutputHelper output, MapMessagesToFunctions mappingRegistration)
        {
            this.output = output;
            this.mappingRegistration = mappingRegistration;
        }

        protected DomainTest.Result Result { get; private set; } = new DomainTest.Result();

        protected void Add<T>(T aggregate) where T : Aggregate, new()
        {
            this.testContext.GetAdd(aggregate, this.output);
        }

        protected void Execute<T>(T msg, IApiIdentity identity) where T : ApiMessage
        {
            msg = msg.Clone(); //make sure changes to this after this call cannot affect the call

            this.output.WriteLine("OUTPUT LOG...");
            Result = this.testContext.GetExecute(this.mappingRegistration, this.output, 0)(msg, identity).Result;
        }

        protected async Task ExecuteWithRetries<T>(
            T msg,
            IApiIdentity identity,
            byte retries,
            Func<DataStore, int, Task> beforeRunHook = null, Guid? runHookUnitOfWorkId = null) where T : ApiMessage
        {
            msg = msg.Clone(); //make sure changes to this after this call cannot affect the call, that includes previous runs affecting retries or calling test code

            var initialRun = 1;
            var availableRuns = retries + initialRun;
            var run = 1;

            this.output.WriteLine("OUTPUT LOG...");

            while (availableRuns > 0)
            {
                this.output.WriteLine(
                    $@"\/\/\/\/\/\/\/\/\/\/\/\/ RUN {run} STARTED {availableRuns - 1} left /\/\/\/\/\/\/\/\/\/\/\/\\/"
                    + Environment.NewLine);
                try
                {
                    if (beforeRunHook != null)
                    {
                        this.output.WriteLine(
                            $@"---------------------- EXECUTING BEFORE RUN HOOK ----------------------"
                            + Environment.NewLine);
                        try
                        {
                            await beforeRunHook.Invoke(
                                new DataStore(
                                    this.testContext.rollingStore.DocumentRepository,
                                    dataStoreOptions: runHookUnitOfWorkId.HasValue
                                                          ? DataStoreOptions
                                                            .Create()
                                                            .SpecifyUnitOfWorkId(runHookUnitOfWorkId.Value)
                                                          : null),
                                run);
                        }
                        catch (Exception bhe)
                        {
                            throw new BeforeHookException("Error executing before hook", bhe);
                        }
                    }

                    this.output.WriteLine(
                             $@"---------------------- EXECUTING MESSAGE HANDLER ----------------------" + Environment.NewLine);
                    Result = await this.testContext.GetExecute(this.mappingRegistration, this.output, retries)(msg, identity);
                }
                catch (Exception e)
                {
                    var lastRun = availableRuns == 1;
                    if (lastRun || e is BeforeHookException)
                    {
                        this.output.WriteLine(Environment.NewLine + e + Environment.NewLine);
                        throw;
                    }
                }

                this.output.WriteLine(
                    Environment.NewLine
                    + $@"\/\/\/\/\/\/\/\/\/\/\/\/  RUN {run} ENDED {availableRuns - 1} left /\/\/\/\/\/\/\/\/\/\/\/\\/");
                availableRuns--;
                run++;
            }
        }
    }
}