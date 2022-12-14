//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.MessageFailedAllRetries
{
    using CircuitBoard;
    using FluentAssertions;
    using Soap.Interfaces.Messages;
    using Xunit;
    using Xunit.Abstractions;

    public class TestMessageFailedAllRetriesWithInvalidParams : Test
    {
        public TestMessageFailedAllRetriesWithInvalidParams(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            TestMessage(
                    new MessageFailedAllRetries()
                    {
                        
                    },
                    Identities.JohnDoeAllPermissions)
                .Wait();
        }

        [Fact]
        public void ItShouldThrowAnErrorWhenAllFieldsAreNotFilledOut()
        {
            Result.Success.Should().BeFalse();
            Result.UnhandledError.Should().BeOfType<CircuitException>();

        }

    }
}
