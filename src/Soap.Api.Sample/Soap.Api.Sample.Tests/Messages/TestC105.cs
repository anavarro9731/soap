namespace Soap.Api.Sample.Tests.Messages
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Context.BlobStorage;
    using Soap.Interfaces.Messages;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC105 : Test
    {
        public TestC105(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            SetupTestByProcessingAMessage(
                new C105v1_SendLargeMessage(), 
                Identities.UserOne,
                setup: messageAggregatorForTesting => messageAggregatorForTesting.When<BlobStorage.Events.BlobUploadEvent>()
                                                                                 .Return(Task.CompletedTask));
        }

        [Fact]
        public void ItShouldSendAMessageWhoseDataHasBeenSavedToBlobStorage()
        {
            Result.MessageBus.CommandsSent.Should().ContainSingle();
            Result.MessageBus.CommandsSent.Single().Should().BeOfType<C106v1_LargeCommand>();
            var sent = Result.MessageBus.CommandsSent.Single() as C106v1_LargeCommand;
            sent.Headers.GetBlobId().Should().Be(sent.Headers.GetMessageId());
            sent.C106_Large256KbString.Should().BeNull();
        }
    }
}