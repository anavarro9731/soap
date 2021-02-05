//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Tests.Messages
{
    using FluentAssertions;
    using Soap.Context;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC100v1WithBadAuth : Test
    {
        public TestC100v1WithBadAuth(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            TestMessage(Commands.Ping, Identities.JaneDoeNoPermissions).Wait();
        }

        [Fact]
        public void ItShouldFailIfAuthIsEnabledAndTheUserDoesNotHavePermissionsToSendThisCommand()
        {
            Result.Success.Should().BeFalse();
            Result.UnhandledError.Should().BeOfType<DomainExceptionWithErrorCode>();
            (Result.UnhandledError as DomainExceptionWithErrorCode).Error.Should()
                                                                   .Be(AuthErrorCodes.NoApiPermissionExistsForThisMessage);
        }
    }
}
