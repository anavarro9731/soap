namespace Sample.Tests
{
    using System;
    using Sample.Logic;
    using Soap.DomainTests;
    using Soap.Interfaces;
    using Xunit.Abstractions;

    public partial class Test
    {
        protected DomainTest.Result Result;
        private readonly DomainTest testContext = new DomainTest();

        private readonly ITestOutputHelper output;

        public Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        public void Execute(ApiMessage msg, IApiIdentity identity)
        {
            if (msg.MessageId == Guid.Empty) msg.MessageId = Guid.NewGuid();
            this.Result = this.testContext.WireExecute(new Mappings(), this.output)(msg, identity).Result;
        }
    }
}