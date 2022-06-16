//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Tests.Messages.Commands.C105
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces.Messages;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC105v1 : Test
    {
        private readonly Guid id = Guid.NewGuid();

        public TestC105v1(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            TestMessage(
                    new C105v1_SendLargeMessage
                    {
                        C105_C106Id = this.id
                    },
                    Identities.JohnDoeAllPermissions)
                .Wait();
        }

        [Fact]
        public async void ItShouldSavMessageToBlobStorage()
        {
            (await Result.BlobStorage.Exists(this.id, "large-messages")).Should().BeTrue();
        }

        [Fact]
        public void ItShouldSendAMessageWhoseDataHasBeenSavedToBlobStorage()
        {
            Result.MessageBus.CommandsSent.Should().ContainSingle();
            Result.MessageBus.CommandsSent.Single().Should().BeOfType<C106v1_LargeCommand>();
            var sent = Result.MessageBus.CommandsSent.Single() as C106v1_LargeCommand;

            sent.Headers.GetBlobId().Should().Be(this.id);
            sent.C106_Large256KbString.Should().BeNull();
        }
    }
}