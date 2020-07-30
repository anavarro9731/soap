namespace Sample.Tests
{
    using System;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces.LowLevel;
    using Soap.DomainTests;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.Utility.Functions.Extensions;
    using Xunit.Abstractions;

    public class BaseTest
    {
        private readonly ITestOutputHelper output;

        private readonly MapMessagesToFunctions mappingRegistration;

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

        protected async Task ExecuteWithRetries<T>(T msg, IApiIdentity identity, int retries, Func<DataStore, int, Task> beforeRunHook = null)
            where T : ApiMessage
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
                    if(beforeRunHook != null) await beforeRunHook.Invoke(new DataStore(this.testContext.rollingStore.DocumentRepository), run);
                    Result = await this.testContext.GetExecute(this.mappingRegistration, this.output, retries)(msg, identity);
                }
                catch (Exception)
                {
                    var lastRun = availableRuns == 1;
                    if (lastRun) throw;
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