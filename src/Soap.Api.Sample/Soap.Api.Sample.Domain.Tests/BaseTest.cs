namespace Sample.Tests
{
    using DataStore.Interfaces.LowLevel;
    using Sample.Logic;
    using Soap.DomainTests;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Xunit.Abstractions;

    public partial class BaseTest
    {
        private readonly ITestOutputHelper output;

        private readonly DomainTest testContext = new DomainTest();

        protected BaseTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        protected DomainTest.Result Result { get; private set; } = new DomainTest.Result();


        protected void Add<T>(T aggregate) where T : Aggregate, new()
        {
            this.testContext.GetAdd(aggregate, this.output);
        }
        
        protected void Execute(ApiMessage msg, IApiIdentity identity)
        {
            Result = this.testContext.GetExecute(new MappingRegistration(), this.output)(msg, identity).Result;
        }
    }
}