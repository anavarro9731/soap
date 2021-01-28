namespace Soap.Api.Sample.Tests.Messages
{
    using FluentAssertions;
    using Soap.Context;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC100v1WithDisabledAuth : Test
    {
        public TestC100v1WithDisabledAuth(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            TestMessage(Commands.Ping, Identities.UserOne.Op(x => x.ApiIdentity.ApiPermissions.Clear()), authEnabled:false).Wait();
        }

        [Fact]
        public void ItShouldConsiderTheCallersPermissionsAndFail()
        {
            Result.Success.Should().BeTrue();
        }
    }
}
