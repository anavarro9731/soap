namespace Sample.Tests
{
    using System;
    using Sample.Logic;
    using Soap.DomainTests;
    using Soap.Interfaces;
    using Xunit.Abstractions;

    public partial class Test
    {
        private readonly ITestOutputHelper output;

        private readonly DomainTest testContext = new DomainTest();

        public Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        protected DomainTest.Result Result { get; private set; }

        public void Execute(ApiMessage msg, IApiIdentity identity)
        {
            if (msg.MessageId == Guid.Empty) msg.MessageId = Guid.NewGuid();
            Result = this.testContext.WireExecute(new Mappings(), this.output)(msg, identity).Result;
        }
    }
}