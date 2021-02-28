namespace Soap.Api.Sample.Tests.Messages
{
    using System;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Models.Aggregates;
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
            Result.UnhandledError.Message.Should().Contain("cannot be null");

        }

    }
}
