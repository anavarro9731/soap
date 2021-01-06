﻿//passes handler registration down
namespace Soap.Api.Sample.Tests
{
    using Soap.Api.Sample.Logic;
    using Soap.PfBase.Tests;
    using Xunit.Abstractions;

    public class Test : SoapMessageTest
    {
        protected Test(ITestOutputHelper output)
            : base(output, new HandlerRegistration())
        {
        }
    }
}
