//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Tests.Messages.Commands.C100
{
    using System.Linq;
    using FluentAssertions;
    using Soap.Idaam;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;
    using Commands = Soap.Api.Sample.Tests.Commands;

    public class TestC100v1WithApiPermissionsOnly : Test
    {
        public TestC100v1WithApiPermissionsOnly(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            TestMessage(Commands.Ping, Identities.JaneDoeNoPermissions, authLevel: AuthLevel.AuthoriseApiPermissions).Wait();
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
