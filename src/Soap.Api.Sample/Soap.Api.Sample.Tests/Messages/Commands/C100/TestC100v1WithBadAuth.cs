//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Tests.Messages.Commands.C100
{
    using FluentAssertions;
    using Soap.Auth0;
    using Soap.Utility;
    using Xunit;
    using Xunit.Abstractions;
    using Commands = Soap.Api.Sample.Tests.Commands;

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
