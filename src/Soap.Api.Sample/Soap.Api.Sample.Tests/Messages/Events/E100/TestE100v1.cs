//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Tests.Messages.Events.E100
{
    using FluentAssertions;
    using Soap.Idaam;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;
    using Events = Soap.Api.Sample.Tests.Events;

    public class TestE100v1 : Test
    {
        public TestE100v1(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            TestMessage(Events.E100v1_Pong, Identities.JaneDoeNoPermissions).Wait();
            
        }

        [Fact]
        public void ItShouldNotSucceedWithNoPermissions()
        {
            Result.UnhandledError.Should().BeOfType<DomainExceptionWithErrorCode>();
            Result.UnhandledError.DirectCast<DomainExceptionWithErrorCode>()
                  .Error.Should()
                  .Be(AuthErrorCodes.NoApiPermissionExistsForThisMessage);
        }
    }
}