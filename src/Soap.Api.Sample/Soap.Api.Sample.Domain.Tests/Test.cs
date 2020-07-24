namespace Sample.Tests
{
    using System;
    using Sample.Logic;
    using Soap.DomainTests;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Xunit.Abstractions;

    public partial class Test
    {
        private readonly ITestOutputHelper output;

        private readonly DomainTest testContext = new DomainTest();

        public Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        protected DomainTest.Result Result { get; private set; } = new DomainTest.Result();

        public void Execute(ApiMessage msg, IApiIdentity identity)
        {
            msg.Headers.EnsureRequiredHeaders();

            Result = this.testContext.GetExecute(new MappingRegistration(), this.output)(msg, identity).Result;
        }
    }
}