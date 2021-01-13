//* ##REMOVE-IN-COPY##
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

    public class TestC105v1 : Test
    {
        public TestC105v1(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            
            
            TestMessage(
                new C105v1_SendLargeMessage(), 
                Identities.UserOne,
                setupMocks: messageAggregatorForTesting =>
                    {
                    messageAggregatorForTesting.When<BlobStorage.Events.BlobGetSasTokenEvent>().Return("fake-token");
                    messageAggregatorForTesting.When<BlobStorage.Events.BlobUploadEvent>().Return(Task.CompletedTask);
                    }).Wait();
        }

        [Fact]
        public void ItShouldSendAMessageWhoseDataHasBeenSavedToBlobStorage()
        {
            Result.MessageBus.CommandsSent.Should().ContainSingle();
            Result.MessageBus.CommandsSent.Single().Should().BeOfType<C106v1_LargeCommand>();
            var sent = Result.MessageBus.CommandsSent.Single() as C106v1_LargeCommand;
            
            sent.Headers.GetBlobId().Should().Be(sent.Headers.GetBlobId());
            sent.C106_Large256KbString.Should().BeNull();
        }
    }
}
